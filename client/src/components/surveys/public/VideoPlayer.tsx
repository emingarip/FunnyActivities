import React, { useRef, useEffect } from 'react';
import './VideoPlayer.css';

interface VideoPlayerProps {
  src: string;
  autoPlay?: boolean;
  muted?: boolean;
  loop?: boolean;
  className?: string;
  onEnded?: () => void;
  onError?: (error: Event) => void;
}

const VideoPlayer: React.FC<VideoPlayerProps> = ({
  src,
  autoPlay = true,
  muted = true,
  loop = false,
  className = '',
  onEnded,
  onError,
}) => {
  const videoRef = useRef<HTMLVideoElement>(null);

  useEffect(() => {
    const video = videoRef.current;
    if (!video) return;

    const handleEnded = () => {
      onEnded?.();
    };

    const handleError = (error: Event) => {
      onError?.(error);
    };

    video.addEventListener('ended', handleEnded);
    video.addEventListener('error', handleError);

    return () => {
      video.removeEventListener('ended', handleEnded);
      video.removeEventListener('error', handleError);
    };
  }, [onEnded, onError]);

  // Auto-play when src changes
  useEffect(() => {
    const video = videoRef.current;
    if (video && autoPlay) {
      video.play().catch((error) => {
        console.warn('Auto-play failed:', error);
      });
    }
  }, [src, autoPlay]);

  return (
    <div className={`video-player ${className}`}>
      <video
        ref={videoRef}
        src={src}
        autoPlay={autoPlay}
        muted={muted}
        loop={loop}
        playsInline
        className="video-player__video"
        controls={!autoPlay} // Show controls if not auto-playing
      >
        Your browser does not support the video tag.
      </video>
    </div>
  );
};

export default VideoPlayer;
