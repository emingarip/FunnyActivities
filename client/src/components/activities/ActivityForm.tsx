import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { useForm, useFieldArray, Controller } from 'react-hook-form';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import {
  Box,
  TextField,
  Button,
  Typography,
  Paper,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  IconButton,
  Card,
  CardContent,
  Divider,
  Alert,
  CircularProgress,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Upload as UploadIcon,
  PlayArrow as PlayIcon,
} from '@mui/icons-material';
import { activitiesAPI, activityCategoriesAPI, productsAPI, stepsAPI, activityProductVariantsAPI } from '../../services/api';
import EnhancedStepManager from './EnhancedStepManager';
import { EnhancedStepDto } from '../../services/api.types';
import { VideoUtils } from '../../services/videoUtils';
import {
  createStep,
  updateStep,
  deleteStep,
  fetchActivitySteps,
  fetchActivityMaterials,
  setSteps,
  setMaterials,
  clearError,
  selectStepsForActivity,
  selectActivityMaterials,
  selectActivityLoading,
  selectActivityErrors
} from '../../store/slices/activitySlice';

interface ActivityCategory {
  id: string;
  name: string;
  description?: string;
}

interface ProductVariant {
  id: string;
  name: string;
  baseProduct?: {
    id: string;
    name: string;
  };
}

interface UnitOfMeasure {
  id: string;
  name: string;
  symbol: string;
}

interface ActivityStep {
  id?: string;
  order: number;
  description: string;
  pauseTimeSeconds?: number;
}

interface ActivityMaterial {
  id?: string;
  productVariantId: string;
  quantity: number;
  unitOfMeasureId: string;
  productVariant?: ProductVariant;
  unitOfMeasure?: UnitOfMeasure;
}

interface ActivityFormData {
  name: string;
  description?: string;
  activityCategoryId?: string;
  durationHours?: number;
  durationMinutes?: number;
  durationSeconds?: number;
  videoFile?: File;
  steps: ActivityStep[];
  materials: ActivityMaterial[];
}

interface ActivityFormProps {
  activity?: any; // The activity being edited, if any
  categories: ActivityCategory[];
  onSuccess: () => void;
  onCancel: () => void;
}

const ActivityForm: React.FC<ActivityFormProps> = ({
  activity,
  categories,
  onSuccess,
  onCancel,
}) => {
  const dispatch = useAppDispatch();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [stepOperationLoading, setStepOperationLoading] = useState(false);
  const [productVariants, setProductVariants] = useState<ProductVariant[]>([]);
  const [unitsOfMeasure, setUnitsOfMeasure] = useState<UnitOfMeasure[]>([]);
  const [videoFile, setVideoFile] = useState<File | null>(null);
  const [videoUrl, setVideoUrl] = useState<string | undefined>();
  const [existingVideoUrl, setExistingVideoUrl] = useState<string | undefined>();

  // Get steps, error, and loading state from Redux using selectors
  const reduxSteps = useAppSelector((state) => {
    // Get steps for the current activity
    return activity?.id ? selectStepsForActivity(state, activity.id) : [];
  });
  const reduxError = useAppSelector(selectActivityErrors);
  const reduxLoading = useAppSelector(selectActivityLoading);

  // Convert Redux ActivityStep[] to EnhancedStepDto[] for EnhancedStepManager (memoized for performance)
  const existingSteps: EnhancedStepDto[] = useMemo(() => {
    return reduxSteps.map(step => ({
      id: step.id,
      activityId: step.activityId, // Use the activityId from Redux state
      order: step.order,
      description: step.description,
      timestampSeconds: 0, // Default value since Redux doesn't store this
      durationSeconds: 0,  // Default value since Redux doesn't store this
      pauseTimeSeconds: step.pauseTimeSeconds || 0,
      mediaAttachments: [], // Default value since Redux doesn't store this
    }));
  }, [reduxSteps]);

  const {
    control,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
    reset,
  } = useForm<ActivityFormData>({
    defaultValues: {
      name: '',
      description: '',
      activityCategoryId: '',
      durationHours: 0,
      durationMinutes: 0,
      durationSeconds: 0,
    },
  });

  // Watch form values for debugging
  const watchedValues = watch();


  const loadFormData = useCallback(async () => {
    try {
      setLoading(true);

      // Load product variants and units of measure
      const [variantsResponse, unitsResponse] = await Promise.all([
        productsAPI.getProductVariants(undefined, { pageSize: 100 }),
        productsAPI.getUnitsOfMeasure(),
      ]);

      if (variantsResponse.data.success) {
        setProductVariants(variantsResponse.data.data?.items || []);
      }

      if (unitsResponse.data.success) {
        setUnitsOfMeasure(unitsResponse.data.data || []);
      }

      // If editing an activity, load its data
      if (activity) {
       // Clear Redux state first to ensure fresh data
       dispatch(setSteps({ activityId: activity.id, steps: [] }));
       dispatch(clearError());

       const [activityDetailsResponse, materialsResponse] = await Promise.all([
         activitiesAPI.getActivityWithDetails(activity.id),
         activityProductVariantsAPI.getActivityProductVariantsByActivityId(activity.id),
       ]);

       if (activityDetailsResponse.data.success) {
         const activityData = activityDetailsResponse.data.data;

         // Parse duration from string format if needed
         let durationHours = 0, durationMinutes = 0, durationSeconds = 0;

         if (activityData.duration && typeof activityData.duration === 'string') {
           const parts = activityData.duration.split(':');
           if (parts.length === 3) {
             durationHours = parseInt(parts[0]) || 0;
             durationMinutes = parseInt(parts[1]) || 0;
             durationSeconds = parseInt(parts[2]) || 0;
           } else if (parts.length === 2) {
             durationHours = 0;
             durationMinutes = parseInt(parts[0]) || 0;
             durationSeconds = parseInt(parts[1]) || 0;
           }
         } else {
           // Use separate fields if available
           durationHours = activityData.durationHours || 0;
           durationMinutes = activityData.durationMinutes || 0;
           durationSeconds = activityData.durationSeconds || 0;
         }

         const resetData = {
           name: activityData.name,
           description: activityData.description || '',
           activityCategoryId: activityData.activityCategoryId || '',
           durationHours,
           durationMinutes,
           durationSeconds,
         };

         reset(resetData);
       }

       // Fetch steps using Redux thunk
       console.log('[ActivityForm] Fetching steps via Redux for activity:', activity.id);
       await dispatch(fetchActivitySteps(activity.id)).unwrap();

       // Set video URL if available - convert object key to signed URL
       if (activity.videoUrl) {
         // Extract object key from signed URLs or use as-is if already an object key
         const objectKey = VideoUtils.extractObjectKeyFromSignedUrl(activity.videoUrl);
         console.log('Extracted object key from activity.videoUrl:', {
           originalUrl: activity.videoUrl,
           extractedObjectKey: objectKey,
           isMinioObjectKey: VideoUtils.isMinioObjectKey(activity.videoUrl)
         });

         // Always use the extracted object key, never pass signed URLs to backend
         let finalObjectKey = objectKey || (VideoUtils.isMinioObjectKey(activity.videoUrl) ? activity.videoUrl : null);

         // Handle corrupted object keys that may have bucket name prepended
         if (finalObjectKey && finalObjectKey.startsWith('activity-videos/')) {
           finalObjectKey = finalObjectKey.substring('activity-videos/'.length);
           console.log('Cleaned corrupted object key:', { cleanedObjectKey: finalObjectKey });
         }

         if (finalObjectKey) {
           setExistingVideoUrl(finalObjectKey); // Store the object key for updates

           try {
             console.log('Getting signed URL for object key:', finalObjectKey);
             const videoUrlResponse = await activitiesAPI.getActivityVideoUrl(activity.id, finalObjectKey);
             if (videoUrlResponse.data.success) {
               const signedUrl = videoUrlResponse.data.data.signedVideoUrl;
               console.log('Generated signed URL:', signedUrl);
               setVideoUrl(signedUrl);
             } else {
               console.warn('Failed to get signed video URL:', videoUrlResponse.data);
               setVideoUrl(activity.videoUrl); // Fallback to original URL
             }
           } catch (error) {
             console.error('Error getting signed video URL:', error);
             setVideoUrl(activity.videoUrl); // Fallback to original URL
           }
         } else {
           console.warn('Could not extract valid object key from video URL:', activity.videoUrl);
           setVideoUrl(activity.videoUrl); // Fallback to original URL
         }
       }

       if (materialsResponse.data.success) {
         const materials = materialsResponse.data.data || [];
         // Dispatch materials to Redux store
         dispatch(setMaterials(materials));
         if (materials.length > 0) {
           setValue('materials', materials.map((material: any) => ({
             id: material.id,
             productVariantId: material.productVariantId,
             quantity: material.quantity,
             unitOfMeasureId: material.unitOfMeasureId,
           })));
         }
       }
     }
   } catch (err: any) {
     console.error('Error loading form data:', err);
     setError('Failed to load form data');
   } finally {
     setLoading(false);
   }
 }, [activity, dispatch, reset, setValue]);

  useEffect(() => {
    loadFormData();
  }, [activity, loadFormData]); // Include loadFormData in dependencies

  // Update duration fields when activity changes - memoized to prevent infinite loops
   const updateDurationFields = useCallback(() => {
     if (activity) {
       console.log('[ActivityForm] 🔄 updateDurationFields called:', {
         activityId: activity.id,
         durationHours: activity.durationHours,
         durationMinutes: activity.durationMinutes,
         durationSeconds: activity.durationSeconds,
         timestamp: Date.now()
       });
       setValue('durationHours', activity.durationHours || 0);
       setValue('durationMinutes', activity.durationMinutes || 0);
       setValue('durationSeconds', activity.durationSeconds || 0);
     }
   }, [activity, setValue]);

  useEffect(() => {
    updateDurationFields();
  }, [updateDurationFields, activity?.id]);

  // Debug: Watch form values (only in development) - reduced frequency to prevent infinite loops
  useEffect(() => {
    if (process.env.NODE_ENV === 'development') {
      const intervalId = setInterval(() => {
        console.log('[ActivityForm] Watched form values:', watchedValues);
      }, 1000); // Only log once per second to prevent spam

      return () => clearInterval(intervalId);
    }
  }, [watchedValues]); // Include watchedValues in dependencies

  // Sync Redux steps state with component (for debugging) - reduced frequency to prevent infinite loops
  useEffect(() => {
    if (process.env.NODE_ENV === 'development') {
      const intervalId = setInterval(() => {
        console.log('[ActivityForm] Redux steps state:', existingSteps);
      }, 2000); // Only log every 2 seconds to prevent spam

      return () => clearInterval(intervalId);
    }
  }, [existingSteps]); // Include existingSteps in dependencies

  // Sync Redux errors with local error state
   useEffect(() => {
     if (reduxError.length > 0) {
       console.error('[ActivityForm] Redux error occurred:', reduxError);
       setError(reduxError[0]); // Take the first error message
     } else {
       setError(null);
     }
   }, [reduxError]);

  // Handle steps change from EnhancedStepManager - simplified Redux integration
   const handleStepsChange = useCallback((updatedSteps: EnhancedStepDto[]) => {
     // Convert EnhancedStepDto[] to ActivityStep[] for Redux
     const reduxSteps = updatedSteps
       .filter(step => step.id) // Only include steps with IDs
       .map(step => ({
         id: step.id!,
         activityId: step.activityId,
         order: step.order,
         description: step.description,
         pauseTimeSeconds: step.pauseTimeSeconds || 0,
       }));

     // Update Redux with the new steps
     if (activity?.id && reduxSteps.length > 0) {
       dispatch(setSteps({ activityId: activity.id, steps: reduxSteps }));
     }
   }, [activity?.id, dispatch]);


  const handleVideoUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      // Validate file type
      if (!file.type.startsWith('video/')) {
        setError('Please select a valid video file');
        return;
      }

      // Validate file size (max 100MB)
      if (file.size > 100 * 1024 * 1024) {
        setError('Video file size must be less than 100MB');
        return;
      }

      setVideoFile(file);
      setError(null);
    }
  };


  const onSubmit = async (data: ActivityFormData) => {
    try {
      setLoading(true);
      setError(null);

      // Debug logging for duration values
      console.log('[ActivityForm] Form submission data:', data);
      console.log('[ActivityForm] Duration values:', {
        hours: data.durationHours,
        minutes: data.durationMinutes,
        seconds: data.durationSeconds
      });

      // Ensure duration values are numbers
      const durationHours = Number(data.durationHours) || 0;
      const durationMinutes = Number(data.durationMinutes) || 0;
      const durationSeconds = Number(data.durationSeconds) || 0;

      console.log('[ActivityForm] Normalized duration values:', {
        durationHours,
        durationMinutes,
        durationSeconds
      });

      // Log what will be sent to API
      const updatePayload: any = {
        name: data.name,
        description: data.description,
        durationHours,
        durationMinutes,
        durationSeconds,
      };

      // Include existing videoUrl if not uploading a new video
      if (!videoFile && existingVideoUrl) {
        updatePayload.videoUrl = existingVideoUrl;
      }

      console.log('[ActivityForm] API update payload:', updatePayload);

      // Validate duration values
      if (durationHours < 0 || durationMinutes < 0 || durationSeconds < 0) {
        setError('Duration values cannot be negative');
        setLoading(false);
        return;
      }

      if (durationMinutes >= 60 || durationSeconds >= 60) {
        setError('Minutes and seconds must be less than 60');
        setLoading(false);
        return;
      }

      let activityId = activity?.id;

      // Create or update activity
      if (activityId) {
        console.log('[ActivityForm] Updating activity with duration:', {
          durationHours,
          durationMinutes,
          durationSeconds,
        });
        await activitiesAPI.updateActivity(activityId, updatePayload);
      } else {
        console.log('[ActivityForm] Creating activity with duration:', {
          durationHours,
          durationMinutes,
          durationSeconds,
        });
        const createResponse = await activitiesAPI.createActivity({
          name: data.name,
          description: data.description,
          activityCategoryId: data.activityCategoryId,
          durationHours,
          durationMinutes,
          durationSeconds,
        });

        if (createResponse.data.success) {
          activityId = createResponse.data.data.id;
        } else {
          throw new Error('Failed to create activity');
        }
      }

      if (!activityId) {
        throw new Error('Activity ID not available');
      }

      // Upload video if provided
      if (videoFile) {
        await activitiesAPI.uploadActivityVideo(activityId, videoFile);
      }

      // Save steps if this is a new activity or if there are unsaved steps
      if (reduxSteps.length > 0) {
        console.log('Saving steps for activity:', activityId, reduxSteps);

        // For each step, create or update it
        for (const step of existingSteps) {
          try {
            if (step.id) {
              // Update existing step
              await stepsAPI.updateEnhancedStep(step.id, {
                order: step.order,
                description: step.description,
                timestampSeconds: step.timestampSeconds,
                durationSeconds: step.durationSeconds,
                pauseTimeSeconds: step.pauseTimeSeconds,
              });
            } else {
              // Create new step
              await stepsAPI.createEnhancedStep({
                activityId,
                order: step.order,
                description: step.description,
                timestampSeconds: step.timestampSeconds,
                durationSeconds: step.durationSeconds,
                pauseTimeSeconds: step.pauseTimeSeconds,
              });
            }
          } catch (stepError) {
            console.error('Failed to save step:', step, stepError);
            // Continue with other steps even if one fails
          }
        }
      }

      // Materials are now handled by Redux store
      // Access materials via Redux selectors when needed

      onSuccess();
    } catch (err: any) {
      console.error('Error saving activity:', err);
      setError(err.response?.data?.message || 'Failed to save activity');
    } finally {
      setLoading(false);
    }
  };

  if ((loading || reduxLoading) && !activity) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box component="form" onSubmit={handleSubmit(onSubmit)} sx={{ mt: 2 }}>
      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
        {/* Basic Information */}
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom>
            Basic Information
          </Typography>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
              <Controller
                name="name"
                control={control}
                rules={{ required: 'Activity name is required' }}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label="Activity Name"
                    sx={{ flex: 1, minWidth: '200px' }}
                    error={!!errors.name}
                    helperText={errors.name?.message}
                    required
                  />
                )}
              />
              <Controller
                name="activityCategoryId"
                control={control}
                render={({ field }) => (
                  <FormControl sx={{ minWidth: '200px' }}>
                    <InputLabel>Category</InputLabel>
                    <Select {...field} label="Category">
                      <MenuItem value="">
                        <em>No category</em>
                      </MenuItem>
                      {categories.map((category) => (
                        <MenuItem key={category.id} value={category.id}>
                          {category.name}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                )}
              />
            </Box>
            <Controller
              name="description"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Description"
                  fullWidth
                  multiline
                  rows={3}
                />
              )}
            />
            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
              <Controller
                key={`durationHours-${activity?.id || 'new'}`}
                name="durationHours"
                control={control}
                render={({ field }) => {
                  console.log('[ActivityForm] DurationHours Controller render:', {
                    fieldValue: field.value,
                    activityId: activity?.id,
                    key: `durationHours-${activity?.id || 'new'}`
                  });
                  return (
                    <TextField
                      {...field}
                      label="Hours"
                      type="number"
                      sx={{ minWidth: '100px' }}
                      inputProps={{ min: 0 }}
                      value={field.value || ''}
                      onChange={(e) => field.onChange(e.target.value === '' ? 0 : parseInt(e.target.value) || 0)}
                    />
                  );
                }}
              />
              <Controller
                key={`durationMinutes-${activity?.id || 'new'}`}
                name="durationMinutes"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label="Minutes"
                    type="number"
                    sx={{ minWidth: '100px' }}
                    inputProps={{ min: 0, max: 59 }}
                    value={field.value || ''}
                    onChange={(e) => field.onChange(e.target.value === '' ? 0 : parseInt(e.target.value) || 0)}
                  />
                )}
              />
              <Controller
                key={`durationSeconds-${activity?.id || 'new'}`}
                name="durationSeconds"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label="Seconds"
                    type="number"
                    sx={{ minWidth: '100px' }}
                    inputProps={{ min: 0, max: 59 }}
                    value={field.value || ''}
                    onChange={(e) => field.onChange(e.target.value === '' ? 0 : parseInt(e.target.value) || 0)}
                  />
                )}
              />
            </Box>
          </Box>
        </Paper>

        {/* Video Upload */}
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom>
            Video
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Button
              variant="outlined"
              component="label"
              startIcon={<UploadIcon />}
            >
              Upload Video
              <input
                type="file"
                hidden
                accept="video/*"
                onChange={handleVideoUpload}
              />
            </Button>
            {videoFile && (
              <Typography variant="body2">
                {videoFile.name} ({(videoFile.size / (1024 * 1024)).toFixed(2)} MB)
              </Typography>
            )}
          </Box>
        </Paper>

        {/* Enhanced Steps Manager */}
        <Paper sx={{ p: 3 }}>
          <EnhancedStepManager
            activityId={activity?.id || 'new'}
            videoUrl={videoUrl}
            steps={existingSteps}
            onStepsChange={handleStepsChange}
            onStepCreate={async (step) => {
              if (activity?.id) {
                try {
                  setStepOperationLoading(true);
                  setError(null);
                  console.log('[ActivityForm] Creating step via Redux:', step);
                  await dispatch(createStep({
                    activityId: activity.id,
                    stepData: {
                      order: step.order,
                      description: step.description,
                      timestampSeconds: step.timestampSeconds,
                      durationSeconds: step.durationSeconds,
                      pauseTimeSeconds: step.pauseTimeSeconds,
                    }
                  })).unwrap();
                  console.log('[ActivityForm] Step created successfully via Redux');
                } catch (error) {
                  console.error('Failed to create step:', error);
                  setError('Failed to create step');
                } finally {
                  setStepOperationLoading(false);
                }
              }
            }}
            onStepUpdate={async (stepId, updates) => {
              try {
                setStepOperationLoading(true);
                setError(null);
                await dispatch(updateStep({
                  stepId,
                  updates: {
                    order: updates.order,
                    description: updates.description,
                    timestampSeconds: updates.timestampSeconds,
                    durationSeconds: updates.durationSeconds,
                    pauseTimeSeconds: updates.pauseTimeSeconds,
                  }
                })).unwrap();
              } catch (error) {
                console.error('Failed to update step:', error);
                setError('Failed to update step');
              } finally {
                setStepOperationLoading(false);
              }
            }}
            onStepDelete={async (stepId) => {
              try {
                setStepOperationLoading(true);
                setError(null);
                await dispatch(deleteStep(stepId)).unwrap();
              } catch (error) {
                console.error('Failed to delete step:', error);
                setError('Failed to delete step');
              } finally {
                setStepOperationLoading(false);
              }
            }}
          />
        </Paper>

      </Box>

      <Box sx={{ mt: 3, display: 'flex', gap: 2, justifyContent: 'flex-end', flexWrap: 'wrap' }}>
        <Button
          variant="outlined"
          onClick={onCancel}
          disabled={loading || reduxLoading}
          sx={{ minHeight: 44 }}
        >
          Cancel
        </Button>
        <Button
          type="submit"
          variant="contained"
          disabled={loading || stepOperationLoading || reduxLoading}
          startIcon={(loading || stepOperationLoading || reduxLoading) ? <CircularProgress size={16} /> : null}
          sx={{ minHeight: 44 }}
        >
          {(loading || stepOperationLoading || reduxLoading) ? 'Saving...' : (activity ? 'Update Activity' : 'Create Activity')}
        </Button>
      </Box>
    </Box>
  );
};

export default ActivityForm;