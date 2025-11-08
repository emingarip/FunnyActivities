import React, { useRef, useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  useTheme,
  useMediaQuery,
  Tooltip,
  IconButton,
} from '@mui/material';
import { PlayArrow as PlayIcon, RadioButtonChecked as MarkerIcon } from '@mui/icons-material';

interface ActivityStep {
  id: string;
  order: number;
  description: string;
  pauseTimeSeconds?: number;
}

interface ActivityVideoPlayerProps {
  videoUrl: string | null;
  introVideoUrl?: string | null;
  steps: ActivityStep[];
  currentStepIndex: number;
  isPlaying: boolean;
  isPausedAtStep: boolean;
  onPlay: () => void;
  onPause: () => void;
  onContinue: () => void;
  onTimeUpdate: (currentTime: number) => void;
  onEnded: () => void;
}

const ActivityVideoPlayer: React.FC<ActivityVideoPlayerProps> = ({
  videoUrl,
  introVideoUrl,
  steps,
  currentStepIndex,
  isPlaying,
  isPausedAtStep,
  onPlay,
  onPause,
  onContinue,
  onTimeUpdate,
  onEnded,
}) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const videoRef = useRef<HTMLVideoElement>(null);
  const introVideoRef = useRef<HTMLVideoElement>(null);
  const [videoDuration, setVideoDuration] = useState<number>(0);
  const [introCompleted, setIntroCompleted] = useState(!introVideoUrl);
  const [shouldAutoPlayMain, setShouldAutoPlayMain] = useState(false);

  useEffect(() => {
    setIntroCompleted(!introVideoUrl);
    setShouldAutoPlayMain(false);
  }, [introVideoUrl]);

  const isIntroPhase = Boolean(introVideoUrl) && !introCompleted;

  console.log('ActivityVideoPlayer: Render', {
    videoUrl: videoUrl ? `${videoUrl.substring(0, 50)}...` : null,
    introVideoUrl: introVideoUrl ? `${introVideoUrl.substring(0, 50)}...` : null,
    stepsCount: steps.length,
    currentStepIndex,
    isPlaying,
    isPausedAtStep,
    isIntroPhase,
  });

  useEffect(() => {
    if (isIntroPhase) {
      return;
    }

    if (videoRef.current && steps.length > 0) {
      const cleanup = setupVideoListeners();
      return cleanup;
    }
  }, [steps, isIntroPhase]);

  useEffect(() => {
    if (isIntroPhase) {
      return;
    }

    const video = videoRef.current;
    if (!video) {
      return;
    }

    if (isPlaying && !isPausedAtStep) {
      video.play().catch(error => console.error('ActivityVideoPlayer: play failed', error));
    } else if (!isPlaying) {
      video.pause();
    }
  }, [isPlaying, isPausedAtStep, isIntroPhase]);

  useEffect(() => {
    if (!isIntroPhase && shouldAutoPlayMain) {
      const timer = setTimeout(() => {
        const video = videoRef.current;
        if (video) {
          video.play().catch(error => console.error('ActivityVideoPlayer: auto play failed', error));
        }
        setShouldAutoPlayMain(false);
      }, 0);

      return () => clearTimeout(timer);
    }
  }, [isIntroPhase, shouldAutoPlayMain]);

  const setupVideoListeners = () => {
    const video = videoRef.current;
    if (!video) {
      return;
    }

    const handleTimeUpdate = () => {
      const currentTime = video.currentTime;
      onTimeUpdate(currentTime);

      const currentStep = steps[currentStepIndex];
      if (currentStep && currentStep.pauseTimeSeconds) {
        const timeToPause = currentStep.pauseTimeSeconds - currentTime;
        if (timeToPause >= 0 && timeToPause < 0.5 && !isPausedAtStep) {
          video.pause();
          onPause();
        }
      }
    };

    const handlePlay = () => onPlay();
    const handlePause = () => onPause();
    const handleEnded = () => onEnded();

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

  if (!videoUrl && !introVideoUrl) {
    return null;
  }

  const handleIntroComplete = (autoPlayMain: boolean) => {
    if (introVideoRef.current) {
      introVideoRef.current.pause();
    }
    setIntroCompleted(true);
    setShouldAutoPlayMain(autoPlayMain);
  };

  if (isIntroPhase && introVideoUrl) {
    return (
      <Card sx={{ mb: 3 }}>
        <CardContent sx={{ p: isMobile ? 1 : 2 }}>
          <Box sx={{ position: 'relative' }}>
            <video
              data-testid="intro-video"
              ref={introVideoRef}
              src={introVideoUrl}
              controls
              autoPlay
              crossOrigin="anonymous"
              preload="auto"
              style={{
                width: '100%',
                maxHeight: isMobile ? '300px' : '500px',
                aspectRatio: isMobile ? undefined : '16/9',
                borderRadius: theme.shape.borderRadius,
              }}
              onEnded={() => handleIntroComplete(true)}
            />
            <Box
              sx={{
                position: 'absolute',
                top: 0,
                left: 0,
                right: 0,
                bottom: 0,
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'flex-end',
                p: 2,
                gap: 1,
                pointerEvents: 'none',
              }}
            >
              <Box
                sx={{
                  backgroundColor: 'rgba(0,0,0,0.6)',
                  color: 'white',
                  borderRadius: theme.shape.borderRadius,
                  p: 2,
                  pointerEvents: 'auto',
                  display: 'flex',
                  flexDirection: isMobile ? 'column' : 'row',
                  alignItems: isMobile ? 'flex-start' : 'center',
                  justifyContent: 'space-between',
                  gap: 2,
                }}
              >
                <Box>
                  <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                    Intro Video
                  </Typography>
                  <Typography variant="body2" sx={{ opacity: 0.9 }}>
                    A short introduction will play before the main activity.
                  </Typography>
                </Box>
                <Button
                  variant="contained"
                  color="secondary"
                  size="small"
                  onClick={() => handleIntroComplete(false)}
                >
                  Skip Intro
                </Button>
              </Box>
            </Box>
          </Box>
        </CardContent>
      </Card>
    );
  }

  if (!videoUrl) {
    return (
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Alert severity="warning">
            The main activity video is not available.
          </Alert>
        </CardContent>
      </Card>
    );
  }

  const currentStep = steps[currentStepIndex];

  const formatTime = (seconds: number): string => {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = Math.floor(seconds % 60);
    return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
  };

  const handleMarkerClick = (pauseTimeSeconds: number) => {
    if (videoRef.current) {
      videoRef.current.currentTime = pauseTimeSeconds;
    }
  };

  return (
    <Card sx={{ mb: 3 }}>
      <CardContent sx={{ p: isMobile ? 1 : 2 }}>
        <Box sx={{ position: 'relative' }}>
          <video
            data-testid="activity-video"
            ref={videoRef}
            src={videoUrl}
            controls={!isPausedAtStep}
            crossOrigin="anonymous"
            preload="none"
            style={{
              width: '100%',
              maxHeight: isMobile ? '300px' : '500px',
              aspectRatio: isMobile ? undefined : '16/9',
              borderRadius: theme.shape.borderRadius,
            }}
            onLoadedMetadata={() => {
              if (videoRef.current) {
                setVideoDuration(videoRef.current.duration || 0);
              }
            }}
            onClick={() => {
              if (!isPlaying && !isPausedAtStep) {
                onPlay();
              }
            }}
          />

          {!isPausedAtStep && videoDuration > 0 && steps.some(step => step.pauseTimeSeconds) && (
            <Box
              sx={{
                position: 'absolute',
                bottom: isMobile ? '60px' : '50px',
                left: 0,
                right: 0,
                height: '20px',
                pointerEvents: 'none',
                zIndex: 1,
              }}
            >
              {steps
                .filter(step => step.pauseTimeSeconds && step.pauseTimeSeconds <= videoDuration)
                .map((step) => {
                  const positionPercent = (step.pauseTimeSeconds! / videoDuration) * 100;
                  return (
                    <Tooltip
                      key={step.id}
                      title={
                        <Box>
                          <Typography variant="body2" sx={{ fontWeight: 'bold' }}>
                            Step {step.order}: {formatTime(step.pauseTimeSeconds!)}
                          </Typography>
                          <Typography variant="body2" sx={{ mt: 0.5 }}>
                            {step.description}
                          </Typography>
                        </Box>
                      }
                      arrow
                      placement="top"
                    >
                      <IconButton
                        size="small"
                        sx={{
                          position: 'absolute',
                          left: `${positionPercent}%`,
                          top: '50%',
                          transform: 'translate(-50%, -50%)',
                          color: theme.palette.primary.main,
                          backgroundColor: 'rgba(255, 255, 255, 0.9)',
                          border: `2px solid ${theme.palette.primary.main}`,
                          width: isMobile ? '16px' : '20px',
                          height: isMobile ? '16px' : '20px',
                          minWidth: 'auto',
                          padding: 0,
                          pointerEvents: 'auto',
                          '&:hover': {
                            backgroundColor: theme.palette.primary.main,
                            color: 'white',
                            transform: 'translate(-50%, -50%) scale(1.2)',
                          },
                          transition: 'all 0.2s ease-in-out',
                        }}
                        onClick={(e) => {
                          e.stopPropagation();
                          handleMarkerClick(step.pauseTimeSeconds!);
                        }}
                        aria-label={`Jump to step ${step.order} at ${formatTime(step.pauseTimeSeconds!)}`}
                      >
                        <MarkerIcon sx={{ fontSize: isMobile ? '12px' : '14px' }} />
                      </IconButton>
                    </Tooltip>
                  );
                })}
            </Box>
          )}

          {isPausedAtStep && currentStep && (
            <Box
              data-testid="step-overlay"
              sx={{
                position: 'absolute',
                top: 0,
                left: 0,
                right: 0,
                bottom: 0,
                bgcolor: 'rgba(0, 0, 0, 0.8)',
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'center',
                alignItems: 'center',
                color: 'white',
                p: isMobile ? 2 : 3,
                borderRadius: theme.shape.borderRadius,
              }}
            >
              <Typography
                variant={isMobile ? 'h6' : 'h5'}
                gutterBottom
                sx={{ textAlign: 'center' }}
              >
                Step {currentStep.order}
              </Typography>
              <Typography
                variant="body1"
                sx={{
                  mb: 3,
                  textAlign: 'center',
                  wordBreak: 'break-word',
                  maxWidth: '90%',
                }}
              >
                {currentStep.description}
              </Typography>
              <Button
                data-cy="continue-button"
                variant="contained"
                size={isMobile ? 'medium' : 'large'}
                onClick={onContinue}
                startIcon={<PlayIcon />}
                sx={{
                  minWidth: isMobile ? '120px' : '160px',
                  minHeight: 44,
                  fontSize: isMobile ? '1rem' : '1rem',
                }}
              >
                Continue Activity
              </Button>
            </Box>
          )}
        </Box>
      </CardContent>
    </Card>
  );
};

export default ActivityVideoPlayer;
