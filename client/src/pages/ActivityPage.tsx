import React, { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Button,
  Alert,
  CircularProgress,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import { useAppSelector, useAppDispatch } from '../store/hooks';
import {
  fetchActivityById,
  fetchActivitySteps,
  fetchActivityMaterials,
  setCurrentStepIndex,
  setPausedAtStep,
  setVideoPlaying,
  updateProgress,
  selectStepsForCurrentActivity,
  selectActivityMaterials,
  selectCurrentActivity,
  selectCurrentStepIndex,
  selectIsPausedAtStep,
  selectVideoState,
  selectActivityLoading,
  selectActivityErrors,
} from '../store/slices/activitySlice';
import { activitiesAPI } from '../services/api';
import VideoUtils from '../services/videoUtils';
import ActivityHeader from '../components/activities/ActivityHeader';
import ActivityProgressBar from '../components/activities/ActivityProgressBar';
import ActivityVideoPlayer from '../components/activities/ActivityVideoPlayer';
import ActivityLayout from '../components/activities/ActivityLayout';
import ActivityMaterialsDialog from '../components/activities/ActivityMaterialsDialog';
import ActivityMaterialsPanel from '../components/activities/ActivityMaterialsPanel';
import { getJsonItem, setJsonItem } from '../utils/storage';


const ActivityPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const dispatch = useAppDispatch();

  const currentActivity = useAppSelector(selectCurrentActivity);
  const steps = useAppSelector(selectStepsForCurrentActivity);
  const materials = useAppSelector(selectActivityMaterials);
  const currentStepIndex = useAppSelector(selectCurrentStepIndex);
  const isPausedAtStep = useAppSelector(selectIsPausedAtStep);
  const videoState = useAppSelector(selectVideoState);
  const loading = useAppSelector(selectActivityLoading);
  const error = useAppSelector(selectActivityErrors);

  const [favorites, setFavorites] = useState<Set<string>>(new Set());
  const [materialsDialogOpen, setMaterialsDialogOpen] = useState(false);
  const [videoUrl, setVideoUrl] = useState<string | null>(null);
  const [progress, setProgress] = useState<Map<string, any>>(new Map());

  useEffect(() => {
    if (id) {
      loadActivityData();
      loadUserData();
    }
  }, [id]); // eslint-disable-line react-hooks/exhaustive-deps

  const loadActivityData = useCallback(async () => {
    if (!id) return;

    try {
      // Fetch activity data
      const activityResult = await dispatch(fetchActivityById(id)).unwrap();

      // Fetch steps
      const stepsResult = await dispatch(fetchActivitySteps(id)).unwrap();

      // Fetch materials
      await dispatch(fetchActivityMaterials(id)).unwrap();

      // Initialize/update progress for this activity
      const existingProgress = progress.get(id);
      if (!existingProgress || existingProgress.totalSteps !== stepsResult.steps.length) {
        updateLocalProgress(id, existingProgress?.completedSteps || 0, stepsResult.steps.length);
      }

      // Get video URL if available
      if (activityResult.videoUrl) {
        console.log('ActivityPage: Processing video URL:', {
          activityId: id,
          videoUrl: activityResult.videoUrl,
          isMinioObjectKey: VideoUtils.isMinioObjectKey(activityResult.videoUrl)
        });

        try {
          // Check if it's a MinIO object key that needs a signed URL
          if (VideoUtils.isMinioObjectKey(activityResult.videoUrl)) {
            console.log('ActivityPage: Getting signed URL for MinIO object key');
            const videoUrlResponse = await activitiesAPI.getPublicActivityVideoUrl(id, activityResult.videoUrl, 3600);
            console.log('ActivityPage: Signed URL response:', videoUrlResponse.data);

            if (videoUrlResponse.data.success && videoUrlResponse.data.data?.signedVideoUrl) {
              console.log('ActivityPage: Setting signed video URL:', videoUrlResponse.data.data.signedVideoUrl);
              setVideoUrl(videoUrlResponse.data.data.signedVideoUrl);
            } else {
              console.error('ActivityPage: Failed to get signed URL, response:', videoUrlResponse.data);
              setVideoUrl(null);
            }
          } else {
            // Direct URL
            console.log('ActivityPage: Using direct video URL:', activityResult.videoUrl);
            setVideoUrl(activityResult.videoUrl);
          }
        } catch (videoError) {
          console.error('ActivityPage: Error getting video URL:', videoError);
          setVideoUrl(null);
        }
      } else {
        console.log('ActivityPage: No video URL available');
        setVideoUrl(null);
      }
    } catch (err) {
      // Error is already handled by the Redux slice and will be displayed in the UI
    }
  }, [id, dispatch]);

  const loadUserData = useCallback(() => {
    const storedFavorites = getJsonItem<string[]>('activityFavorites', []);
    setFavorites(new Set(storedFavorites));

    const storedProgress = getJsonItem<Record<string, any>>('activityProgress', {});
    setProgress(new Map(Object.entries(storedProgress)));
  }, []);

  const toggleFavorite = useCallback(() => {
    if (!currentActivity) return;

    const newFavorites = new Set(favorites);
    if (newFavorites.has(currentActivity.id)) {
      newFavorites.delete(currentActivity.id);
    } else {
      newFavorites.add(currentActivity.id);
    }
    setFavorites(newFavorites);
    setJsonItem('activityFavorites', Array.from(newFavorites));
  }, [currentActivity, favorites]);

  const handleStepClick = useCallback((stepIndex: number) => {
    dispatch(setCurrentStepIndex(stepIndex));
    dispatch(setPausedAtStep(false));

    const step = steps[stepIndex];
    if (videoUrl && step.pauseTimeSeconds) {
      // TODO: Seek video to step time
    }
  }, [dispatch, steps, videoUrl]);

  const updateLocalProgress = useCallback((activityId: string, completedSteps: number, totalSteps: number) => {
    const progressData = {
      activityId,
      completedSteps,
      totalSteps,
      lastWatchedAt: new Date().toISOString(),
    };

    setProgress(prevProgress => {
      const newProgress = new Map(prevProgress);
      newProgress.set(activityId, progressData);
      setJsonItem('activityProgress', Object.fromEntries(newProgress));
      return newProgress;
    });
  }, []);

  const handleContinue = useCallback(() => {
    dispatch(setPausedAtStep(false));
    const nextStepIndex = Math.min(currentStepIndex + 1, steps.length - 1);
    dispatch(setCurrentStepIndex(nextStepIndex));

    // Update local progress when moving to next step
    if (currentActivity && nextStepIndex > currentStepIndex) {
      updateLocalProgress(currentActivity.id, nextStepIndex, steps.length);
    }

    dispatch(updateProgress({
      activityId: id!,
      currentStep: nextStepIndex,
      completedSteps: Array.from({ length: nextStepIndex }, (_, i) => i),
      lastWatchedAt: new Date().toISOString(),
    }));

    // TODO: Play video
  }, [dispatch, currentStepIndex, steps, id, currentActivity, updateLocalProgress]);

  const handleVideoPlay = useCallback(() => {
    dispatch(setVideoPlaying(true));
  }, [dispatch]);

  const handleVideoPause = useCallback(() => {
    dispatch(setVideoPlaying(false));
  }, [dispatch]);

  const handleVideoTimeUpdate = useCallback((currentTime: number) => {
    // Handle time updates if needed
  }, []);

  const handleVideoEnded = useCallback(() => {
    dispatch(setVideoPlaying(false));

    // Mark activity as completed
    if (currentActivity) {
      updateLocalProgress(currentActivity.id, steps.length, steps.length);
    }

    dispatch(updateProgress({
      activityId: id!,
      currentStep: steps.length,
      completedSteps: Array.from({ length: steps.length }, (_, i) => i),
      lastWatchedAt: new Date().toISOString(),
    }));
  }, [dispatch, id, steps, currentActivity, updateLocalProgress]);

  const getProgressPercentage = () => {
    if (!currentActivity || steps.length === 0) return 0;
    const activityProgress = progress.get(currentActivity.id);
    if (!activityProgress) return 0;
    return Math.round((activityProgress.completedSteps / activityProgress.totalSteps) * 100);
  };

  if (!id) {
    return (
      <Box p={3}>
        <Alert severity="error">Activity ID is required</Alert>
      </Box>
    );
  }

  if (loading && !currentActivity) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  if (error.length > 0 || !currentActivity) {
    return (
      <Box p={3}>
        <Alert severity="error" sx={{ mb: 2 }}>
          {error.length > 0 ? error[0] : 'Activity not found'}
        </Alert>
        <Button onClick={() => navigate('/')}>Back to Activities</Button>
      </Box>
    );
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', p: isMobile ? 2 : 3 }}>
      <ActivityHeader
        activity={currentActivity}
        favorites={favorites}
        onToggleFavorite={toggleFavorite}
        onBack={() => navigate('/')}
      />

      <ActivityProgressBar progressPercentage={getProgressPercentage()} />

      {/* Main Layout */}
      <Box sx={{
        display: 'flex',
        flexDirection: isMobile ? 'column' : 'row',
        gap: isMobile ? 2 : 3,
        mt: 2
      }}>
        {/* Main Content Area */}
        <Box sx={{
          flex: isMobile ? 'none' : 1,
          display: 'flex',
          flexDirection: 'column',
          gap: 2
        }}>
          <ActivityVideoPlayer
            videoUrl={videoUrl}
            steps={steps}
            currentStepIndex={currentStepIndex}
            isPlaying={videoState.isPlaying}
            isPausedAtStep={isPausedAtStep}
            onPlay={handleVideoPlay}
            onPause={handleVideoPause}
            onContinue={handleContinue}
            onTimeUpdate={handleVideoTimeUpdate}
            onEnded={handleVideoEnded}
          />

          <ActivityLayout
            steps={steps}
            currentStepIndex={currentStepIndex}
            onStepClick={handleStepClick}
          />

          {/* Materials on Mobile - below steps */}
          {isMobile && materials.length > 0 && (
            <Box sx={{ mt: 2 }}>
              <ActivityMaterialsPanel
                activityName={currentActivity.name}
                materials={materials}
              />
            </Box>
          )}
        </Box>

        {/* Materials Sidebar - Desktop Only */}
        {!isMobile && materials.length > 0 && (
          <Box sx={{
            width: '320px',
            flexShrink: 0
          }}>
            <ActivityMaterialsPanel
              activityName={currentActivity.name}
              materials={materials}
            />
          </Box>
        )}
      </Box>

      {/* Materials Dialog - Fallback for any edge cases */}
      <ActivityMaterialsDialog
        open={materialsDialogOpen}
        onClose={() => setMaterialsDialogOpen(false)}
        activityName={currentActivity.name}
        materials={materials}
      />
    </Box>
  );
};

export default ActivityPage;
