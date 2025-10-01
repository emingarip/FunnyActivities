import React, { useRef, useEffect, useState, useCallback } from 'react';
import { Box, CircularProgress, Typography, Button, Snackbar, Alert } from '@mui/material';
import { Refresh as RefreshIcon, Error as ErrorIcon } from '@mui/icons-material';
import videoAnalyticsService from '../../services/videoAnalytics';
import VideoUtils, { VideoSource } from '../../services/videoUtils';

// Export empty object to make this a module
export {};

interface LazyVideoLoaderProps {
  src: string;
  poster?: string;
  autoPlay?: boolean;
  muted?: boolean;
  loop?: boolean;
  controls?: boolean;
  width?: string | number;
  height?: string | number;
  onLoad?: () => void;
  onError?: (error: Error) => void;
  onPlay?: () => void;
  onPause?: () => void;
  preloadStrategy?: 'none' | 'metadata' | 'auto';
  priority?: 'low' | 'medium' | 'high';
  className?: string;
}

interface PreloadConfig {
  threshold: number; // Distance from viewport to start preloading
  rootMargin: string;
  priority: 'low' | 'medium' | 'high';
}

const LazyVideoLoader: React.FC<LazyVideoLoaderProps> = ({
  src,
  poster,
  autoPlay = true,
  muted = true,
  loop = true,
  controls = true,
  width = '100%',
  height = 'auto',
  onLoad,
  onError,
  onPlay,
  onPause,
  preloadStrategy = 'metadata',
  priority = 'medium',
  className,
}) => {
  const videoRef = useRef<HTMLVideoElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const observerRef = useRef<IntersectionObserver | null>(null);
  const preloadObserverRef = useRef<IntersectionObserver | null>(null);

  const [isLoading, setIsLoading] = useState(true);
  const [hasError, setHasError] = useState(false);
  const [isInView, setIsInView] = useState(false);
  const [isPreloaded, setIsPreloaded] = useState(false);
  const [loadStartTime, setLoadStartTime] = useState<number | null>(null);

  // Enhanced error handling
  const [videoError, setVideoError] = useState<{
    type: 'validation' | 'loading' | 'autoplay' | 'format' | 'network' | 'unknown';
    message: string;
    details?: string;
    retryable: boolean;
  } | null>(null);
  const [retryCount, setRetryCount] = useState(0);
  const [showErrorSnackbar, setShowErrorSnackbar] = useState(false);
  const [validationResult, setValidationResult] = useState<any>(null);
  const [videoSources, setVideoSources] = useState<VideoSource[]>([]);
  const [currentSourceIndex, setCurrentSourceIndex] = useState(0);

  // Preload configurations based on priority
  const getPreloadConfig = useCallback((priority: string): PreloadConfig => {
    switch (priority) {
      case 'high':
        return {
          threshold: 0.1,
          rootMargin: '200px',
          priority: 'high',
        };
      case 'medium':
        return {
          threshold: 0.3,
          rootMargin: '100px',
          priority: 'medium',
        };
      case 'low':
      default:
        return {
          threshold: 0.5,
          rootMargin: '50px',
          priority: 'low',
        };
    }
  }, []);

  const config = getPreloadConfig(priority);

  // Preload observer - starts loading video metadata before it comes into view
  useEffect(() => {
    const video = videoRef.current;
    if (!video || preloadStrategy === 'none') return;

    preloadObserverRef.current = new IntersectionObserver(
      (entries) => {
        const entry = entries[0];
        if (entry.isIntersecting && !isPreloaded) {
          setIsPreloaded(true);

          // Start preloading based on strategy
          if (preloadStrategy === 'metadata') {
            video.preload = 'metadata';
          } else if (preloadStrategy === 'auto') {
            video.preload = 'auto';
          }

          // Track preload start
          setLoadStartTime(Date.now());

          video.load();
        }
      },
      {
        threshold: config.threshold,
        rootMargin: config.rootMargin,
      }
    );

    if (containerRef.current) {
      preloadObserverRef.current.observe(containerRef.current);
    }

    return () => {
      if (preloadObserverRef.current) {
        preloadObserverRef.current.disconnect();
      }
    };
  }, [preloadStrategy, isPreloaded, config, priority]);

  // Main intersection observer for auto-play
  useEffect(() => {
    const video = videoRef.current;
    if (!video || !autoPlay) return;

    observerRef.current = new IntersectionObserver(
      (entries) => {
        const entry = entries[0];
        setIsInView(entry.isIntersecting);

        if (entry.isIntersecting && entry.intersectionRatio >= 0.5) {
          // Video is in view and at least 50% visible
          video.play()
            .then(() => {
              onPlay?.();

              // Track load performance if we have timing data
              if (loadStartTime) {
                const loadTime = Date.now() - loadStartTime;
                videoAnalyticsService.trackInteraction({
                  activityId: 'unknown',
                  videoUrl: src,
                  viewDuration: loadTime,
                  interactionCount: 1,
                });
              }
            })
            .catch((error) => {
              console.warn('Auto-play failed:', error);
              setHasError(true);
              onError?.(new Error('Auto-play failed'));
            });
        } else {
          // Video is out of view
          video.pause();
          onPause?.();
        }
      },
      {
        threshold: 0.5,
        rootMargin: '50px',
      }
    );

    if (containerRef.current) {
      observerRef.current.observe(containerRef.current);
    }

    return () => {
      if (observerRef.current) {
        observerRef.current.disconnect();
      }
    };
  }, [autoPlay, onPlay, onPause, onError, loadStartTime, src]);

  // Video event handlers
  const handleLoadedData = useCallback(() => {
    setIsLoading(false);
    onLoad?.();

    // Track successful load
    if (loadStartTime) {
      const loadTime = Date.now() - loadStartTime;
      videoAnalyticsService.trackInteraction({
        activityId: 'unknown',
        videoUrl: src,
        viewDuration: loadTime,
        interactionCount: 1,
      });
    }
  }, [onLoad, loadStartTime, src]);

  const handleError = useCallback((e: React.SyntheticEvent<HTMLVideoElement, Event>) => {
    console.error('Video loading error:', e);
    setHasError(true);
    setIsLoading(false);

    // Track load error
    if (loadStartTime) {
      const loadTime = Date.now() - loadStartTime;
      videoAnalyticsService.trackInteraction({
        activityId: 'unknown',
        videoUrl: src,
        viewDuration: loadTime,
        interactionCount: 0, // Error occurred
      });
    }

    onError?.(new Error('Video failed to load'));
  }, [onError, loadStartTime, src]);

  const handleCanPlay = useCallback(() => {
    // Video is ready to play
    setIsLoading(false);
  }, []);

  const handleLoadStart = useCallback(() => {
    // Video started loading
    if (!loadStartTime) {
      setLoadStartTime(Date.now());
    }
  }, [loadStartTime]);

  // Retry loading on error
  const retryLoad = useCallback(() => {
    const video = videoRef.current;
    if (video) {
      setHasError(false);
      setIsLoading(true);
      setLoadStartTime(Date.now());
      video.load();
    }
  }, []);

  if (hasError) {
    return (
      <Box
        ref={containerRef}
        className={className}
        sx={{
          width,
          height,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          bgcolor: 'grey.100',
          borderRadius: 2,
          gap: 2,
          p: 2,
        }}
      >
        <ErrorIcon sx={{ fontSize: 48, color: 'error.main' }} />
        <Typography variant="h6" color="error">
          Video Error
        </Typography>
        <Typography variant="body2" color="text.secondary" align="center">
          {videoError?.message || 'Video failed to load'}
        </Typography>
        {videoError?.details && (
          <Typography variant="caption" color="text.secondary" align="center">
            {videoError.details}
          </Typography>
        )}
        {videoError?.retryable && retryCount < 3 && (
          <Button
            variant="contained"
            startIcon={<RefreshIcon />}
            onClick={retryLoad}
            size="small"
            sx={{ minHeight: 44 }}
          >
            Retry ({retryCount}/3)
          </Button>
        )}
        {videoSources.length > 1 && currentSourceIndex < videoSources.length - 1 && (
          <Typography variant="caption" color="text.secondary">
            Using source {currentSourceIndex + 1} of {videoSources.length}
          </Typography>
        )}
        <Snackbar
          open={showErrorSnackbar}
          autoHideDuration={6000}
          onClose={() => setShowErrorSnackbar(false)}
          anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
        >
          <Alert
            onClose={() => setShowErrorSnackbar(false)}
            severity="error"
            sx={{ width: '100%' }}
          >
            {videoError?.message || 'Video error occurred'}
          </Alert>
        </Snackbar>
      </Box>
    );
  }

  return (
    <Box
      ref={containerRef}
      className={className}
      sx={{
        position: 'relative',
        width,
        height,
        overflow: 'hidden',
        borderRadius: 2,
      }}
    >
      {/* Loading indicator */}
      {isLoading && (
        <Box
          sx={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            zIndex: 2,
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            gap: 1,
          }}
        >
          <CircularProgress />
          <Typography variant="caption" color="white">
            {isPreloaded ? 'Loading...' : 'Preparing...'}
          </Typography>
        </Box>
      )}

      {/* Video element */}
      <video
        ref={videoRef}
        src={src}
        poster={poster}
        muted={muted}
        loop={loop}
        playsInline
        preload={preloadStrategy === 'none' ? 'none' : 'metadata'}
        style={{
          width: '100%',
          height: '100%',
          objectFit: 'cover',
          opacity: isLoading ? 0.3 : 1,
          transition: 'opacity 0.3s ease',
        }}
        onLoadedData={handleLoadedData}
        onError={handleError}
        onCanPlay={handleCanPlay}
        onLoadStart={handleLoadStart}
      />

      {/* Debug info (remove in production) */}
      {process.env.NODE_ENV === 'development' && (
        <Box
          sx={{
            position: 'absolute',
            top: 0,
            right: 0,
            bgcolor: 'rgba(0, 0, 0, 0.8)',
            color: 'white',
            p: 1,
            fontSize: '0.75rem',
            zIndex: 3,
          }}
        >
          <div>Priority: {priority}</div>
          <div>In View: {isInView ? 'Yes' : 'No'}</div>
          <div>Preloaded: {isPreloaded ? 'Yes' : 'No'}</div>
          <div>Loading: {isLoading ? 'Yes' : 'No'}</div>
        </Box>
      )}
    </Box>
  );
};

export default LazyVideoLoader;