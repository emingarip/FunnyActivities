import React, { useRef, useEffect, useState, useCallback, forwardRef } from 'react';
import {
  Box,
  IconButton,
  Typography,
  Paper,
  Tooltip,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import {
  PlayArrow as PlayIcon,
  Pause as PauseIcon,
  VolumeUp as VolumeUpIcon,
  VolumeOff as VolumeOffIcon,
  Fullscreen as FullscreenIcon,
  FullscreenExit as FullscreenExitIcon,
  SkipNext as SkipNextIcon,
  SkipPrevious as SkipPreviousIcon,
  Add as AddIcon,
  RadioButtonChecked as MarkerIcon,
} from '@mui/icons-material';
import { TimelineMarker, VideoPlayerState } from '../../services/api.types';

interface VideoPlayerWithScrubberProps {
  src: string;
  markers: TimelineMarker[];
  onTimeChange: (time: number) => void;
  onMarkerClick: (marker: TimelineMarker) => void;
  onMarkerAdd: (time: number) => void;
  onStateChange?: (state: VideoPlayerState) => void;
  autoPlay?: boolean;
  controls?: boolean;
  width?: string | number;
  height?: string | number;
}

const VideoPlayerWithScrubber = forwardRef<HTMLVideoElement, VideoPlayerWithScrubberProps>(({
  src,
  markers,
  onTimeChange,
  onMarkerClick,
  onMarkerAdd,
  onStateChange,
  autoPlay = false,
  controls = true,
  width = '100%',
  height = 'auto',
}, ref) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const containerRef = useRef<HTMLDivElement>(null);

  const [playerState, setPlayerState] = useState<VideoPlayerState>({
    currentTime: 0,
    duration: 0,
    isPlaying: false,
    isPaused: true,
    volume: 1,
    playbackRate: 1,
    buffered: {} as TimeRanges,
  });

  const [isFullscreen, setIsFullscreen] = useState(false);
  const [showControls, setShowControls] = useState(true);

  // Update player state
  const updatePlayerState = useCallback((updates: Partial<VideoPlayerState>) => {
    setPlayerState(prev => {
      const newState = { ...prev, ...updates };
      onStateChange?.(newState);
      return newState;
    });
  }, [onStateChange]);

  // Video event handlers
  useEffect(() => {
    const video = (ref as React.RefObject<HTMLVideoElement>)?.current;
    if (!video) return;

    const handleLoadedMetadata = () => {
      updatePlayerState({
        duration: video.duration,
        buffered: video.buffered,
      });
    };

    const handleTimeUpdate = () => {
      updatePlayerState({
        currentTime: video.currentTime,
      });
      onTimeChange(video.currentTime);
    };

    const handlePlay = () => {
      updatePlayerState({
        isPlaying: true,
        isPaused: false,
      });
    };

    const handlePause = () => {
      updatePlayerState({
        isPlaying: false,
        isPaused: true,
      });
    };

    const handleVolumeChange = () => {
      updatePlayerState({
        volume: video.volume,
      });
    };

    const handleRateChange = () => {
      updatePlayerState({
        playbackRate: video.playbackRate,
      });
    };

    // Keyboard shortcuts for accessibility
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.target !== video && !(event.target as HTMLElement)?.closest('[role="button"]')) {
        return;
      }

      switch (event.code) {
        case 'Space':
          event.preventDefault();
          togglePlay();
          break;
        case 'ArrowLeft':
          event.preventDefault();
          skipTime(-10);
          break;
        case 'ArrowRight':
          event.preventDefault();
          skipTime(10);
          break;
        case 'KeyM':
          event.preventDefault();
          toggleMute();
          break;
        case 'KeyF':
          event.preventDefault();
          toggleFullscreen();
          break;
        case 'Digit1':
        case 'Digit2':
        case 'Digit3':
        case 'Digit4':
        case 'Digit5':
        case 'Digit6':
        case 'Digit7':
        case 'Digit8':
        case 'Digit9':
          event.preventDefault();
          const percentage = parseInt(event.code.slice(-1)) * 10;
          const newTime = (percentage / 100) * playerState.duration;
          if (video) {
            video.currentTime = newTime;
          }
          break;
      }
    };

    video.addEventListener('loadedmetadata', handleLoadedMetadata);
    video.addEventListener('timeupdate', handleTimeUpdate);
    video.addEventListener('play', handlePlay);
    video.addEventListener('pause', handlePause);
    video.addEventListener('volumechange', handleVolumeChange);
    video.addEventListener('ratechange', handleRateChange);
    document.addEventListener('keydown', handleKeyDown);

    return () => {
      video.removeEventListener('loadedmetadata', handleLoadedMetadata);
      video.removeEventListener('timeupdate', handleTimeUpdate);
      video.removeEventListener('play', handlePlay);
      video.removeEventListener('pause', handlePause);
      video.removeEventListener('volumechange', handleVolumeChange);
      video.removeEventListener('ratechange', handleRateChange);
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [updatePlayerState, onTimeChange, playerState.duration]);

  // Fullscreen handling
  useEffect(() => {
    const handleFullscreenChange = () => {
      setIsFullscreen(!!document.fullscreenElement);
    };

    document.addEventListener('fullscreenchange', handleFullscreenChange);
    return () => document.removeEventListener('fullscreenchange', handleFullscreenChange);
  }, []);

  // Control visibility
  useEffect(() => {
    let timeout: NodeJS.Timeout;
    if (playerState.isPlaying && !isMobile) {
      timeout = setTimeout(() => setShowControls(false), 3000);
    } else {
      setShowControls(true);
    }

    return () => clearTimeout(timeout);
  }, [playerState.isPlaying, isMobile]);

  // Playback controls
  const togglePlay = () => {
    const video = (ref as React.RefObject<HTMLVideoElement>)?.current;
    if (!video) return;

    if (playerState.isPlaying) {
      video.pause();
    } else {
      video.play();
    }
  };


  const toggleMute = () => {
    const video = (ref as React.RefObject<HTMLVideoElement>)?.current;
    if (video) {
      video.muted = !video.muted;
    }
  };

  const toggleFullscreen = () => {
    const container = containerRef.current;
    if (!container) return;

    if (!isFullscreen) {
      container.requestFullscreen?.();
    } else {
      document.exitFullscreen?.();
    }
  };

  const skipTime = (seconds: number) => {
    const video = (ref as React.RefObject<HTMLVideoElement>)?.current;
    if (video) {
      const newTime = Math.max(0, Math.min(video.duration, video.currentTime + seconds));
      video.currentTime = newTime;
    }
  };

  const addMarkerAtCurrentTime = () => {
    onMarkerAdd(playerState.currentTime);
  };

  // Format time display
  const formatTime = (time: number): string => {
    const minutes = Math.floor(time / 60);
    const seconds = Math.floor(time % 60);
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  };

  // Calculate marker positions
  const getMarkerPosition = (time: number): number => {
    return (time / playerState.duration) * 100;
  };

  return (
    <Paper
      ref={containerRef}
      sx={{
        position: 'relative',
        width: width,
        height: height,
        bgcolor: 'black',
        overflow: 'hidden',
        cursor: showControls ? 'default' : 'none',
      }}
      onMouseMove={() => setShowControls(true)}
      onMouseLeave={() => playerState.isPlaying && !isMobile && setShowControls(false)}
    >
      {/* Video Element */}
      <video
        ref={ref}
        src={src}
        autoPlay={autoPlay}
        style={{
          width: '100%',
          height: '100%',
          objectFit: 'contain',
        }}
        onClick={togglePlay}
      />

      {/* Video Timeline Bar */}
      {playerState.duration > 0 && (
        <Box
          sx={{
            position: 'absolute',
            bottom: showControls ? (isMobile ? '80px' : '70px') : (isMobile ? '60px' : '50px'),
            left: 0,
            right: 0,
            height: '40px',
            bgcolor: 'rgba(0, 0, 0, 0.7)',
            display: 'flex',
            alignItems: 'center',
            px: 2,
            zIndex: 1,
          }}
        >
          {/* Timeline Progress Bar */}
          <Box sx={{ flex: 1, position: 'relative', height: '6px', bgcolor: 'rgba(255, 255, 255, 0.3)', borderRadius: '3px' }}>
            {/* Progress Fill */}
            <Box
              sx={{
                position: 'absolute',
                left: 0,
                top: 0,
                height: '100%',
                width: `${(playerState.currentTime / playerState.duration) * 100}%`,
                bgcolor: theme.palette.primary.main,
                borderRadius: '3px',
                transition: 'width 0.1s ease-out',
              }}
            />

            {/* Step Markers on Timeline */}
            {markers.map((marker) => (
              <Tooltip
                key={marker.id}
                title={
                  <Box>
                    <Typography variant="body2" sx={{ fontWeight: 'bold' }}>
                      {marker.label}
                    </Typography>
                    <Typography variant="body2" sx={{ mt: 0.5 }}>
                      {formatTime(marker.time)} ({marker.time.toFixed(1)}s)
                    </Typography>
                    {marker.data && (
                      <Typography variant="body2" sx={{ mt: 0.5, fontSize: '0.75rem', color: 'text.secondary' }}>
                        {marker.data.description?.substring(0, 50)}{marker.data.description?.length > 50 ? '...' : ''}
                      </Typography>
                    )}
                  </Box>
                }
                arrow
                placement="top"
              >
                <IconButton
                  size="small"
                  sx={{
                    position: 'absolute',
                    left: `${getMarkerPosition(marker.time)}%`,
                    top: '50%',
                    transform: 'translate(-50%, -50%)',
                    color: marker.color || theme.palette.primary.main,
                    backgroundColor: 'rgba(255, 255, 255, 0.9)',
                    border: `2px solid ${marker.color || theme.palette.primary.main}`,
                    width: isMobile ? '14px' : '18px',
                    height: isMobile ? '14px' : '18px',
                    minWidth: 'auto',
                    padding: 0,
                    pointerEvents: 'auto',
                    '&:hover': {
                      backgroundColor: marker.color || theme.palette.primary.main,
                      color: 'white',
                      transform: 'translate(-50%, -50%) scale(1.3)',
                      zIndex: 2,
                    },
                    transition: 'all 0.2s ease-in-out',
                  }}
                  onClick={(e) => {
                    e.stopPropagation();
                    onMarkerClick(marker);
                    const video = (ref as React.RefObject<HTMLVideoElement>)?.current;
                    if (video) {
                      video.currentTime = marker.time;
                      updatePlayerState({ currentTime: marker.time });
                      onTimeChange(marker.time);
                    }
                  }}
                  aria-label={`Jump to ${marker.label} at ${formatTime(marker.time)}`}
                >
                  <MarkerIcon sx={{ fontSize: isMobile ? '10px' : '12px' }} />
                </IconButton>
              </Tooltip>
            ))}

            {/* Clickable Timeline for Seeking */}
            <Box
              sx={{
                position: 'absolute',
                left: 0,
                top: 0,
                width: '100%',
                height: '100%',
                cursor: 'pointer',
                '&:hover': {
                  '&::after': {
                    content: '""',
                    position: 'absolute',
                    top: '-4px',
                    height: '14px',
                    width: '2px',
                    bgcolor: 'white',
                    left: 'var(--hover-position, 0%)',
                    transform: 'translateX(-50%)',
                  }
                }
              }}
              onClick={(e) => {
                const rect = e.currentTarget.getBoundingClientRect();
                const clickX = e.clientX - rect.left;
                const percentage = clickX / rect.width;
                const newTime = percentage * playerState.duration;

                const video = (ref as React.RefObject<HTMLVideoElement>)?.current;
                if (video) {
                  video.currentTime = newTime;
                  updatePlayerState({ currentTime: newTime });
                  onTimeChange(newTime);
                }
              }}
              onMouseMove={(e) => {
                const rect = e.currentTarget.getBoundingClientRect();
                const hoverX = e.clientX - rect.left;
                const percentage = hoverX / rect.width;
                e.currentTarget.style.setProperty('--hover-position', `${percentage * 100}%`);
              }}
            />
          </Box>

          {/* Timeline Time Display */}
          <Typography
            variant="caption"
            sx={{
              ml: 2,
              color: 'white',
              fontSize: isMobile ? '0.7rem' : '0.75rem',
              minWidth: '80px',
              textAlign: 'right'
            }}
          >
            {formatTime(playerState.currentTime)} / {formatTime(playerState.duration)}
          </Typography>
        </Box>
      )}

      {/* Controls Overlay */}
      {controls && showControls && (
        <Box
          sx={{
            position: 'absolute',
            bottom: 0,
            left: 0,
            right: 0,
            bgcolor: 'rgba(0, 0, 0, 0.8)',
            p: 2,
            transition: 'opacity 0.3s',
          }}
          role="region"
          aria-label="Video player controls"
        >

          {/* Control Buttons */}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
            <IconButton
              onClick={() => skipTime(-10)}
              color="inherit"
              size="small"
              aria-label="Skip backward 10 seconds"
            >
              <SkipPreviousIcon />
            </IconButton>

            <IconButton
              onClick={togglePlay}
              color="inherit"
              size="large"
              aria-label={playerState.isPlaying ? 'Pause video' : 'Play video'}
            >
              {playerState.isPlaying ? <PauseIcon /> : <PlayIcon />}
            </IconButton>

            <IconButton
              onClick={() => skipTime(10)}
              color="inherit"
              size="small"
              aria-label="Skip forward 10 seconds"
            >
              <SkipNextIcon />
            </IconButton>

            <IconButton
              onClick={toggleMute}
              color="inherit"
              size="small"
              aria-label={playerState.volume === 0 ? 'Unmute video' : 'Mute video'}
            >
              {playerState.volume === 0 ? <VolumeOffIcon /> : <VolumeUpIcon />}
            </IconButton>

            <Tooltip title="Add step marker at current time (Keyboard: Ctrl+Click)">
              <IconButton
                onClick={addMarkerAtCurrentTime}
                color="inherit"
                size="small"
                aria-label="Add step marker at current time"
              >
                <AddIcon />
              </IconButton>
            </Tooltip>

            <Box sx={{ flex: 1, minWidth: 20 }} />

            <Typography variant="caption" color="white" sx={{ mr: 1 }}>
              Keyboard shortcuts: Space (play/pause), ←→ (seek), M (mute), F (fullscreen), 1-9 (jump to %)
            </Typography>

            <IconButton
              onClick={toggleFullscreen}
              color="inherit"
              size="small"
              aria-label={isFullscreen ? 'Exit fullscreen' : 'Enter fullscreen'}
            >
              {isFullscreen ? <FullscreenExitIcon /> : <FullscreenIcon />}
            </IconButton>
          </Box>
        </Box>
      )}

      {/* Loading Indicator */}
      {playerState.duration === 0 && (
        <Box
          sx={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            color: 'white',
          }}
        >
          <Typography>Loading video...</Typography>
        </Box>
      )}
    </Paper>
  );
});

VideoPlayerWithScrubber.displayName = 'VideoPlayerWithScrubber';

export default VideoPlayerWithScrubber;