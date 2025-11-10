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
  Chip,
} from '@mui/material';
import { PlayArrow as PlayIcon, RadioButtonChecked as MarkerIcon, VolumeUp as VolumeIcon, VolumeOff as VolumeOffIcon, Fullscreen as FullscreenIcon, Favorite as FavoriteIcon, FavoriteBorder as FavoriteBorderIcon } from '@mui/icons-material';

interface ActivityStep {
  id: string;
  order: number;
  description: string;
  timestampSeconds: number;
}

interface Activity {
  id: string;
  name: string;
  description?: string;
  activityCategoryId?: string;
  activityCategory?: {
    id: string;
    name: string;
  };
}

interface ActivityVideoPlayerProps {
  activity: Activity;
  videoUrl: string | null;
  introVideoUrl?: string | null;
  steps: ActivityStep[];
  currentStepIndex: number;
  isPlaying: boolean;
  isPausedAtStep: boolean;
  onPlay: () => void;
  onPause: () => void;
  onContinue: () => void;
  onReplayStep?: () => void;
  onTimeUpdate: (currentTime: number) => void;
  onStepReached: (stepIndex: number) => void;
  onEnded: () => void;
  showFavoriteButton?: boolean;
  isFavorite?: boolean;
  onToggleFavorite?: () => void;
}

const ActivityVideoPlayer: React.FC<ActivityVideoPlayerProps> = ({
  activity,
  videoUrl,
  introVideoUrl,
  steps,
  currentStepIndex,
  isPlaying,
  isPausedAtStep,
  onPlay,
  onPause,
  onContinue,
  onReplayStep,
  onTimeUpdate,
  onStepReached,
  onEnded,
  showFavoriteButton = false,
  isFavorite = false,
  onToggleFavorite,
}) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const videoRef = useRef<HTMLVideoElement>(null);
  const introVideoRef = useRef<HTMLVideoElement>(null);
  const [videoDuration, setVideoDuration] = useState<number>(0);
  const [currentTime, setCurrentTime] = useState<number>(0);
  const [introCompleted, setIntroCompleted] = useState(!introVideoUrl);
  const [shouldAutoPlayMain, setShouldAutoPlayMain] = useState(false);
  const [showUnmuteButton, setShowUnmuteButton] = useState(false);
  const [introVideoPlaying, setIntroVideoPlaying] = useState(false);
  const [introAutoplayAllowed, setIntroAutoplayAllowed] = useState(false);
  const [isMuted, setIsMuted] = useState(false);
  const [isFullscreen, setIsFullscreen] = useState(false);
  const nextPauseIndexRef = useRef(0);

  useEffect(() => {
    setIntroCompleted(!introVideoUrl);
    setShouldAutoPlayMain(false);
  }, [introVideoUrl]);

useEffect(() => {
  nextPauseIndexRef.current = 0;
}, [steps]);

  const isIntroPhase = Boolean(introVideoUrl) && !introCompleted;
  const showControls = !isPausedAtStep && !isIntroPhase;

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

  // Handle fullscreen changes
  useEffect(() => {
    const handleFullscreenChange = () => {
      setIsFullscreen(!!document.fullscreenElement);
    };

    document.addEventListener('fullscreenchange', handleFullscreenChange);
    return () => document.removeEventListener('fullscreenchange', handleFullscreenChange);
  }, []);

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

  // Auto-play main video when loaded (after intro or directly)
  useEffect(() => {
    if (isIntroPhase || !videoUrl || isPausedAtStep) {
      return;
    }

    const video = videoRef.current;
    if (!video) {
      return;
    }

    // Auto-play when video is loaded and ready
    const handleCanPlay = () => {
      if (!isPlaying && !isPausedAtStep) {
        onPlay();
      }
    };

    video.addEventListener('canplay', handleCanPlay);
    return () => video.removeEventListener('canplay', handleCanPlay);
  }, [isIntroPhase, videoUrl, isPausedAtStep, isPlaying, onPlay]);

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
      setCurrentTime(currentTime);
      onTimeUpdate(currentTime);

      if (isPausedAtStep) {
        return;
      }

      const targetIndex = nextPauseIndexRef.current;
      if (targetIndex >= steps.length) {
        return;
      }

      const targetStep = steps[targetIndex];
      if (currentTime >= targetStep.timestampSeconds) {
        nextPauseIndexRef.current = targetIndex + 1;
        video.pause();
        onStepReached(targetIndex);
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
    setShowUnmuteButton(true);

    // Always start main video when intro completes (either by ending or skipping)
    setTimeout(() => {
      onPlay();
    }, 100); // Small delay to ensure state updates
  };

  const handleIntroPlay = () => {
    setIntroVideoPlaying(true);
  };

  const handleIntroPlayClick = () => {
    if (introVideoRef.current) {
      introVideoRef.current.play().catch(error => console.error('ActivityVideoPlayer: intro play failed', error));
      setIntroVideoPlaying(true);
    }
  };

  const handleUnmute = () => {
    if (videoRef.current) {
      videoRef.current.muted = false;
      videoRef.current.volume = 1;
      setShowUnmuteButton(false);
    }
  };

  if (isIntroPhase && introVideoUrl) {
    return (
      <Card sx={{ mb: 3 }}>
        <CardContent sx={{ p: isMobile ? 1 : 2 }}>
          <Box sx={{
            position: 'relative',
            width: '100%',
            height: isMobile ? '200px' : '320px',
            overflow: 'hidden',
            bgcolor: 'black',
            borderRadius: 1,
          }}>
            <video
              data-testid="intro-video"
              ref={introVideoRef}
              src={introVideoUrl}
              autoPlay
              crossOrigin="anonymous"
              preload="auto"
              style={{
                width: '100%',
                height: '100%',
                aspectRatio: '16/9',
                borderRadius: theme.shape.borderRadius,
                objectFit: 'contain',
              }}
              onEnded={() => handleIntroComplete(true)}
              onPlay={handleIntroPlay}
            />
            <Box
              sx={{
                position: 'absolute',
                bottom: 16,
                right: 10,
                pointerEvents: 'none',
              }}
            >
              <Button
                variant="contained"
                color="secondary"
                size="small"
                onClick={() => handleIntroComplete(false)}
                sx={{
                  pointerEvents: 'auto',
                  fontSize: { xs: '0.625rem', sm: '0.875rem' },
                  padding: { xs: '2px 6px', sm: '6px 16px' },
                  minWidth: { xs: 'auto', sm: '64px' },
                  height: { xs: '24px', sm: '32px' },
                }}
              >
                Skip Intro
              </Button>
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

  const hasSteps = steps.length > 0;
  const safeStepIndex = hasSteps ? Math.min(Math.max(currentStepIndex, 0), steps.length - 1) : 0;
  const currentStep = hasSteps ? steps[safeStepIndex] : null;

  const formatTime = (seconds: number): string => {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = Math.floor(seconds % 60);
    return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
  };

  const handleMarkerClick = (timestampSeconds: number) => {
    if (videoRef.current) {
      videoRef.current.currentTime = timestampSeconds;
      const markerIndex = steps.findIndex(step => step.timestampSeconds >= timestampSeconds - 0.1);
      nextPauseIndexRef.current = markerIndex === -1 ? steps.length : markerIndex;
    }
  };

  return (
    <Card sx={{ mb: isMobile ? 3 : 1, mx: isMobile ? 1 : 2, backgroundColor: 'white', boxShadow: 1 }}>
      <CardContent sx={{ p: 0 }}>
        {/* Activity Header Information */}
        <Box sx={{ mb: 0, pl: isMobile ? 1 : 2, pr: isMobile ? 1 : 2, pt: 0, pb: 0 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 2 }}>
            <Box sx={{ flex: 1, display: 'flex', alignItems: 'center', gap: 2 }}>
              <Typography
                variant={isMobile ? 'h5' : 'h4'}
                component="h1"
                sx={{ wordBreak: 'break-word' }}
                data-cy="activity-title"
              >
                {activity.name}
              </Typography>
              {activity.description && (
                <Typography
                  variant="body1"
                  color="text.secondary"
                  sx={{ wordBreak: 'break-word' }}
                  data-cy="activity-description"
                >
                  {activity.description}
                </Typography>
              )}
            </Box>
            {activity.activityCategory && (
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexShrink: 0 }}>
                <Typography variant="body2" color="text.secondary">
                  Category:
                </Typography>
                <Chip label={activity.activityCategory.name} size="small" />
              </Box>
            )}
          </Box>
        </Box>

        <Box sx={{
          position: 'relative',
          width: '100%',
          height: isFullscreen ? '100vh' : (isMobile ? '200px' : '320px'),
          overflow: 'hidden',
          bgcolor: 'black',
          borderRadius: isFullscreen ? 0 : 1,
        }}>
          {/* Favorite Button - Top Right */}
          {showFavoriteButton && onToggleFavorite && (
            <IconButton
              onClick={onToggleFavorite}
              sx={{
                position: 'absolute',
                top: 16,
                right: 16,
                zIndex: 15,
                bgcolor: 'rgba(0, 0, 0, 0.6)',
                color: 'white',
                '&:hover': {
                  bgcolor: 'rgba(0, 0, 0, 0.8)',
                },
                width: 48,
                height: 48,
              }}
              size="large"
            >
              {isFavorite ? (
                <FavoriteIcon color="error" fontSize="large" />
              ) : (
                <FavoriteBorderIcon fontSize="large" />
              )}
            </IconButton>
          )}

          <video
            data-testid="activity-video"
            ref={videoRef}
            src={videoUrl}
            crossOrigin="anonymous"
            preload="none"
            style={{
              width: '100%',
              height: '100%',
              aspectRatio: '16/9',
              borderRadius: isFullscreen ? 0 : theme.shape.borderRadius,
              objectFit: 'contain',
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

          {/* Bottom Controls Bar - Timeline and Custom Controls in same row */}
          {showControls && videoDuration > 0 && (
            <Box
              sx={{
                position: 'absolute',
                bottom: isMobile ? 8 : 16,
                left: 16,
                right: 16,
                display: 'flex',
                alignItems: 'center',
                gap: 2,
                zIndex: 10,
              }}
            >
              {/* Timeline */}
              <Box sx={{ flex: 1 }}>
                <Box
                  sx={{
                    position: 'relative',
                    height: '8px',
                    backgroundColor: 'rgba(255, 255, 255, 0.2)',
                    borderRadius: '4px',
                    cursor: 'pointer',
                    '&:hover': {
                      backgroundColor: 'rgba(255, 255, 255, 0.3)',
                    },
                  }}
                  onClick={(e) => {
                    if (videoRef.current && videoDuration > 0) {
                      const rect = e.currentTarget.getBoundingClientRect();
                      const clickX = e.clientX - rect.left;
                      const percentage = clickX / rect.width;
                      const newTime = percentage * videoDuration;
                      videoRef.current.currentTime = Math.max(0, Math.min(newTime, videoDuration));
                    }
                  }}
                >
                  {/* Progress bar */}
                  <Box
                    sx={{
                      position: 'absolute',
                      left: 0,
                      top: 0,
                      height: '100%',
                      backgroundColor: theme.palette.primary.main,
                      borderRadius: '4px',
                      width: videoDuration > 0 ? `${(currentTime / videoDuration) * 100}%` : '0%',
                      transition: 'width 0.1s ease',
                    }}
                  />

                  {/* Step markers */}
                  {steps.map((step, index) => {
                    if (!step.timestampSeconds) return null;
                    const position = (step.timestampSeconds / videoDuration) * 100;
                    return (
                      <Box
                        key={`timeline-marker-${index}`}
                        sx={{
                          position: 'absolute',
                          left: `${position}%`,
                          top: '50%',
                          transform: 'translate(-50%, -50%)',
                          width: '12px',
                          height: '12px',
                          backgroundColor: currentStepIndex === index ? theme.palette.secondary.main : 'white',
                          border: `2px solid ${theme.palette.primary.main}`,
                          borderRadius: '50%',
                          cursor: 'pointer',
                          zIndex: 2,
                          '&:hover': {
                            transform: 'translate(-50%, -50%) scale(1.2)',
                          },
                        }}
                        onClick={(e) => {
                          e.stopPropagation();
                          if (videoRef.current) {
                            videoRef.current.currentTime = step.timestampSeconds;
                            if (onStepReached) {
                              onStepReached(index);
                            }
                          }
                        }}
                        title={`Jump to step ${index + 1} at ${Math.floor(step.timestampSeconds / 60)}:${(step.timestampSeconds % 60).toFixed(0).padStart(2, '0')}`}
                      />
                    );
                  })}
                </Box>

                {/* Time display removed as requested */}
              </Box>

              {/* Custom Controls */}
              <Box sx={{ display: 'flex', gap: 1 }} className="video-player-controls">
                <IconButton
                  onClick={() => {
                    if (videoRef.current) {
                      const newMuted = !videoRef.current.muted;
                      videoRef.current.muted = newMuted;
                      setIsMuted(newMuted);
                    }
                  }}
                  sx={{
                    bgcolor: 'rgba(0, 0, 0, 0.6)',
                    color: 'white',
                    '&:hover': {
                      bgcolor: 'rgba(0, 0, 0, 0.8)',
                    },
                    width: isMobile ? 28 : 40,
                    height: isMobile ? 28 : 40,
                    padding: isMobile ? '4px' : '8px',
                  }}
                  size={isMobile ? 'small' : 'small'}
                >
                  {isMuted ? <VolumeOffIcon fontSize={isMobile ? 'inherit' : 'small'} /> : <VolumeIcon fontSize={isMobile ? 'inherit' : 'small'} />}
                </IconButton>
                <IconButton
                  onClick={() => {
                    if (videoRef.current) {
                      if (document.fullscreenElement) {
                        document.exitFullscreen();
                        setIsFullscreen(false);
                      } else {
                        // Create a container for fullscreen that includes video and timeline
                        const container = videoRef.current.parentElement;
                        if (container) {
                          container.requestFullscreen();
                          setIsFullscreen(true);
                        }
                      }
                    }
                  }}
                  sx={{
                    bgcolor: 'rgba(0, 0, 0, 0.6)',
                    color: 'white',
                    '&:hover': {
                      bgcolor: 'rgba(0, 0, 0, 0.8)',
                    },
                    width: isMobile ? 28 : 40,
                    height: isMobile ? 28 : 40,
                    padding: isMobile ? '4px' : '8px',
                  }}
                  size={isMobile ? 'small' : 'small'}
                >
                  <FullscreenIcon fontSize={isMobile ? 'inherit' : 'small'} />
                </IconButton>
              </Box>
            </Box>
          )}

          {/* Timeline only when controls are hidden */}
          {videoDuration > 0 && !showControls && (
            <Box
              sx={{
                position: 'absolute',
                bottom: isMobile ? 8 : 16,
                left: 16,
                right: 16,
                zIndex: 5,
              }}
            >
              <Box
                sx={{
                  position: 'relative',
                  height: '8px',
                  backgroundColor: 'rgba(255, 255, 255, 0.2)',
                  borderRadius: '4px',
                  cursor: 'pointer',
                  '&:hover': {
                    backgroundColor: 'rgba(255, 255, 255, 0.3)',
                  },
                }}
                onClick={(e) => {
                  if (videoRef.current && videoDuration > 0) {
                    const rect = e.currentTarget.getBoundingClientRect();
                    const clickX = e.clientX - rect.left;
                    const percentage = clickX / rect.width;
                    const newTime = percentage * videoDuration;
                    videoRef.current.currentTime = Math.max(0, Math.min(newTime, videoDuration));
                  }
                }}
              >
                {/* Progress bar */}
                <Box
                  sx={{
                    position: 'absolute',
                    left: 0,
                    top: 0,
                    height: '100%',
                    backgroundColor: theme.palette.primary.main,
                    borderRadius: '4px',
                    width: videoDuration > 0 ? `${(currentTime / videoDuration) * 100}%` : '0%',
                    transition: 'width 0.1s ease',
                  }}
                />

                {/* Step markers */}
                {steps.map((step, index) => {
                  if (!step.timestampSeconds) return null;
                  const position = (step.timestampSeconds / videoDuration) * 100;
                  return (
                    <Box
                      key={`timeline-marker-${index}`}
                      sx={{
                        position: 'absolute',
                        left: `${position}%`,
                        top: '50%',
                        transform: 'translate(-50%, -50%)',
                        width: '12px',
                        height: '12px',
                        backgroundColor: currentStepIndex === index ? theme.palette.secondary.main : 'white',
                        border: `2px solid ${theme.palette.primary.main}`,
                        borderRadius: '50%',
                        cursor: 'pointer',
                        zIndex: 2,
                        '&:hover': {
                          transform: 'translate(-50%, -50%) scale(1.2)',
                        },
                      }}
                      onClick={(e) => {
                        e.stopPropagation();
                        if (videoRef.current) {
                          videoRef.current.currentTime = step.timestampSeconds;
                          if (onStepReached) {
                            onStepReached(index);
                          }
                        }
                      }}
                      title={`Jump to step ${index + 1} at ${Math.floor(step.timestampSeconds / 60)}:${(step.timestampSeconds % 60).toFixed(0).padStart(2, '0')}`}
                    />
                  );
                })}
              </Box>

              {/* Time display removed as requested */}
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
          <Box sx={{ display: 'flex', gap: 2, flexDirection: isMobile ? 'column' : 'row' }}>
            <Button
              data-cy="continue-button"
              variant="contained"
              size={isMobile ? 'medium' : 'large'}
              onClick={() => {
                const nextIndex = Math.min(safeStepIndex + 1, steps.length);
                nextPauseIndexRef.current = Math.max(nextPauseIndexRef.current, nextIndex);
                const video = videoRef.current;
                onContinue();
                if (video) {
                  const epsilon = 0.05;
                  const targetTime = Math.min(video.currentTime + epsilon, videoDuration || video.currentTime + epsilon);
                  video.currentTime = targetTime;
                  video.play().catch(error => console.error('ActivityVideoPlayer: continue play failed', error));
                }
              }}
              startIcon={<PlayIcon />}
              sx={{
                minWidth: isMobile ? '120px' : '160px',
                minHeight: 44,
                fontSize: isMobile ? '1rem' : '1rem',
              }}
            >
              Continue Activity
            </Button>

            {onReplayStep && (
              <Button
                variant="outlined"
                size={isMobile ? 'medium' : 'large'}
                onClick={() => {
                  const video = videoRef.current;
                  if (video) {
                    // Always start from the beginning of current step (previous step's end or video start)
                    const replayStartTime = safeStepIndex > 0 ? steps[safeStepIndex - 1].timestampSeconds : 0;
                    video.currentTime = replayStartTime;
                    nextPauseIndexRef.current = safeStepIndex; // Reset to current step
                    onReplayStep();
                    video.play().catch(error => console.error('ActivityVideoPlayer: replay step failed', error));
                  }
                }}
                startIcon={<PlayIcon />}
                sx={{
                  minWidth: isMobile ? '120px' : '160px',
                  minHeight: 44,
                  fontSize: isMobile ? '1rem' : '1rem',
                }}
              >
                Replay Step
              </Button>
            )}
          </Box>
        </Box>
          )}
        </Box>
      </CardContent>
    </Card>
  );
};

export default ActivityVideoPlayer;
