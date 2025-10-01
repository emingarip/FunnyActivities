import React, { useState, useEffect, useRef } from 'react';
import {
  Box,
  Typography,
  Button,
  Card,
  CardContent,
  IconButton,
  Chip,
  Alert,
  CircularProgress,
  useTheme,
  useMediaQuery,
  Paper,
  LinearProgress,
} from '@mui/material';
import {
  Favorite as FavoriteIcon,
  FavoriteBorder as FavoriteBorderIcon,
  PlayArrow as PlayIcon,
  Pause as PauseIcon,
  SkipNext as SkipNextIcon,
  SkipPrevious as SkipPreviousIcon,
  ListAlt as ListIcon,
  AccessTime as TimeIcon,
  Category as CategoryIcon,
} from '@mui/icons-material';
import { activitiesAPI, stepsAPI, activityProductVariantsAPI } from '../../services/api';

// Helper function to check if user is authenticated
const isAuthenticated = (): boolean => {
  return !!localStorage.getItem('accessToken');
};
import ActivityMaterialsDialog from './ActivityMaterialsDialog';

interface Activity {
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
}

interface ActivityStep {
  id: string;
  order: number;
  description: string;
  pauseTimeSeconds?: number;
}

interface ActivityMaterial {
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

interface ActivityDetailProps {
  activityId: string;
  onBack: () => void;
}

const ActivityDetail: React.FC<ActivityDetailProps> = ({ activityId, onBack }) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const videoRef = useRef<HTMLVideoElement>(null);

  const [activity, setActivity] = useState<Activity | null>(null);
  const [steps, setSteps] = useState<ActivityStep[]>([]);
  const [materials, setMaterials] = useState<ActivityMaterial[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentStepIndex, setCurrentStepIndex] = useState(0);
  const [isPlaying, setIsPlaying] = useState(false);
  const [isPausedAtStep, setIsPausedAtStep] = useState(false);
  const [materialsDialogOpen, setMaterialsDialogOpen] = useState(false);
  const [favorites, setFavorites] = useState<Set<string>>(new Set());
  const [progress, setProgress] = useState<Map<string, any>>(new Map());
  const [videoUrl, setVideoUrl] = useState<string | null>(null);

  useEffect(() => {
    loadActivityData();
    loadUserData();
  }, [activityId]);

  useEffect(() => {
    if (videoRef.current && steps.length > 0) {
      setupVideoListeners();
    }
  }, [steps]);

  const loadActivityData = async () => {
    try {
      setLoading(true);
      setError(null);

      // Use public endpoint if user is not authenticated, otherwise use authenticated endpoint
      const activityResponse = isAuthenticated()
        ? await activitiesAPI.getActivityWithDetails(activityId)
        : await activitiesAPI.getPublicActivity(activityId);

      const [stepsResponse, materialsResponse] = await Promise.all([
        stepsAPI.getStepsByActivityId(activityId),
        activityProductVariantsAPI.getActivityProductVariantsByActivityId(activityId),
      ]);

      if (activityResponse.data.success) {
        const activityData = activityResponse.data.data;
        setActivity(activityData);

        // Get video URL if available (use public endpoint if user is not authenticated)
        if (activityData.videoUrl) {
          try {
            const videoResponse = isAuthenticated()
              ? await activitiesAPI.getActivityVideoUrl(activityId, activityData.videoUrl)
              : await activitiesAPI.getPublicActivityVideoUrl(activityId, activityData.videoUrl);

            if (videoResponse.data.success) {
              setVideoUrl(videoResponse.data.data.signedVideoUrl);
            }
          } catch (videoError) {
            console.error('Error loading video URL:', videoError);
          }
        }
      }

      if (stepsResponse.data.success) {
        const stepsData = stepsResponse.data.data || [];
        setSteps(stepsData.sort((a: ActivityStep, b: ActivityStep) => a.order - b.order));
      }

      if (materialsResponse.data.success) {
        setMaterials(materialsResponse.data.data || []);
      }
    } catch (err: any) {
      console.error('Error loading activity data:', err);
      setError('Failed to load activity details');
    } finally {
      setLoading(false);
    }
  };

  const loadUserData = () => {
    // Load favorites from localStorage
    const savedFavorites = localStorage.getItem('activityFavorites');
    if (savedFavorites) {
      setFavorites(new Set(JSON.parse(savedFavorites)));
    }

    // Load progress from localStorage
    const savedProgress = localStorage.getItem('activityProgress');
    if (savedProgress) {
      const progressData = JSON.parse(savedProgress);
      setProgress(new Map(Object.entries(progressData)));
    }
  };

  const setupVideoListeners = () => {
    const video = videoRef.current;
    if (!video) return;

    const handleTimeUpdate = () => {
      const currentTime = video.currentTime;
      const currentStep = steps[currentStepIndex];

      if (currentStep && currentStep.pauseTimeSeconds && Math.abs(currentTime - currentStep.pauseTimeSeconds) < 1) {
        if (!isPausedAtStep) {
          video.pause();
          setIsPlaying(false);
          setIsPausedAtStep(true);
        }
      }
    };

    const handlePlay = () => {
      setIsPlaying(true);
      setIsPausedAtStep(false);
    };

    const handlePause = () => {
      setIsPlaying(false);
    };

    const handleEnded = () => {
      setIsPlaying(false);
      updateProgress(activityId, steps.length, steps.length);
    };

    video.addEventListener('timeupdate', handleTimeUpdate);
    video.addEventListener('play', handlePlay);
    video.addEventListener('pause', handlePause);
    video.addEventListener('ended', handleEnded);

    return () => {
      video.removeEventListener('timeupdate', handleTimeUpdate);
      video.removeEventListener('play', handlePlay);
      video.removeEventListener('pause', handlePause);
      video.removeEventListener('ended', handleEnded);
    };
  };

  const toggleFavorite = () => {
    if (!activity) return;

    const newFavorites = new Set(favorites);
    if (newFavorites.has(activity.id)) {
      newFavorites.delete(activity.id);
    } else {
      newFavorites.add(activity.id);
    }
    setFavorites(newFavorites);
    localStorage.setItem('activityFavorites', JSON.stringify(Array.from(newFavorites)));
  };

  const updateProgress = (activityId: string, completedSteps: number, totalSteps: number) => {
    const progressData = {
      activityId,
      completedSteps,
      totalSteps,
      lastWatchedAt: new Date().toISOString(),
    };

    const newProgress = new Map(progress);
    newProgress.set(activityId, progressData);
    setProgress(newProgress);

    localStorage.setItem('activityProgress', JSON.stringify(Object.fromEntries(newProgress)));
  };

  const handleContinue = () => {
    setIsPausedAtStep(false);
    setCurrentStepIndex(prev => Math.min(prev + 1, steps.length - 1));
    updateProgress(activityId, currentStepIndex + 1, steps.length);

    if (videoRef.current) {
      videoRef.current.play();
    }
  };

  const handleSkipToStep = (stepIndex: number) => {
    setCurrentStepIndex(stepIndex);
    setIsPausedAtStep(false);

    const step = steps[stepIndex];
    if (videoRef.current && step.pauseTimeSeconds) {
      videoRef.current.currentTime = step.pauseTimeSeconds;
      videoRef.current.play();
    }
  };

  const formatDuration = (hours?: number, minutes?: number, seconds?: number) => {
    const parts = [];
    if (hours && hours > 0) parts.push(`${hours}h`);
    if (minutes && minutes > 0) parts.push(`${minutes}m`);
    if (seconds && seconds > 0) parts.push(`${seconds}s`);
    return parts.join(' ') || 'N/A';
  };

  const getProgressPercentage = () => {
    if (!activity || steps.length === 0) return 0;
    const activityProgress = progress.get(activity.id);
    if (!activityProgress) return 0;
    return Math.round((activityProgress.completedSteps / activityProgress.totalSteps) * 100);
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  if (error || !activity) {
    return (
      <Box p={3}>
        <Alert severity="error" sx={{ mb: 2 }}>
          {error || 'Activity not found'}
        </Alert>
        <Button onClick={onBack}>Back to Activities</Button>
      </Box>
    );
  }

  const currentStep = steps[currentStepIndex];

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 3 }}>
        <Box>
          <Typography variant="h4" component="h1" gutterBottom>
            {activity.name}
          </Typography>
          {activity.description && (
            <Typography variant="body1" color="text.secondary" sx={{ mb: 2 }}>
              {activity.description}
            </Typography>
          )}
        </Box>
        <IconButton onClick={toggleFavorite} size="large">
          {favorites.has(activity.id) ? (
            <FavoriteIcon color="error" fontSize="large" />
          ) : (
            <FavoriteBorderIcon fontSize="large" />
          )}
        </IconButton>
      </Box>

      {/* Activity Info */}
      <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <TimeIcon />
          <Typography variant="body2">
            Duration: {formatDuration(activity.durationHours, activity.durationMinutes, activity.durationSeconds)}
          </Typography>
        </Box>
        {activity.activityCategory && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <CategoryIcon />
            <Chip label={activity.activityCategory.name} size="small" />
          </Box>
        )}
        <Button
          variant="outlined"
          startIcon={<ListIcon />}
          onClick={() => setMaterialsDialogOpen(true)}
          sx={{ minHeight: 44 }}
        >
          Materials ({materials.length})
        </Button>
      </Box>

      {/* Progress Bar */}
      {getProgressPercentage() > 0 && (
        <Box sx={{ mb: 3 }}>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
            Progress: {getProgressPercentage()}%
          </Typography>
          <LinearProgress variant="determinate" value={getProgressPercentage()} />
        </Box>
      )}

      {/* Video Player */}
      {videoUrl && (
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Box sx={{ position: 'relative' }}>
              <video
                ref={videoRef}
                src={videoUrl}
                controls={!isPausedAtStep}
                style={{ width: '100%', maxHeight: '500px' }}
                onLoadedData={() => {
                  // Video loaded, can start playing
                }}
              />

              {isPausedAtStep && currentStep && (
                <Box
                  sx={{
                    position: 'absolute',
                    top: 0,
                    left: 0,
                    right: 0,
                    bottom: 0,
                    bgcolor: 'rgba(0, 0, 0, 0.7)',
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'center',
                    alignItems: 'center',
                    color: 'white',
                    p: 3,
                  }}
                >
                  <Typography variant="h5" gutterBottom>
                    Step {currentStep.order}
                  </Typography>
                  <Typography variant="body1" sx={{ mb: 3, textAlign: 'center' }}>
                    {currentStep.description}
                  </Typography>
                  <Button
                    variant="contained"
                    size="large"
                    onClick={handleContinue}
                    startIcon={<PlayIcon />}
                  >
                    Continue Activity
                  </Button>
                </Box>
              )}
            </Box>
          </CardContent>
        </Card>
      )}

      {/* Steps Navigation */}
      {steps.length > 0 && (
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Activity Steps
            </Typography>
            <Box sx={{ display: 'flex', gap: 1, mb: 2, flexWrap: 'wrap' }}>
              {steps.map((step, index) => (
                <Button
                  key={step.id}
                  variant={index === currentStepIndex ? 'contained' : 'outlined'}
                  size="small"
                  onClick={() => handleSkipToStep(index)}
                  sx={{ minWidth: 'auto' }}
                >
                  {step.order}
                </Button>
              ))}
            </Box>
            {currentStep && (
              <Box>
                <Typography variant="h6" color="primary">
                  Step {currentStep.order}
                </Typography>
                <Typography variant="body1">
                  {currentStep.description}
                </Typography>
                {currentStep.pauseTimeSeconds && (
                  <Typography variant="body2" color="text.secondary">
                    Pause at: {Math.floor(currentStep.pauseTimeSeconds / 60)}:
                    {(currentStep.pauseTimeSeconds % 60).toString().padStart(2, '0')}
                  </Typography>
                )}
              </Box>
            )}
          </CardContent>
        </Card>
      )}

      {/* Action Buttons */}
      <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
        <Button onClick={onBack} sx={{ minHeight: 44 }}>
          Back to Activities
        </Button>
      </Box>

      {/* Materials Dialog */}
      <ActivityMaterialsDialog
        open={materialsDialogOpen}
        onClose={() => setMaterialsDialogOpen(false)}
        activityName={activity.name}
        materials={materials}
      />
    </Box>
  );
};

export default ActivityDetail;