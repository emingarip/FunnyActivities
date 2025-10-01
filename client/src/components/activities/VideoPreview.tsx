import React, { useRef, useEffect, useState, useCallback } from 'react';
import VideoUtils, { VideoSource, VideoManager, VideoInstance } from '../../services/videoUtils';
import { activitiesAPI } from '../../services/api';

interface VideoPreviewProps {
  src: string;
  poster?: string;
  autoPlay?: boolean;
  muted?: boolean;
  loop?: boolean;
  width?: string | number;
  height?: string | number;
  aspectRatio?: string; // e.g., "16/9", "4/3", "1/1"
  onPlay?: () => void;
  onPause?: () => void;
  onError?: (error: Error) => void;
  className?: string;
  activityId?: string; // Required for MinIO object keys to request signed URLs
  index?: number; // Index for tracking order in smart playback system
}


const VideoPreview: React.FC<VideoPreviewProps> = ({
  src,
  poster,
  autoPlay = false,
  muted = true,
  loop = true,
  width = '100%',
  height = 'auto',
  aspectRatio,
  onPlay,
  onPause,
  onError,
  className,
  activityId,
  index = 0,
}) => {
  // Enhanced logging for component mounting and props
  console.log('🎬 VideoPreview: Component mounting with props:', {
    src,
    poster,
    autoPlay,
    muted,
    loop,
    width,
    height,
    aspectRatio,
    className,
    activityId,
    index,
    hasOnPlay: !!onPlay,
    hasOnPause: !!onPause,
    hasOnError: !!onError,
    timestamp: new Date().toISOString(),
  });

  // Specific logging for videoUrl processing
  console.log('🎬 VideoPreview: VideoUrl analysis:', {
    receivedSrc: src,
    isMinioObjectKey: VideoUtils.isMinioObjectKey(src),
    isValidHttpUrl: VideoUtils.isValidHttpUrl(src),
    srcLength: src.length,
    activityId,
    index,
    timestamp: new Date().toISOString(),
  });

  const videoRef = useRef<HTMLVideoElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const observerRef = useRef<IntersectionObserver | null>(null);

  const [isLoading, setIsLoading] = useState(true);
  const [hasError, setHasError] = useState(false);
  const [videoSources, setVideoSources] = useState<VideoSource[]>([]);
  const [currentSourceIndex, setCurrentSourceIndex] = useState(0);
  const [isDesktop, setIsDesktop] = useState(false);
  const [isInitialized, setIsInitialized] = useState(false);

  // Smart playback system state
  const [scrollDirection, setScrollDirection] = useState<'up' | 'down' | null>(null);
  const [lastScrollY, setLastScrollY] = useState(0);
  const [intersectionRatio, setIntersectionRatio] = useState(0);
  const [isVisible, setIsVisible] = useState(false);

  // VideoManager integration
  const videoManager = VideoManager.getInstance();
  const [videoInstanceId, setVideoInstanceId] = useState<string | null>(null);

  // Device detection
  useEffect(() => {
    const checkIsDesktop = () => {
      const width = window.innerWidth;
      const isDesktopDevice = width >= 768; // Match MUI's 'sm' breakpoint
      setIsDesktop(isDesktopDevice);
    };

    checkIsDesktop();
    window.addEventListener('resize', checkIsDesktop);

    return () => {
      window.removeEventListener('resize', checkIsDesktop);
    };
  }, []);

  // Global event system for single video playback
  useEffect(() => {
    const handleVideoPlay = (event: CustomEvent) => {
      const playingActivityId = event.detail.activityId;
      console.log('🎬 Global Event: Received video-play event', { playingActivityId, currentActivityId: activityId });

      if (playingActivityId !== activityId && videoInstanceId) {
        console.log('🎬 Global Event: Pausing video due to different activity playing', { videoInstanceId, playingActivityId, currentActivityId: activityId });
        videoManager.pauseVideo(videoInstanceId);
        if (videoRef.current && !videoRef.current.paused) {
          videoRef.current.pause();
        }
        onPause?.();
      }
    };

    window.addEventListener('video-play', handleVideoPlay as EventListener);

    return () => {
      window.removeEventListener('video-play', handleVideoPlay as EventListener);
    };
  }, [activityId, videoInstanceId, videoManager, onPause]);

  // Throttle function for scroll events
  const throttle = (func: Function, limit: number) => {
    let inThrottle: boolean;
    return function(this: any, ...args: any[]) {
      if (!inThrottle) {
        func.apply(this, args);
        inThrottle = true;
        setTimeout(() => inThrottle = false, limit);
      }
    };
  };

  // Scroll direction detection (mobile only)
  useEffect(() => {
    if (isDesktop) return;

    const handleScroll = throttle(() => {
      const currentScrollY = window.scrollY;
      const direction = currentScrollY > lastScrollY ? 'down' : 'up';
      setScrollDirection(direction);
      setLastScrollY(currentScrollY);

      console.log('🎯 Smart Playback: Scroll direction detected:', {
        direction,
        currentScrollY,
        lastScrollY,
        index
      });
    }, 100); // Throttle to 100ms

    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, [isDesktop, lastScrollY, index]);


  // Register video instance with VideoManager on mount
  useEffect(() => {
    if (videoRef.current && !videoInstanceId) {
      const videoInstance: VideoInstance = {
        id: `video-${index}-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
        element: videoRef.current,
        container: videoRef.current.parentElement || document.body,
        isPlaying: false,
        isIntersecting: false,
        isLoading: false,
        autoPlay: autoPlay,
        intersectionRatio: 0,
        index: index,
        onStateChange: () => {
          // No UI state updates needed
        }
      };
      const id = videoManager.registerVideo(videoInstance);
      setVideoInstanceId(id);

      console.log('🎬 Smart Playback: Video instance registered', {
        index,
        videoInstanceId: id,
        totalInstances: videoManager.getAllInstances().size
      });
    }

    // Cleanup on unmount
    return () => {
      if (videoInstanceId) {
        console.log('🎬 Smart Playback: Unregistering video instance', { index, videoInstanceId });
        videoManager.unregisterVideo(videoInstanceId);
      }
    };
  }, [videoInstanceId, videoManager, autoPlay, index]);

  // Function to request signed URL for MinIO object keys (public endpoint for public activities)
  const requestSignedUrl = useCallback(async (objectKey: string, activityId: string): Promise<string | null> => {
    try {
      console.log('Requesting signed URL for video:', objectKey);
      const response = await activitiesAPI.getPublicActivityVideoUrl(activityId, objectKey, 3600);

      if (!response?.data?.data?.signedVideoUrl) {
        console.warn('API response missing signedVideoUrl, will fallback to direct URL');
        return null;
      }

      return response.data.data.signedVideoUrl;
    } catch (error) {
      console.warn('Failed to get signed URL for video, will fallback:', error instanceof Error ? error.message : String(error));
      return null;
    }
  }, []);

  // Video source validation and setup
  useEffect(() => {
    const initializeVideo = async () => {
      console.log('🎬 VideoPreview: Initializing video source:', {
        src,
        activityId,
        index,
        timestamp: new Date().toISOString()
      });
      setIsLoading(true);
      setHasError(false);
      setCurrentSourceIndex(0);

      try {
        const isMinioKey = VideoUtils.isMinioObjectKey(src);
        let finalSrc = src;

        console.log('🎬 VideoPreview: Source analysis:', {
          src,
          isMinioKey,
          activityId,
          index
        });

        if (isMinioKey) {
          if (!activityId) {
            console.error('🎬 VideoPreview: MinIO object key detected but no activityId provided', { src, index });
            setHasError(true);
            onError?.(new Error('MinIO object key detected - activityId required'));
            return;
          }

          console.log('🎬 VideoPreview: Requesting signed URL for MinIO object key', { src, activityId, index });
          const signedUrl = await requestSignedUrl(src, activityId);
          if (signedUrl) {
            finalSrc = signedUrl;
            console.log('🎬 VideoPreview: Signed URL obtained successfully', { src, signedUrl, index });
          } else {
            finalSrc = `http://localhost:9000/activity-videos/${src}`;
            console.log('🎬 VideoPreview: Using fallback URL', { src, finalSrc, index });
          }
        }

        if (!finalSrc) {
          console.error('🎬 VideoPreview: Video source is undefined', { src, index });
          setHasError(true);
          onError?.(new Error('Video source is undefined'));
          return;
        }

        const detectedFormat = VideoUtils.detectVideoFormat(finalSrc) || 'video/mp4';
        const videoSource = { src: finalSrc, type: detectedFormat, quality: 'high' as const };
        setVideoSources([videoSource]);
        setIsInitialized(true);

        console.log('🎬 VideoPreview: Video initialization completed successfully', {
          src,
          finalSrc,
          detectedFormat,
          isMinioKey,
          index,
          timestamp: new Date().toISOString()
        });

      } catch (error) {
        console.error('🎬 VideoPreview: Video initialization failed:', {
          src,
          activityId,
          index,
          error: error instanceof Error ? error.message : String(error),
          timestamp: new Date().toISOString()
        });
        setHasError(true);
        onError?.(error instanceof Error ? error : new Error('Failed to initialize video'));
      } finally {
        setIsLoading(false);
      }
    };

    if (src) {
      initializeVideo();
    } else {
      console.log('⚠️ VideoPreview: No source provided, skipping initialization', { activityId, index });
    }
  }, [src, autoPlay, requestSignedUrl, activityId, onError, index]);


  // Smart playback handler
  const handleSmartPlayback = useCallback(async (entry: IntersectionObserverEntry, ratio: number) => {
    if (!videoInstanceId || isLoading || !autoPlay) return;

    // Update intersection ratio in VideoManager
    videoManager.updateIntersectionRatio(videoInstanceId, ratio);

    console.log('🎯 Smart Playback: Evaluating playback for video', {
      index,
      videoInstanceId,
      ratio,
      scrollDirection,
      currentlyPlaying: videoManager.getCurrentlyPlayingVideoId()
    });

    // Get the video with highest intersection ratio
    const highestRatioVideo = videoManager.getHighestRatioVideo();
    const isThisVideoHighest = highestRatioVideo?.id === videoInstanceId;

    console.log('🎯 Smart Playback: Visibility analysis', {
      highestRatioVideo: highestRatioVideo?.id,
      highestRatio: highestRatioVideo?.intersectionRatio,
      isThisVideoHighest,
      scrollDirection
    });

    // Determine if this video should play
    let shouldPlay = false;

    if (isThisVideoHighest && ratio >= 0.9) {
      // This video has the highest intersection ratio and meets threshold
      shouldPlay = true;
    } else if (scrollDirection && highestRatioVideo) {
      // Consider scroll direction for tie-breaking when ratios are close
      const currentPlayingId = videoManager.getCurrentlyPlayingVideoId();
      const currentPlayingInstance = currentPlayingId ? videoManager.getAllInstances().get(currentPlayingId) : null;

      if (currentPlayingInstance && highestRatioVideo.intersectionRatio - ratio < 0.1) {
        // Ratios are close, consider scroll direction
        if (scrollDirection === 'down' && index > currentPlayingInstance.index) {
          shouldPlay = ratio >= 0.7; // Lower threshold for scroll direction priority
        } else if (scrollDirection === 'up' && index < currentPlayingInstance.index) {
          shouldPlay = ratio >= 0.7; // Lower threshold for scroll direction priority
        }
      }
    }

    if (shouldPlay) {
      // Dispatch global event to pause other videos
      console.log('🎬 Global Event: Dispatching video-play event', { activityId, videoInstanceId });
      window.dispatchEvent(new CustomEvent('video-play', { detail: { activityId } }));

      const canPlay = videoManager.canPlayVideo(videoInstanceId);
      console.log('🎯 Smart Playback: Attempting to play video', {
        index,
        videoInstanceId,
        canPlay,
        reason: isThisVideoHighest ? 'highest_ratio' : 'scroll_direction'
      });

      if (canPlay) {
        try {
          const playResult = await videoManager.requestPlay(videoInstanceId);
          if (playResult.success) {
            await videoRef.current?.play();
            console.log('🎯 Smart Playback: Successfully started playback', { index, videoInstanceId });
            onPlay?.();
          } else {
            console.log('🎯 Smart Playback: VideoManager denied play request', {
              index,
              videoInstanceId,
              reason: playResult.reason
            });
          }
        } catch (error) {
          console.warn('🎯 Smart Playback: Failed to play video', {
            index,
            videoInstanceId,
            error: error instanceof Error ? error.message : String(error)
          });
        }
      }
    }
  }, [videoInstanceId, isLoading, autoPlay, index, scrollDirection, videoManager, onPlay, activityId]);

  // Smart video playback system for mobile devices
  useEffect(() => {
    const video = videoRef.current;
    if (!video || !videoInstanceId || isDesktop) return; // Skip on desktop

    console.log('🎬 Smart Playback: Setting up Intersection Observer for video', { index, videoInstanceId });

    observerRef.current = new IntersectionObserver(
      (entries) => {
        const entry = entries[0];
        const ratio = entry.intersectionRatio;
        const wasVisible = isVisible;

        setIntersectionRatio(ratio);
        setIsVisible(entry.isIntersecting);

        console.log('🎯 Smart Playback: Intersection update', {
          index,
          videoInstanceId,
          ratio,
          isIntersecting: entry.isIntersecting,
          scrollDirection,
          wasVisible
        });

        // Handle smart playback logic
        if (entry.isIntersecting && ratio >= 0.9) {
          console.log('🎯 Smart Playback: Video sufficiently visible, evaluating playback', { ratio });
          // Video is sufficiently visible - consider for playback
          handleSmartPlayback(entry, ratio);
        } else if (!entry.isIntersecting || ratio < 0.9) {
          console.log('🎯 Smart Playback: Video no longer sufficiently visible', { ratio, isIntersecting: entry.isIntersecting });
          // Video is no longer sufficiently visible - pause if playing
          if (videoInstanceId && videoManager.getCurrentlyPlayingVideoId() === videoInstanceId) {
            console.log('🎬 Smart Playback: Pausing video due to low visibility', { index, videoInstanceId, ratio });
            videoManager.pauseVideo(videoInstanceId);
            onPause?.();
          }
        }
      },
      {
        threshold: 0.9, // Trigger when 90% of video is visible
        rootMargin: '50px', // Start observing 50px before video comes into view
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
  }, [autoPlay, onPlay, onPause, isLoading, videoInstanceId, videoManager, isDesktop, index, scrollDirection, isVisible]);


  // Video event handlers
  const handleLoadedData = useCallback(() => {
    console.log('🎬 VideoPreview: Video data loaded successfully', {
      index,
      videoInstanceId,
      isDesktop,
      autoPlay,
      isLoading,
      videoSrc: videoRef.current?.src,
      videoReadyState: videoRef.current?.readyState,
      videoNetworkState: videoRef.current?.networkState,
      videoError: videoRef.current?.error,
      timestamp: new Date().toISOString()
    });
    setIsLoading(false);

    const video = videoRef.current;
    if (video) {
      if (autoPlay && !isDesktop) {
        VideoUtils.attemptPlay(video)
          .then((result) => {
            if (result.success) {
              console.log('🎬 VideoPreview: Auto-play successful after data load (mobile)', { index, videoInstanceId });
              onPlay?.();
            } else if (result.requiresInteraction) {
              console.log('🎬 VideoPreview: Video loaded but requires user interaction to play (mobile)', { index, videoInstanceId });
            }
          })
          .catch((error) => {
            console.warn('🎬 VideoPreview: Failed to auto-play loaded video (mobile):', {
              index,
              videoInstanceId,
              error: error instanceof Error ? error.message : String(error)
            });
          });
      }
    }
  }, [autoPlay, onPlay, isDesktop, index, videoInstanceId, isLoading]);

  const handleError = useCallback((e: React.SyntheticEvent<HTMLVideoElement, Event>) => {
    console.error('🎬 Smart Playback: Video loading error occurred', { index, videoInstanceId });
    setIsLoading(false);
    setHasError(true);

    const target = e.target as HTMLVideoElement;
    const error = target.error;
    const errorMessage = error ? `Error code: ${error.code}, Message: ${error.message}` : 'Video failed to load';

    // Enhanced error logging for MinIO-related failures
    const isMinioRelated = src && (VideoUtils.isMinioObjectKey(src) || target.src.includes('minio') || target.src.includes('localhost:9000'));
    const errorDetails = {
      index,
      videoInstanceId,
      errorCode: error?.code,
      errorMessage: error?.message,
      src: target.src,
      originalSrc: src,
      isMinioRelated,
      networkState: target.networkState,
      readyState: target.readyState,
      activityId,
      timestamp: new Date().toISOString(),
    };

    console.error('🎬 Smart Playback: Video error details', errorDetails);

    // Specific handling for MinIO failures
    if (isMinioRelated) {
      console.error('🚨 MinIO Video Load Failure:', {
        ...errorDetails,
        possibleCauses: [
          'MinIO server unreachable',
          'Invalid signed URL',
          'Object key not found',
          'CORS policy blocking request',
          'Network connectivity issues'
        ],
        troubleshooting: [
          'Check MinIO server status',
          'Verify object key exists',
          'Check signed URL expiration',
          'Review CORS configuration'
        ]
      });
    }

    onError?.(new Error(errorMessage));
  }, [onError, index, videoInstanceId, src, activityId]);

  // No retry mechanism needed for clean video display

  // No user interaction handlers needed for clean video display

  const handlePlay = useCallback(() => {
    console.log('🎬 Smart Playback: Video play event triggered', {
      index,
      videoInstanceId,
      intersectionRatio,
      scrollDirection,
      currentlyPlaying: videoManager.getCurrentlyPlayingVideoId()
    });
    onPlay?.();
  }, [onPlay, index, videoInstanceId, intersectionRatio, scrollDirection, videoManager]);

  const handlePause = useCallback(() => {
    console.log('🎬 Smart Playback: Video pause event triggered', {
      index,
      videoInstanceId,
      intersectionRatio,
      scrollDirection,
      currentlyPlaying: videoManager.getCurrentlyPlayingVideoId()
    });
    onPause?.();
  }, [onPause, index, videoInstanceId, intersectionRatio, scrollDirection, videoManager]);

  const handleEnded = useCallback(() => {
    console.log('🎬 Smart Playback: Video ended event triggered', {
      index,
      videoInstanceId,
      intersectionRatio,
      scrollDirection
    });
    onPause?.();
  }, [onPause, index, videoInstanceId, intersectionRatio, scrollDirection]);


  // Desktop hover event handlers
  const handleMouseEnter = useCallback(async () => {
    if (!isDesktop || !videoInstanceId || isLoading) return;

    const video = videoRef.current;
    if (!video) return;

    try {
      const playResult = await videoManager.requestPlay(videoInstanceId);
      if (playResult.success) {
        await video.play();
        onPlay?.();
      }
    } catch (error) {
      console.warn('Failed to play video on hover:', error instanceof Error ? error.message : String(error));
    }
  }, [isDesktop, videoInstanceId, isLoading, videoManager, onPlay]);

  const handleMouseLeave = useCallback(() => {
    if (!isDesktop || !videoInstanceId) return;

    const video = videoRef.current;
    if (!video) return;

    try {
      videoManager.pauseVideo(videoInstanceId);
      if (!video.paused) {
        video.pause();
      }
      onPause?.();
    } catch (error) {
      console.error('Error pausing video on mouse leave:', error instanceof Error ? error.message : String(error));
    }
  }, [isDesktop, videoInstanceId, videoManager, onPause]);



  // Cleanup video element on unmount
  useEffect(() => {
    return () => {
      const video = videoRef.current;
      if (video) {
        // Pause video and clear src to prevent memory leaks
        video.pause();
        video.removeAttribute('src');
        video.load(); // Reset video element
      }
    };
  }, [src]);

  if (hasError) {
    return (
      <div
        ref={containerRef}
        className={className}
        style={{
          width,
          height,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          backgroundColor: '#f5f5f5',
          borderRadius: '8px',
          color: '#666',
          fontSize: '16px',
        }}
      >
        Video failed to load
      </div>
    );
  }

  return (
    <div
      ref={containerRef}
      className={className}
      style={{
        position: 'relative',
        width,
        height,
        overflow: 'hidden',
        borderRadius: '8px',
        cursor: isDesktop ? 'pointer' : 'default', // Show pointer cursor on desktop for hover indication
        aspectRatio: aspectRatio || undefined,
      }}
    >
      {/* Debug logging for video element */}
      {(() => {
        console.log('🎬 VideoPreview: Rendering video element', {
          index,
          isInitialized,
          currentSourceIndex,
          videoSources: videoSources.map(s => ({ src: s.src.substring(0, 100) + '...', type: s.type })),
          actualSrc: isInitialized ? (videoSources[currentSourceIndex]?.src || '') : '',
          hasVideoRef: !!videoRef.current,
          timestamp: new Date().toISOString()
        });
        return null;
      })()}

      {/* Video element */}
      <video
        ref={videoRef}
        src={isInitialized ? (videoSources[currentSourceIndex]?.src || '') : ''}
        poster={poster}
        muted={muted}
        loop={loop}
        playsInline
        preload="metadata"
        style={{
          width: '100%',
          height: '100%',
          objectFit: 'cover',
        }}
        onLoadedData={handleLoadedData}
        onError={handleError}
        onPlay={handlePlay}
        onPause={handlePause}
        onEnded={handleEnded}
        onMouseEnter={handleMouseEnter}
        onMouseLeave={handleMouseLeave}
      />
    </div>
  );
};

export default VideoPreview;