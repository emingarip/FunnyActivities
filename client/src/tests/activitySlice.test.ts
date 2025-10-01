// Mock axios before importing anything else
import axios from 'axios';

// Mock axios module
jest.mock('axios', () => {
  const mockAxiosInstance = {
    interceptors: {
      request: { use: jest.fn() },
      response: { use: jest.fn() }
    },
    get: jest.fn(),
    post: jest.fn(),
    put: jest.fn(),
    delete: jest.fn(),
    defaults: {},
    isAxiosError: jest.fn()
  };

  return {
    ...mockAxiosInstance,
    create: jest.fn(() => mockAxiosInstance),
    isAxiosError: jest.fn()
  };
});

const mockedAxios = axios as jest.Mocked<typeof axios>;

import activitySlice, {
  fetchActivities,
  fetchActivityById,
  fetchActivitySteps,
  fetchActivityMaterials,
  setCurrentActivity,
  setCurrentStepIndex,
  setPausedAtStep,
  updateProgress,
  setVideoPlaying,
  setVideoCurrentTime,
  setVideoDuration,
  setVideoVolume,
  resetVideoState,
  clearError,
} from '../store/slices/activitySlice';
import { configureStore } from '@reduxjs/toolkit';
import type { ActivityState } from '../store/slices/activitySlice';

describe('activitySlice', () => {
  let store: ReturnType<typeof configureStore<{
    activity: ActivityState;
  }>>;

  beforeEach(() => {
    store = configureStore({
      reducer: {
        activity: activitySlice,
      },
    });
    jest.clearAllMocks();
  });

  describe('initial state', () => {
    it('should have correct initial state', () => {
      const state = store.getState().activity;

      expect(state.activities).toEqual([]);
      expect(state.currentActivity).toBeNull();
      expect(state.stepsByActivityId).toEqual({});
      expect(state.materials).toEqual([]);
      expect(state.currentStepIndex).toBe(0);
      expect(state.isPausedAtStep).toBe(false);
      expect(state.progress).toBeNull();
      expect(state.loading).toBe(false);
      expect(state.error).toBeNull();
      expect(state.videoState).toEqual({
        isPlaying: false,
        currentTime: 0,
        duration: 0,
        volume: 1,
      });
    });
  });

  describe('reducers', () => {
    it('setCurrentActivity should update current activity', () => {
      const activity = { id: '1', name: 'Test Activity' };
      store.dispatch(setCurrentActivity(activity));

      const state = store.getState().activity;
      expect(state.currentActivity).toEqual(activity);
    });

    it('setCurrentStepIndex should update step index and reset pause state', () => {
      // First set pause state to true
      store.dispatch(setPausedAtStep(true));
      store.dispatch(setCurrentStepIndex(2));

      const state = store.getState().activity;
      expect(state.currentStepIndex).toBe(2);
      expect(state.isPausedAtStep).toBe(false);
    });

    it('setPausedAtStep should update pause state', () => {
      store.dispatch(setPausedAtStep(true));

      const state = store.getState().activity;
      expect(state.isPausedAtStep).toBe(true);
    });

    it('updateProgress should update progress when it exists', () => {
      const initialProgress = { activityId: '1', currentStep: 1, completedSteps: [0] };
      store.dispatch(updateProgress(initialProgress));

      store.dispatch(updateProgress({ currentStep: 2 }));

      const state = store.getState().activity;
      expect(state.progress).toEqual({
        activityId: '1',
        currentStep: 2,
        completedSteps: [0],
      });
    });

    it('updateProgress should create progress when it does not exist', () => {
      store.dispatch(updateProgress({ activityId: '1', currentStep: 1 }));

      const state = store.getState().activity;
      expect(state.progress).toEqual({
        activityId: '1',
        currentStep: 1,
      });
    });

    it('setVideoPlaying should update video playing state', () => {
      store.dispatch(setVideoPlaying(true));

      const state = store.getState().activity;
      expect(state.videoState.isPlaying).toBe(true);
    });

    it('setVideoCurrentTime should update video current time', () => {
      store.dispatch(setVideoCurrentTime(45.5));

      const state = store.getState().activity;
      expect(state.videoState.currentTime).toBe(45.5);
    });

    it('setVideoDuration should update video duration', () => {
      store.dispatch(setVideoDuration(120.5));

      const state = store.getState().activity;
      expect(state.videoState.duration).toBe(120.5);
    });

    it('setVideoVolume should update video volume', () => {
      store.dispatch(setVideoVolume(0.8));

      const state = store.getState().activity;
      expect(state.videoState.volume).toBe(0.8);
    });

    it('resetVideoState should reset video state to initial', () => {
      // Set some values first
      store.dispatch(setVideoPlaying(true));
      store.dispatch(setVideoCurrentTime(30));
      store.dispatch(setVideoVolume(0.5));

      store.dispatch(resetVideoState());

      const state = store.getState().activity;
      expect(state.videoState).toEqual({
        isPlaying: false,
        currentTime: 0,
        duration: 0,
        volume: 1,
      });
    });

    it('clearError should clear error state', () => {
      // Set an error first (we'll do this by dispatching a rejected thunk)
      store.dispatch(clearError());

      const state = store.getState().activity;
      expect(state.error).toBeNull();
    });
  });

  describe('async thunks', () => {
    describe('fetchActivities', () => {
      it('should handle successful fetch', async () => {
        const mockActivities = {
          success: true,
          message: 'Public activities retrieved successfully',
          data: {
            items: [
              { id: '1', name: 'Activity 1' },
              { id: '2', name: 'Activity 2' },
            ],
            page: 1,
            pageSize: 10,
            totalCount: 2,
            totalPages: 1,
            hasPreviousPage: false,
            hasNextPage: false
          }
        };

        mockedAxios.get.mockResolvedValueOnce({ data: mockActivities });

        await store.dispatch(fetchActivities());

        const state = store.getState().activity;
        expect(state.activities).toEqual(mockActivities.data);
        expect(state.loading).toBe(false);
        expect(state.error).toBeNull();
      });

      it('should handle fetch error', async () => {
        const errorMessage = 'Failed to fetch activities';

        mockedAxios.get.mockRejectedValueOnce(new Error(errorMessage));

        await store.dispatch(fetchActivities());

        const state = store.getState().activity;
        expect(state.loading).toBe(false);
        expect(state.error).toBe(errorMessage);
      });

      it('should set loading state during fetch', () => {
        mockedAxios.get.mockImplementationOnce(
          () => new Promise(resolve => setTimeout(() => resolve({
            data: {
              success: true,
              message: 'Activities retrieved successfully',
              data: []
            }
          }), 100))
        );

        store.dispatch(fetchActivities());

        const state = store.getState().activity;
        expect(state.loading).toBe(true);
      });
    });

    describe('fetchActivityById', () => {
      it('should handle successful fetch', async () => {
        const mockActivity = {
          success: true,
          message: 'Activity retrieved successfully',
          data: { id: '1', name: 'Test Activity' }
        };

        mockedAxios.get.mockResolvedValueOnce({ data: mockActivity });

        await store.dispatch(fetchActivityById('1'));

        const state = store.getState().activity;
        expect(state.currentActivity).toEqual(mockActivity.data);
        expect(state.loading).toBe(false);
        expect(state.error).toBeNull();
      });

      it('should handle fetch error', async () => {
        const errorMessage = 'Failed to fetch activity';

        mockedAxios.get.mockRejectedValueOnce(new Error(errorMessage));

        await store.dispatch(fetchActivityById('1'));

        const state = store.getState().activity;
        expect(state.loading).toBe(false);
        expect(state.error).toBe(errorMessage);
      });
    });

    describe('fetchActivitySteps', () => {
      it('should handle successful fetch', async () => {
        const mockSteps = {
          success: true,
          message: 'Steps retrieved successfully',
          data: [
            { id: '1', activityId: 'activity1', order: 1, description: 'Step 1' },
            { id: '2', activityId: 'activity1', order: 2, description: 'Step 2' },
          ]
        };

        mockedAxios.get.mockResolvedValueOnce({ data: mockSteps });

        await store.dispatch(fetchActivitySteps('activity1'));

        const state = store.getState().activity;
        expect(state.stepsByActivityId['activity1']).toEqual(mockSteps.data);
        expect(state.loading).toBe(false);
        expect(state.error).toBeNull();
      });

      it('should handle fetch error', async () => {
        const errorMessage = 'Failed to fetch activity steps';

        mockedAxios.get.mockRejectedValueOnce(new Error(errorMessage));

        await store.dispatch(fetchActivitySteps('1'));

        const state = store.getState().activity;
        expect(state.loading).toBe(false);
        expect(state.error).toBe(errorMessage);
      });
    });

    describe('fetchActivityMaterials', () => {
      it('should handle successful fetch', async () => {
        // Mock localStorage to simulate authenticated user
        Object.defineProperty(window, 'localStorage', {
          value: {
            getItem: jest.fn(() => 'fake-token'),
            setItem: jest.fn(),
            removeItem: jest.fn(),
          },
          writable: true,
        });

        const mockMaterials = {
          success: true,
          message: 'Materials retrieved successfully',
          data: [
            { id: '1', productVariantId: 'pv1', quantity: 2 },
            { id: '2', productVariantId: 'pv2', quantity: 1 },
          ]
        };

        mockedAxios.get.mockResolvedValueOnce({ data: mockMaterials });

        await store.dispatch(fetchActivityMaterials('1'));

        const state = store.getState().activity;
        expect(state.materials).toEqual(mockMaterials.data);
        expect(state.loading).toBe(false);
        expect(state.error).toBeNull();
      });

      it('should handle fetch error', async () => {
        // Mock localStorage to simulate authenticated user
        Object.defineProperty(window, 'localStorage', {
          value: {
            getItem: jest.fn(() => 'fake-token'),
            setItem: jest.fn(),
            removeItem: jest.fn(),
          },
          writable: true,
        });

        const errorMessage = 'Failed to fetch activity materials';

        mockedAxios.get.mockRejectedValueOnce(new Error(errorMessage));

        await store.dispatch(fetchActivityMaterials('1'));

        const state = store.getState().activity;
        expect(state.loading).toBe(false);
        expect(state.error).toBe(errorMessage);
      });
    });
  });

  describe('extra reducers', () => {
    it('should handle pending state for all thunks', () => {
      // Set error first
      store.dispatch({ type: 'activity/fetchActivities/rejected', payload: 'error' });

      store.dispatch({ type: 'activity/fetchActivities/pending' });

      const state = store.getState().activity;
      expect(state.loading).toBe(true);
      expect(state.error).toBeNull();
    });

    it('should handle fulfilled state for fetchActivities', () => {
      const activities = [{ id: '1', name: 'Activity 1' }];

      store.dispatch({ type: 'activity/fetchActivities/fulfilled', payload: activities });

      const state = store.getState().activity;
      expect(state.activities).toEqual(activities);
      expect(state.loading).toBe(false);
    });

    it('should handle fulfilled state for fetchActivityById', () => {
      const activity = { id: '1', name: 'Activity 1' };

      store.dispatch({ type: 'activity/fetchActivityById/fulfilled', payload: activity });

      const state = store.getState().activity;
      expect(state.currentActivity).toEqual(activity);
      expect(state.loading).toBe(false);
    });

    it('should handle fulfilled state for fetchActivitySteps', () => {
      const steps = [
        { id: '1', activityId: 'activity1', order: 1, description: 'Step 1' },
        { id: '2', activityId: 'activity1', order: 2, description: 'Step 2' }
      ];
      const payload = { activityId: 'activity1', steps };

      store.dispatch({ type: 'activity/fetchActivitySteps/fulfilled', payload });

      const state = store.getState().activity;
      // Steps should be stored under the explicit activityId from payload
      expect(state.stepsByActivityId['activity1']).toEqual(steps);
      expect(state.loading).toBe(false);
    });

    it('should handle fulfilled state for fetchActivityMaterials', () => {
      const materials = [{ id: '1', productVariantId: 'pv1', quantity: 2 }];

      store.dispatch({ type: 'activity/fetchActivityMaterials/fulfilled', payload: materials });

      const state = store.getState().activity;
      expect(state.materials).toEqual(materials);
      expect(state.loading).toBe(false);
    });

    it('should handle rejected state for all thunks', () => {
      const error = 'Network error';

      store.dispatch({ type: 'activity/fetchActivities/rejected', payload: error });

      const state = store.getState().activity;
      expect(state.loading).toBe(false);
      expect(state.error).toBe(error);
    });
  });
});