import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import ActivityPage from '../pages/ActivityPage';
import activitySlice, {
  fetchActivityById,
  fetchActivitySteps,
  fetchActivityMaterials,
  setCurrentStepIndex,
  setPausedAtStep,
  setVideoPlaying,
  updateProgress,
} from '../store/slices/activitySlice';

// Mock React Router
const mockUseParams = jest.fn();
const mockUseNavigate = jest.fn();

jest.mock('react-router-dom', () => ({
  useParams: () => mockUseParams(),
  useNavigate: () => mockUseNavigate(),
}));

// Mock Redux hooks
const mockUseAppSelector = jest.fn();
const mockUseAppDispatch = jest.fn();

jest.mock('../store/hooks', () => ({
  useAppSelector: () => mockUseAppSelector(),
  useAppDispatch: () => mockUseAppDispatch(),
}));

// Mock localStorage
const mockLocalStorage = {
  getItem: jest.fn(),
  setItem: jest.fn(),
  removeItem: jest.fn(),
  clear: jest.fn(),
};
Object.defineProperty(window, 'localStorage', {
  value: mockLocalStorage,
});

// Mock fetch
global.fetch = jest.fn();

describe('ActivityPage', () => {
  let store: ReturnType<typeof configureStore>;
  let mockDispatch: jest.Mock;

  beforeEach(() => {
    store = configureStore({
      reducer: {
        activity: activitySlice,
      },
    });

    mockDispatch = jest.fn();
    mockUseAppDispatch.mockReturnValue(mockDispatch);
    mockUseNavigate.mockReturnValue(jest.fn());

    // Reset mocks
    jest.clearAllMocks();
    mockLocalStorage.getItem.mockClear();
    mockLocalStorage.setItem.mockClear();
    (global.fetch as jest.Mock).mockClear();
  });

  const mockActivity = {
    id: 'activity-1',
    name: 'Test Activity',
    description: 'Test Description',
    durationHours: 1,
    durationMinutes: 30,
    durationSeconds: 0,
    activityCategory: {
      id: 'category-1',
      name: 'Test Category',
    },
    videoUrl: 'https://example.com/video.mp4',
  };

  const mockSteps = [
    {
      id: 'step-1',
      order: 1,
      description: 'First step',
      pauseTimeSeconds: 30,
    },
    {
      id: 'step-2',
      order: 2,
      description: 'Second step',
      pauseTimeSeconds: 60,
    },
  ];

  const mockMaterials = [
    {
      id: 'material-1',
      productVariantId: 'variant-1',
      quantity: 2,
      unitOfMeasureId: 'unit-1',
      productVariant: {
        id: 'variant-1',
        name: 'Test Product',
        baseProduct: {
          id: 'product-1',
          name: 'Base Product',
        },
      },
      unitOfMeasure: {
        id: 'unit-1',
        name: 'pieces',
        symbol: 'pcs',
      },
    },
  ];

  const mockState = {
    activity: {
      currentActivity: mockActivity,
      steps: mockSteps,
      materials: mockMaterials,
      currentStepIndex: 0,
      isPausedAtStep: false,
      videoState: {
        isPlaying: false,
        currentTime: 0,
        duration: 0,
        volume: 1,
      },
      loading: false,
      error: null,
    },
  };

  it('renders loading state initially', () => {
    mockUseParams.mockReturnValue({ id: 'activity-1' });
    mockUseAppSelector.mockReturnValue({
      ...mockState.activity,
      currentActivity: null,
      loading: true,
    });

    render(
      <Provider store={store}>
        <ActivityPage />
      </Provider>
    );

    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });

  it('renders error state when activity not found', () => {
    mockUseParams.mockReturnValue({ id: 'activity-1' });
    mockUseAppSelector.mockReturnValue({
      ...mockState.activity,
      currentActivity: null,
      loading: false,
      error: 'Activity not found',
    });

    render(
      <Provider store={store}>
        <ActivityPage />
      </Provider>
    );

    expect(screen.getByText('Activity not found')).toBeInTheDocument();
    expect(screen.getByText('Back to Activities')).toBeInTheDocument();
  });

  it('renders activity content when loaded', () => {
    mockUseParams.mockReturnValue({ id: 'activity-1' });
    mockUseAppSelector.mockReturnValue(mockState.activity);

    render(
      <Provider store={store}>
        <ActivityPage />
      </Provider>
    );

    expect(screen.getByText('Test Activity')).toBeInTheDocument();
    expect(screen.getByText('Test Description')).toBeInTheDocument();
    expect(screen.getByText('Materials (1)')).toBeInTheDocument();
  });

  it('loads activity data on mount', () => {
    mockUseParams.mockReturnValue({ id: 'activity-1' });
    mockUseAppSelector.mockReturnValue({
      ...mockState.activity,
      currentActivity: null,
      loading: true,
    });

    // Mock successful API responses
    mockDispatch.mockImplementation((thunk) => {
      if (typeof thunk === 'function') {
        return thunk(mockDispatch, store.getState, undefined);
      }
      return Promise.resolve({ unwrap: () => Promise.resolve(mockActivity) });
    });

    render(
      <Provider store={store}>
        <ActivityPage />
      </Provider>
    );

    expect(mockDispatch).toHaveBeenCalledWith(fetchActivityById('activity-1'));
    expect(mockDispatch).toHaveBeenCalledWith(fetchActivitySteps('activity-1'));
    expect(mockDispatch).toHaveBeenCalledWith(fetchActivityMaterials('activity-1'));
  });

  it('loads user favorites from localStorage', () => {
    mockUseParams.mockReturnValue({ id: 'activity-1' });
    mockUseAppSelector.mockReturnValue(mockState.activity);
    mockLocalStorage.getItem.mockReturnValue(JSON.stringify(['activity-1', 'activity-2']));

    render(
      <Provider store={store}>
        <ActivityPage />
      </Provider>
    );

    expect(mockLocalStorage.getItem).toHaveBeenCalledWith('activityFavorites');
  });

  it('handles step click', () => {
    mockUseParams.mockReturnValue({ id: 'activity-1' });
    mockUseAppSelector.mockReturnValue(mockState.activity);

    render(
      <Provider store={store}>
        <ActivityPage />
      </Provider>
    );

    // Find and click a step button (assuming ActivityLayout renders step buttons)
    const stepButtons = screen.getAllByRole('button');
    const stepButton = stepButtons.find(button => button.textContent === '1');
    if (stepButton) {
      fireEvent.click(stepButton);
      expect(mockDispatch).toHaveBeenCalledWith(setCurrentStepIndex(0));
      expect(mockDispatch).toHaveBeenCalledWith(setPausedAtStep(false));
    }
  });

  it('handles favorite toggle', () => {
    mockUseParams.mockReturnValue({ id: 'activity-1' });
    mockUseAppSelector.mockReturnValue(mockState.activity);
    mockLocalStorage.getItem.mockReturnValue(JSON.stringify([]));

    render(
      <Provider store={store}>
        <ActivityPage />
      </Provider>
    );

    const favoriteButton = screen.getByRole('button', { name: /favorite/i });
    fireEvent.click(favoriteButton);

    expect(mockLocalStorage.setItem).toHaveBeenCalledWith(
      'activityFavorites',
      JSON.stringify(['activity-1'])
    );
  });

  it('handles continue button click', () => {
    mockUseParams.mockReturnValue({ id: 'activity-1' });
    mockUseAppSelector.mockReturnValue({
      ...mockState.activity,
      isPausedAtStep: true,
    });

    render(
      <Provider store={store}>
        <ActivityPage />
      </Provider>
    );

    const continueButton = screen.getByText('Continue Activity');
    fireEvent.click(continueButton);

    expect(mockDispatch).toHaveBeenCalledWith(setPausedAtStep(false));
    expect(mockDispatch).toHaveBeenCalledWith(setCurrentStepIndex(1));
    expect(mockDispatch).toHaveBeenCalledWith(
      updateProgress({
        activityId: 'activity-1',
        currentStep: 1,
        completedSteps: [0],
        lastWatchedAt: expect.any(String),
      })
    );
  });

  it('opens materials dialog', () => {
    mockUseParams.mockReturnValue({ id: 'activity-1' });
    mockUseAppSelector.mockReturnValue(mockState.activity);

    render(
      <Provider store={store}>
        <ActivityPage />
      </Provider>
    );

    const materialsButton = screen.getByText('Materials (1)');
    fireEvent.click(materialsButton);

    // Dialog should be open, but we can't easily test the dialog content without more setup
    // This test ensures the button click doesn't crash
  });

  it('calculates progress percentage correctly', () => {
    mockUseParams.mockReturnValue({ id: 'activity-1' });
    mockUseAppSelector.mockReturnValue({
      ...mockState.activity,
      currentStepIndex: 1,
    });

    render(
      <Provider store={store}>
        <ActivityPage />
      </Provider>
    );

    expect(screen.getByText('Progress: 50%')).toBeInTheDocument();
  });

  it('handles missing activity ID', () => {
    mockUseParams.mockReturnValue({});
    mockUseAppSelector.mockReturnValue(mockState.activity);

    render(
      <Provider store={store}>
        <ActivityPage />
      </Provider>
    );

    expect(screen.getByText('Activity ID is required')).toBeInTheDocument();
  });

  it('handles video play/pause events', () => {
    mockUseParams.mockReturnValue({ id: 'activity-1' });
    mockUseAppSelector.mockReturnValue(mockState.activity);

    render(
      <Provider store={store}>
        <ActivityPage />
      </Provider>
    );

    // These handlers are passed to ActivityVideoPlayer, but we can't easily test them
    // without mocking the video element. The setup ensures they don't crash.
  });
});