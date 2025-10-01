import React, { useRef, useEffect, useState, useCallback, forwardRef } from 'react';
import {
  Box,
  IconButton,
  Slider,
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
  const [isScrubbing, setIsScrubbing] = useState(false);

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
      if (!isScrubbing) {
        updatePlayerState({
          currentTime: video.currentTime,
        });
        onTimeChange(video.currentTime);
      }
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
  }, [isScrubbing, updatePlayerState, onTimeChange, playerState.duration]);

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

  const handleSeek = (_: Event, newValue: number | number[]) => {
    const time = Array.isArray(newValue) ? newValue[0] : newValue;
    const video = (ref as React.RefObject<HTMLVideoElement>)?.current;
    if (video) {
      video.currentTime = time;
      updatePlayerState({ currentTime: time });
      onTimeChange(time);
    }
  };

  const handleScrubStart = () => {
    setIsScrubbing(true);
  };

  const handleScrubEnd = () => {
    setIsScrubbing(false);
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

      {/* Timeline Markers Overlay */}
      <Box
        sx={{
          position: 'absolute',
          bottom: showControls ? '80px' : '40px',
          left: 0,
          right: 0,
          height: '20px',
          pointerEvents: 'none',
        }}
      >
        {markers.map((marker) => (
          <Tooltip key={marker.id} title={marker.label} arrow>
            <Box
              sx={{
                position: 'absolute',
                left: `${getMarkerPosition(marker.time)}%`,
                top: 0,
                width: '4px',
                height: '20px',
                bgcolor: marker.color || theme.palette.primary.main,
                cursor: 'pointer',
                pointerEvents: 'auto',
                transform: 'translateX(-50%)',
                borderRadius: '2px',
                '&:hover': {
                  width: '6px',
                  height: '24px',
                },
              }}
              onClick={(e) => {
                e.stopPropagation();
                onMarkerClick(marker);
                const video = (ref as React.RefObject<HTMLVideoElement>)?.current;
                if (video) {
                  video.currentTime = marker.time;
                }
              }}
            />
          </Tooltip>
        ))}
      </Box>

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
          {/* Progress Bar */}
          <Box sx={{ mb: 2 }}>
            <Slider
              value={playerState.currentTime}
              max={playerState.duration || 100}
              onChange={handleSeek}
              onChangeCommitted={handleScrubEnd}
              onMouseDown={handleScrubStart}
              aria-label="Video progress"
              sx={{
                color: theme.palette.primary.main,
                '& .MuiSlider-thumb': {
                  width: 16,
                  height: 16,
                },
              }}
            />
            <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 1 }}>
              <Typography
                variant="caption"
                color="white"
                aria-live="polite"
                aria-label={`Current time: ${formatTime(playerState.currentTime)}`}
              >
                {formatTime(playerState.currentTime)}
              </Typography>
              <Typography
                variant="caption"
                color="white"
                aria-label={`Total duration: ${formatTime(playerState.duration)}`}
              >
                {formatTime(playerState.duration)}
              </Typography>
            </Box>
          </Box>

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