import React, { useRef, useEffect, useState } from 'react';
import {
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
  const [videoDuration, setVideoDuration] = useState<number>(0);

  console.log('ActivityVideoPlayer: Component rendered with props:', {
    videoUrl: videoUrl ? `${videoUrl.substring(0, 50)}...` : null,
    stepsCount: steps.length,
    currentStepIndex,
    isPlaying,
    isPausedAtStep,
  });

  useEffect(() => {
    if (videoRef.current && steps.length > 0) {
      console.log('ActivityVideoPlayer: Setting up video listeners for steps:', steps.length);
      const cleanup = setupVideoListeners();
      return cleanup;
    } else {
      console.log('ActivityVideoPlayer: Skipping video listeners setup - videoRef:', !!videoRef.current, 'steps:', steps.length);
    }
  }, [steps]);

  // Control video playback based on isPlaying prop
  useEffect(() => {
    const video = videoRef.current;
    if (!video) {
      console.log('ActivityVideoPlayer: No video element ref for playback control');
      return;
    }

    console.log('ActivityVideoPlayer: Playback control - isPlaying:', isPlaying, 'isPausedAtStep:', isPausedAtStep, 'video.readyState:', video.readyState);

    if (isPlaying && !isPausedAtStep) {
      console.log('ActivityVideoPlayer: Attempting to play video');
      video.play().catch(error => {
        console.error('ActivityVideoPlayer: Failed to play video:', error);
      });
    } else if (!isPlaying) {
      console.log('ActivityVideoPlayer: Pausing video');
      video.pause();
    } else {
      console.log('ActivityVideoPlayer: Video paused at step, not controlling playback');
    }
  }, [isPlaying, isPausedAtStep]);

  const setupVideoListeners = () => {
    const video = videoRef.current;
    if (!video) {
      console.log('ActivityVideoPlayer: No video element for listeners');
      return;
    }

    console.log('ActivityVideoPlayer: Setting up video event listeners');

    const handleTimeUpdate = () => {
      const currentTime = video.currentTime;
      console.log('ActivityVideoPlayer: Time update - currentTime:', currentTime);
      onTimeUpdate(currentTime);

      const currentStep = steps[currentStepIndex];
      if (currentStep && currentStep.pauseTimeSeconds) {
        // Check if we're approaching the pause time (within 0.5 seconds)
        const timeToPause = currentStep.pauseTimeSeconds - currentTime;
        console.log('ActivityVideoPlayer: Checking pause time - step:', currentStep.order, 'pauseTime:', currentStep.pauseTimeSeconds, 'timeToPause:', timeToPause, 'isPausedAtStep:', isPausedAtStep);
        if (timeToPause >= 0 && timeToPause < 0.5 && !isPausedAtStep) {
          console.log(`ActivityVideoPlayer: Pausing video at step ${currentStep.order} (time: ${currentTime}s, pause time: ${currentStep.pauseTimeSeconds}s)`);
          video.pause();
          onPause();
        }
      }
    };

    const handlePlay = () => {
      console.log('ActivityVideoPlayer: Video play event triggered');
      onPlay();
    };

    const handlePause = () => {
      console.log('ActivityVideoPlayer: Video pause event triggered');
      onPause();
    };

    const handleEnded = () => {
      console.log('ActivityVideoPlayer: Video ended event triggered');
      onEnded();
    };

    video.addEventListener('timeupdate', handleTimeUpdate);
    video.addEventListener('play', handlePlay);
    video.addEventListener('pause', handlePause);
    video.addEventListener('ended', handleEnded);

    console.log('ActivityVideoPlayer: Video listeners added');

    return () => {
      console.log('ActivityVideoPlayer: Cleaning up video listeners');
      video.removeEventListener('timeupdate', handleTimeUpdate);
      video.removeEventListener('play', handlePlay);
      video.removeEventListener('pause', handlePause);
      video.removeEventListener('ended', handleEnded);
    };
  };

  if (!videoUrl) {
    console.log('ActivityVideoPlayer: No video URL provided, component will not render');
    return null;
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
      console.log('ActivityVideoPlayer: Seeking to step time:', pauseTimeSeconds);
    }
  };

  console.log('ActivityVideoPlayer: Rendering video element with:', {
    src: videoUrl ? `${videoUrl.substring(0, 50)}...` : null,
    controls: !isPausedAtStep,
    crossOrigin: 'anonymous',
    preload: 'none',
    isPausedAtStep,
    currentStep: currentStep ? { id: currentStep.id, order: currentStep.order, description: currentStep.description.substring(0, 50) } : null
  });

  return (
    <Card sx={{ mb: 3 }}>
      <CardContent sx={{ p: isMobile ? 1 : 2 }}>
        <Box sx={{ position: 'relative' }}>
          <video
            ref={videoRef}
            src={videoUrl}
            controls={!isPausedAtStep}
            data-testid="activity-video"
            crossOrigin="anonymous"
            preload="none"
            style={{
              width: '100%',
              maxHeight: isMobile ? '300px' : '500px',
              aspectRatio: isMobile ? undefined : '16/9',
              borderRadius: theme.shape.borderRadius,
            }}
            onClick={() => {
              if (!isPlaying && !isPausedAtStep) {
                onPlay();
              }
            }}
            onLoadedData={() => {
              // Video loaded, can start playing
              const duration = videoRef.current?.duration || 0;
              setVideoDuration(duration);
              console.log('ActivityVideoPlayer: Video loaded successfully - duration:', duration, 'readyState:', videoRef.current?.readyState);
            }}
            onError={(e) => {
              const video = e.target as HTMLVideoElement;
              console.error('ActivityVideoPlayer: Video failed to load:', {
                error: e,
                videoSrc: video.src,
                errorCode: video.error?.code,
                errorMessage: video.error?.message,
                networkState: video.networkState,
                readyState: video.readyState
              });
              // Could dispatch an error action here if needed
            }}
            onLoadStart={() => {
              console.log('ActivityVideoPlayer: Video load started - src:', videoRef.current?.src);
            }}
            onCanPlay={() => {
              console.log('ActivityVideoPlayer: Video can play - duration:', videoRef.current?.duration, 'videoWidth:', videoRef.current?.videoWidth, 'videoHeight:', videoRef.current?.videoHeight);
            }}
            onStalled={() => {
              console.warn('ActivityVideoPlayer: Video stalled - networkState:', videoRef.current?.networkState, 'readyState:', videoRef.current?.readyState);
            }}
            onSuspend={() => {
              console.log('ActivityVideoPlayer: Video loading suspended');
            }}
            onWaiting={() => {
              console.log('ActivityVideoPlayer: Video waiting for data');
            }}
            onPlaying={() => {
              console.log('ActivityVideoPlayer: Video started playing');
            }}
            onSeeking={() => {
              console.log('ActivityVideoPlayer: Video seeking to:', videoRef.current?.currentTime);
            }}
            onSeeked={() => {
              console.log('ActivityVideoPlayer: Video seeked to:', videoRef.current?.currentTime);
            }}
            onProgress={() => {
              console.log('ActivityVideoPlayer: Video progress - buffered:', videoRef.current?.buffered);
            }}
            onAbort={() => {
              console.warn('ActivityVideoPlayer: Video loading aborted');
            }}
          />

          {/* Timeline markers for step timestamps */}
          {!isPausedAtStep && videoDuration > 0 && steps.some(step => step.pauseTimeSeconds) && (
            <Box
              sx={{
                position: 'absolute',
                bottom: isMobile ? '60px' : '50px', // Position above video controls
                left: 0,
                right: 0,
                height: '20px',
                pointerEvents: 'none', // Allow clicks to pass through to video controls
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
                          pointerEvents: 'auto', // Enable clicks on markers
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
              data-cy="step-overlay"
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