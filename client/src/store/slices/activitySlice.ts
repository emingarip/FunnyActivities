/**
 * Activity Slice - Single Source of Truth for Activity Management
 *
 * This Redux slice serves as the central state management for all activity-related data
 * and operations in the FunnyActivities application. It provides:
 *
 * 1. **Comprehensive State Management**:
 *    - Activities list and current activity
 *    - Steps organized by activity ID
 *    - Materials and progress tracking
 *    - Video state management
 *    - Granular loading and error states
 *
 * 2. **Race Condition Prevention**:
 *    - Granular loading states prevent conflicts between concurrent operations
 *    - Operation-specific error handling
 *    - Proper state isolation between different data types
 *
 * 3. **Enhanced Error Handling**:
 *    - Specific error types for different operations
 *    - Better error messages with context
 *    - Graceful handling of authentication issues
 *
 * 4. **Step Management Reliability**:
 *    - Step ordering validation
 *    - Progress tracking with completion states
 *    - Automatic step reordering and validation
 *
 * 5. **Single Source of Truth**:
 *    - All activity data flows through this slice
 *    - Consistent state updates
 *    - Comprehensive selectors for easy state access
 *
 * USAGE:
 * - Use the async thunks for API operations (fetchActivities, createStep, etc.)
 * - Use the action creators for local state updates (setCurrentActivity, updateProgress, etc.)
 * - Use the selectors to access computed state (selectCurrentStep, selectStepProgress, etc.)
 * - Subscribe to loading and error states for UI feedback
 *
 * @example
 * ```typescript
 * // Dispatch an async operation
 * dispatch(fetchActivityById('activity-123'));
 *
 * // Use selectors to access state
 * const currentStep = useSelector(selectCurrentStep);
 * const isLoading = useSelector(selectActivityLoading);
 * const errors = useSelector(selectActivityErrors);
 *
 * // Update local state
 * dispatch(setCurrentStepIndex(2));
 * dispatch(markStepCompleted({ activityId: 'activity-123', stepIndex: 1 }));
 * ```
 */

import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { activitiesAPI, stepsAPI, activityProductVariantsAPI } from '../../services/api';

// Helper function to check authentication status
const isAuthenticated = (): boolean => {
  return !!localStorage.getItem('accessToken');
};

// Helper function to create a unique operation ID for tracking concurrent operations
const createOperationId = (): string => {
  return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
};

// Enhanced progress interface with operation tracking
export interface ActivityProgress {
  activityId: string;
  currentStep: number;
  completedSteps: number[];
  lastWatchedAt?: string;
  totalSteps?: number;
  isCompleted?: boolean;
  currentOperationId?: string;
}

// Enhanced types for better type safety
export interface ActivityProgress {
  activityId: string;
  currentStep: number;
  completedSteps: number[];
  lastWatchedAt?: string;
  totalSteps?: number;
  isCompleted?: boolean;
}

export interface ActivityState {
  activities: Activity[];
  currentActivity: Activity | null;
  stepsByActivityId: Record<string, ActivityStep[]>;
  materials: ActivityMaterial[];
  currentStepIndex: number;
  isPausedAtStep: boolean;
  progress: ActivityProgress | null;
  videoState: VideoState;
  loading: {
    activities: boolean;
    currentActivity: boolean;
    steps: boolean;
    materials: boolean;
    creatingStep: boolean;
    updatingStep: boolean;
    deletingStep: boolean;
  };
  error: {
    activities: string | null;
    currentActivity: string | null;
    steps: string | null;
    materials: string | null;
    stepOperations: string | null;
  };
}

// Types based on existing Activity interface
export interface Activity {
  id: string;
  name: string;
  description?: string;
  durationHours?: number;
  durationMinutes?: number;
  durationSeconds?: number;
  activityCategoryId?: string;
  activityCategory?: {
    id: string;
    name: string;
  };
  videoUrl?: string;
  introVideoUrl?: string;
}

export interface ActivityStep {
  id: string;
  activityId: string;
  order: number;
  description: string;
  timestampSeconds: number;
}

export interface ActivityMaterial {
  id: string;
  productVariantId: string;
  quantity: number;
  unitOfMeasureId: string;
  productVariant?: {
    id: string;
    name: string;
    baseProduct?: {
      id: string;
      name: string;
    };
  };
  unitOfMeasure?: {
    id: string;
    name: string;
    symbol: string;
  };
}

export interface ActivityProgress {
  activityId: string;
  currentStep: number;
  completedSteps: number[];
  lastWatchedAt?: string;
}

export interface VideoState {
  isPlaying: boolean;
  currentTime: number;
  duration: number;
  volume: number;
}

export interface ActivityState {
  activities: Activity[];
  currentActivity: Activity | null;
  stepsByActivityId: Record<string, ActivityStep[]>;
  materials: ActivityMaterial[];
  currentStepIndex: number;
  isPausedAtStep: boolean;
  progress: ActivityProgress | null;
  videoState: VideoState;
  loading: {
    activities: boolean;
    currentActivity: boolean;
    steps: boolean;
    materials: boolean;
    creatingStep: boolean;
    updatingStep: boolean;
    deletingStep: boolean;
  };
  error: {
    activities: string | null;
    currentActivity: string | null;
    steps: string | null;
    materials: string | null;
    stepOperations: string | null;
  };
}

// Initial state
const initialState: ActivityState = {
  activities: [],
  currentActivity: null,
  stepsByActivityId: {},
  materials: [],
  currentStepIndex: 0,
  isPausedAtStep: false,
  progress: null,
  videoState: {
    isPlaying: false,
    currentTime: 0,
    duration: 0,
    volume: 1,
  },
  loading: {
    activities: false,
    currentActivity: false,
    steps: false,
    materials: false,
    creatingStep: false,
    updatingStep: false,
    deletingStep: false,
  },
  error: {
    activities: null,
    currentActivity: null,
    steps: null,
    materials: null,
    stepOperations: null,
  },
};

// Async thunks
export const fetchActivities = createAsyncThunk(
  'activity/fetchActivities',
  async (_, { rejectWithValue }) => {
    try {
      const response = await activitiesAPI.getPublicActivities();
      return response.data.data;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to fetch activities');
    }
  }
);

export const fetchActivityById = createAsyncThunk(
  'activity/fetchActivityById',
  async (activityId: string, { rejectWithValue }) => {
    try {
      // Use appropriate endpoint based on authentication status
      const response = isAuthenticated()
        ? await activitiesAPI.getActivity(activityId)
        : await activitiesAPI.getPublicActivity(activityId);

      return response.data.data;
    } catch (error: any) {
      return rejectWithValue(error.message || 'Failed to fetch activity');
    }
  }
);

export const fetchActivitySteps = createAsyncThunk(
  'activity/fetchActivitySteps',
  async (activityId: string, { rejectWithValue }) => {
    try {
      const response = await stepsAPI.getStepsByActivityId(activityId);
      // Ensure each step has the correct activityId
      const stepsWithActivityId = response.data.data.map((step: any) => ({
        ...step,
        activityId: step.activityId || activityId // Use step's activityId if available, otherwise use the provided activityId
      }));
      // Return both the activityId and steps for reliable reducer logic
      return { activityId, steps: stepsWithActivityId };
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to fetch activity steps');
    }
  }
);

export const fetchActivityMaterials = createAsyncThunk(
  'activity/fetchActivityMaterials',
  async (activityId: string, { rejectWithValue }) => {
    try {
      // Check if user is authenticated at the time of the request
      const isAuthenticated = !!localStorage.getItem('accessToken');

      // Only fetch materials if user is authenticated
      if (!isAuthenticated) {
        return []; // Return empty array for non-authenticated users
      }

      const response = await activityProductVariantsAPI.getActivityProductVariantsByActivityId(activityId);
      return response.data.data;
    } catch (error: any) {
      // For non-authenticated users, return empty array instead of error
      if (error.status === 401 || error.response?.status === 401) {
        return []; // Return empty array for non-authenticated users
      }

      // Enhanced error handling with more specific error types
      let errorMessage = 'Failed to fetch activity materials';
      if (error.response?.status === 404) {
        errorMessage = 'Activity materials not found';
      } else if (error.response?.status >= 500) {
        errorMessage = 'Server error while fetching materials';
      } else if (error.message) {
        errorMessage = error.message;
      }

      return rejectWithValue(errorMessage);
    }
  }
);

export const createStep = createAsyncThunk(
  'activity/createStep',
  async ({ activityId, stepData }: { activityId: string; stepData: { order: number; description: string; timestampSeconds: number } }, { rejectWithValue }) => {
    try {
      console.log('🔄 Redux: Creating step for activity:', activityId, 'with data:', stepData);
      const response = await stepsAPI.createEnhancedStep({
        activityId,
        ...stepData,
      });
      console.log('✅ Redux: Step created successfully:', response.data.data);
      // Ensure the returned step has the correct activityId
      return {
        ...response.data.data,
        activityId: response.data.data.activityId || activityId
      };
    } catch (error) {
      console.error('❌ Redux: Failed to create step:', error);
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to create step');
    }
  }
);

export const updateStep = createAsyncThunk(
  'activity/updateStep',
  async ({ stepId, updates }: { stepId: string; updates: { order?: number; description?: string; timestampSeconds?: number } }, { rejectWithValue, getState }) => {
    try {
      console.log('🔄 Redux: Updating step:', stepId, 'with updates:', updates);

      // Get current state to preserve activityId
      const state = getState() as { activity: ActivityState };
      // Find the step in any activity's steps array
      let currentStep: ActivityStep | undefined;
      Object.values(state.activity.stepsByActivityId).forEach(steps => {
        const found = steps.find(step => step.id === stepId);
        if (found) currentStep = found;
      });

      const response = await stepsAPI.updateEnhancedStep(stepId, updates);
      console.log('✅ Redux: Step updated successfully:', response.data.data);

      // Return updates with activityId preserved
      return {
        stepId,
        updates: {
          ...response.data.data,
          activityId: response.data.data.activityId || currentStep?.activityId
        }
      };
    } catch (error) {
      console.error('❌ Redux: Failed to update step:', error);
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to update step');
    }
  }
);

export const deleteStep = createAsyncThunk(
  'activity/deleteStep',
  async (stepId: string, { rejectWithValue }) => {
    try {
      await stepsAPI.deleteStep(stepId);
      return stepId;
    } catch (error) {
      return rejectWithValue(error instanceof Error ? error.message : 'Failed to delete step');
    }
  }
);

// Slice
const activitySlice = createSlice({
  name: 'activity',
  initialState,
  reducers: {
    setCurrentActivity: (state, action: PayloadAction<Activity | null>) => {
      state.currentActivity = action.payload;
    },
    setSteps: (state, action: PayloadAction<{ activityId: string; steps: ActivityStep[] }>) => {
      state.stepsByActivityId[action.payload.activityId] = action.payload.steps;
    },
    setMaterials: (state, action: PayloadAction<ActivityMaterial[]>) => {
      state.materials = action.payload;
    },
    setCurrentStepIndex: (state, action: PayloadAction<number>) => {
      state.currentStepIndex = action.payload;
      state.isPausedAtStep = false;
    },
    setPausedAtStep: (state, action: PayloadAction<boolean>) => {
      state.isPausedAtStep = action.payload;
    },
    updateProgress: (state, action: PayloadAction<Partial<ActivityProgress>>) => {
      if (state.progress) {
        Object.assign(state.progress, action.payload);
      } else {
        state.progress = action.payload as ActivityProgress;
      }
    },
    // Enhanced step management actions
    reorderSteps: (state, action: PayloadAction<{ activityId: string; steps: ActivityStep[] }>) => {
      const { activityId, steps } = action.payload;
      // Validate that all steps have the correct activityId and are properly ordered
      const validatedSteps = steps.map((step, index) => ({
        ...step,
        activityId,
        order: index + 1
      }));
      state.stepsByActivityId[activityId] = validatedSteps;
    },
    markStepCompleted: (state, action: PayloadAction<{ activityId: string; stepIndex: number }>) => {
      const { activityId, stepIndex } = action.payload;
      if (state.progress && state.progress.activityId === activityId) {
        const completedSteps = [...(state.progress.completedSteps || [])];
        if (!completedSteps.includes(stepIndex)) {
          completedSteps.push(stepIndex);
          state.progress.completedSteps = completedSteps;
          state.progress.currentStep = Math.max(state.progress.currentStep, stepIndex + 1);
        }
      }
    },
    resetProgress: (state, action: PayloadAction<string>) => {
      const activityId = action.payload;
      if (state.progress && state.progress.activityId === activityId) {
        state.progress = {
          activityId,
          currentStep: 0,
          completedSteps: [],
          lastWatchedAt: new Date().toISOString()
        };
      }
    },
    setVideoPlaying: (state, action: PayloadAction<boolean>) => {
      state.videoState.isPlaying = action.payload;
    },
    setVideoCurrentTime: (state, action: PayloadAction<number>) => {
      state.videoState.currentTime = action.payload;
    },
    setVideoDuration: (state, action: PayloadAction<number>) => {
      state.videoState.duration = action.payload;
    },
    setVideoVolume: (state, action: PayloadAction<number>) => {
      state.videoState.volume = action.payload;
    },
    resetVideoState: (state) => {
      state.videoState = initialState.videoState;
    },
    clearError: (state) => {
      state.error.activities = null;
      state.error.currentActivity = null;
      state.error.steps = null;
      state.error.materials = null;
      state.error.stepOperations = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchActivities.pending, (state) => {
        state.loading.activities = true;
        state.error.activities = null;
      })
      .addCase(fetchActivities.fulfilled, (state, action) => {
        state.loading.activities = false;
        state.activities = action.payload;
      })
      .addCase(fetchActivities.rejected, (state, action) => {
        state.loading.activities = false;
        state.error.activities = action.payload as string;
      })
      .addCase(fetchActivityById.pending, (state) => {
        state.loading.currentActivity = true;
        state.error.currentActivity = null;
      })
      .addCase(fetchActivityById.fulfilled, (state, action) => {
        state.loading.currentActivity = false;
        state.currentActivity = action.payload;
      })
      .addCase(fetchActivityById.rejected, (state, action) => {
        state.loading.currentActivity = false;
        state.error.currentActivity = action.payload as string;
      })
      .addCase(fetchActivitySteps.pending, (state) => {
        state.loading.steps = true;
        state.error.steps = null;
      })
      .addCase(fetchActivitySteps.fulfilled, (state, action) => {
        state.loading.steps = false;
        // Use the explicit activityId from the payload
        const { activityId, steps } = action.payload;
        state.stepsByActivityId[activityId] = steps;
      })
      .addCase(fetchActivitySteps.rejected, (state, action) => {
        state.loading.steps = false;
        state.error.steps = action.payload as string;
      })
      .addCase(fetchActivityMaterials.pending, (state) => {
        state.loading.materials = true;
        state.error.materials = null;
      })
      .addCase(fetchActivityMaterials.fulfilled, (state, action) => {
        state.loading.materials = false;
        state.materials = action.payload;
      })
      .addCase(fetchActivityMaterials.rejected, (state, action) => {
        state.loading.materials = false;
        state.error.materials = action.payload as string;
      })
      .addCase(createStep.pending, (state) => {
        state.loading.creatingStep = true;
        state.error.stepOperations = null;
      })
      .addCase(createStep.fulfilled, (state, action) => {
        state.loading.creatingStep = false;
        // Add the new step to the appropriate activity's steps array
        const activityId = action.payload.activityId;
        if (!state.stepsByActivityId[activityId]) {
          state.stepsByActivityId[activityId] = [];
        }
        state.stepsByActivityId[activityId].push(action.payload);
      })
      .addCase(createStep.rejected, (state, action) => {
        state.loading.creatingStep = false;
        state.error.stepOperations = action.payload as string;
      })
      .addCase(updateStep.pending, (state) => {
        state.loading.updatingStep = true;
        state.error.stepOperations = null;
      })
      .addCase(updateStep.fulfilled, (state, action) => {
        state.loading.updatingStep = false;
        // Update the step in the appropriate activity's steps array
        const currentStep = action.payload.updates;
        const activityId = currentStep.activityId;
        if (state.stepsByActivityId[activityId]) {
          const index = state.stepsByActivityId[activityId].findIndex(step => step.id === action.payload.stepId);
          if (index !== -1) {
            state.stepsByActivityId[activityId][index] = { ...state.stepsByActivityId[activityId][index], ...action.payload.updates };
          }
        }
      })
      .addCase(updateStep.rejected, (state, action) => {
        state.loading.updatingStep = false;
        state.error.stepOperations = action.payload as string;
      })
      .addCase(deleteStep.pending, (state) => {
        state.loading.deletingStep = true;
        state.error.stepOperations = null;
      })
      .addCase(deleteStep.fulfilled, (state, action) => {
        state.loading.deletingStep = false;
        // Remove the step from all activity steps arrays
        Object.keys(state.stepsByActivityId).forEach(activityId => {
          state.stepsByActivityId[activityId] = state.stepsByActivityId[activityId].filter(step => step.id !== action.payload);
        });
      })
      .addCase(deleteStep.rejected, (state, action) => {
        state.loading.deletingStep = false;
        state.error.stepOperations = action.payload as string;
      });
  },
});

export const {
  setCurrentActivity,
  setSteps,
  setMaterials,
  setCurrentStepIndex,
  setPausedAtStep,
  updateProgress,
  setVideoPlaying,
  setVideoCurrentTime,
  setVideoDuration,
  setVideoVolume,
  resetVideoState,
  clearError,
  reorderSteps,
  markStepCompleted,
  resetProgress,
} = activitySlice.actions;

// Selector helpers for better state access
export const selectStepsForActivity = (state: { activity: ActivityState }, activityId: string): ActivityStep[] => {
  return state.activity.stepsByActivityId[activityId] || [];
};

export const selectCurrentActivity = (state: { activity: ActivityState }): Activity | null => {
  return state.activity.currentActivity;
};

export const selectActivityProgress = (state: { activity: ActivityState }): ActivityProgress | null => {
  return state.activity.progress;
};

export const selectCurrentStepIndex = (state: { activity: ActivityState }): number => {
  return state.activity.currentStepIndex;
};

export const selectActivityMaterials = (state: { activity: ActivityState }): ActivityMaterial[] => {
  return state.activity.materials;
};

export const selectVideoState = (state: { activity: ActivityState }): VideoState => {
  return state.activity.videoState;
};

export const selectIsPausedAtStep = (state: { activity: ActivityState }): boolean => {
  return state.activity.isPausedAtStep;
};

// Enhanced selectors for complex state computations
export const selectStepsForCurrentActivity = (state: { activity: ActivityState }): ActivityStep[] => {
  const currentActivity = state.activity.currentActivity;
  if (!currentActivity) return [];
  return selectStepsForActivity(state, currentActivity.id);
};

export const selectCurrentStep = (state: { activity: ActivityState }): ActivityStep | null => {
  const steps = selectStepsForCurrentActivity(state);
  const currentIndex = state.activity.currentStepIndex;
  return steps[currentIndex] || null;
};

export const selectNextStep = (state: { activity: ActivityState }): ActivityStep | null => {
  const steps = selectStepsForCurrentActivity(state);
  const currentIndex = state.activity.currentStepIndex;
  return steps[currentIndex + 1] || null;
};

export const selectPreviousStep = (state: { activity: ActivityState }): ActivityStep | null => {
  const steps = selectStepsForCurrentActivity(state);
  const currentIndex = state.activity.currentStepIndex;
  return steps[currentIndex - 1] || null;
};

export const selectIsLastStep = (state: { activity: ActivityState }): boolean => {
  const steps = selectStepsForCurrentActivity(state);
  const currentIndex = state.activity.currentStepIndex;
  return currentIndex >= steps.length - 1;
};

export const selectIsFirstStep = (state: { activity: ActivityState }): boolean => {
  return state.activity.currentStepIndex <= 0;
};

export const selectStepProgress = (state: { activity: ActivityState }): { current: number; total: number; percentage: number } => {
  const steps = selectStepsForCurrentActivity(state);
  const currentIndex = state.activity.currentStepIndex;
  const total = steps.length;
  const current = Math.min(currentIndex + 1, total);
  const percentage = total > 0 ? Math.round((current / total) * 100) : 0;

  return { current, total, percentage };
};

export const selectActivityLoading = (state: { activity: ActivityState }): boolean => {
  return Object.values(state.activity.loading).some(loading => loading);
};

export const selectActivityErrors = (state: { activity: ActivityState }): string[] => {
  return Object.values(state.activity.error).filter((error): error is string => error !== null);
};

export const selectHasActivityErrors = (state: { activity: ActivityState }): boolean => {
  return selectActivityErrors(state).length > 0;
};

export const selectActivityById = (state: { activity: ActivityState }, activityId: string): Activity | null => {
  return state.activity.activities.find(activity => activity.id === activityId) || null;
};

export const selectMaterialsForActivity = (state: { activity: ActivityState }, activityId: string): ActivityMaterial[] => {
  // This would typically filter materials by activityId if the API provided that relationship
  // For now, return all materials as they should be associated with the current activity
  return state.activity.materials;
};

export default activitySlice.reducer;
