import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import ActivityForm from '../components/activities/ActivityForm';

// Mock the API calls
const mockActivitiesAPI = {
  getActivityWithDetails: jest.fn(),
  createActivity: jest.fn(),
  updateActivity: jest.fn(),
  uploadActivityVideo: jest.fn(),
  getActivityVideoUrl: jest.fn(),
};

const mockStepsAPI = {
  createEnhancedStep: jest.fn(),
  updateEnhancedStep: jest.fn(),
};

const mockActivityProductVariantsAPI = {
  getActivityProductVariantsByActivityId: jest.fn(),
};

jest.mock('../services/api', () => ({
  activitiesAPI: mockActivitiesAPI,
  stepsAPI: mockStepsAPI,
  activityProductVariantsAPI: mockActivityProductVariantsAPI,
}));

// Mock Redux hooks
const mockUseAppSelector = jest.fn();
const mockUseAppDispatch = jest.fn();

jest.mock('../store/hooks', () => ({
  useAppDispatch: () => mockUseAppDispatch,
  useAppSelector: mockUseAppSelector,
}));

// Mock VideoUtils
jest.mock('../services/videoUtils', () => ({
  VideoUtils: {
    extractObjectKeyFromSignedUrl: jest.fn(),
    isMinioObjectKey: jest.fn(),
  },
}));

describe('ActivityForm Tabs', () => {
  const mockCategories = [
    { id: '1', name: 'Category 1', description: 'Test category' },
  ];

  const mockProps = {
    activity: null,
    categories: mockCategories,
    onSuccess: jest.fn(),
    onCancel: jest.fn(),
  };

  beforeEach(() => {
    jest.clearAllMocks();
    // Mock the selectors to return empty arrays and false for loading
    mockUseAppSelector.mockImplementation((selector) => {
      if (selector.name === 'selectStepsForActivity') return [];
      if (selector.name === 'selectActivityErrors') return [];
      if (selector.name === 'selectActivityLoading') return false;
      return [];
    });
  });

  test('renders tabs correctly', async () => {
    render(<ActivityForm {...mockProps} />);

    // Wait for loading to complete
    await waitFor(() => {
      expect(screen.getByText('Basic Information')).toBeInTheDocument();
    });

    // Check if tabs are rendered
    expect(screen.getByText('Video & Steps')).toBeInTheDocument();
  });

  test('shows Basic Information tab by default', async () => {
    render(<ActivityForm {...mockProps} />);

    // Wait for loading to complete
    await waitFor(() => {
      expect(screen.getByText('Basic Information')).toBeInTheDocument();
    });

    // Check if Basic Information content is visible
    expect(screen.getByLabelText('Activity Name')).toBeInTheDocument();
    expect(screen.getByLabelText('Description')).toBeInTheDocument();
    expect(screen.getByText('Hours')).toBeInTheDocument();
    expect(screen.getByText('Minutes')).toBeInTheDocument();
    expect(screen.getByText('Seconds')).toBeInTheDocument();
  });

  test('switches to Video & Steps tab when clicked', async () => {
    render(<ActivityForm {...mockProps} />);

    // Wait for loading to complete
    await waitFor(() => {
      expect(screen.getByText('Basic Information')).toBeInTheDocument();
    });

    // Click on Video & Steps tab
    fireEvent.click(screen.getByText('Video & Steps'));

    // Check if Video & Steps content is visible
    expect(screen.getByText('Upload Video')).toBeInTheDocument();
    expect(screen.getByText('Upload Intro Video')).toBeInTheDocument();
  });

  test('switches back to Basic Information tab when clicked', async () => {
    render(<ActivityForm {...mockProps} />);

    // Wait for loading to complete
    await waitFor(() => {
      expect(screen.getByText('Basic Information')).toBeInTheDocument();
    });

    // Click on Video & Steps tab first
    fireEvent.click(screen.getByText('Video & Steps'));

    // Then click back to Basic Information
    fireEvent.click(screen.getByText('Basic Information'));

    // Check if Basic Information content is visible again
    expect(screen.getByLabelText('Activity Name')).toBeInTheDocument();
  });
});
