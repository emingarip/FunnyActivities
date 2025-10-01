export interface VideoSource {
  src: string;
  type: string;
  quality?: 'low' | 'medium' | 'high';
}

export interface VideoValidationResult {
  isValid: boolean;
  error?: string;
  supportedFormats: string[];
  recommendedFormat?: string;
}

export interface VideoLoadOptions {
  timeout?: number;
  retryAttempts?: number;
  retryDelay?: number;
}

export interface VideoInstance {
  id: string;
  element: HTMLVideoElement;
  container: HTMLElement;
  isPlaying: boolean;
  isIntersecting: boolean;
  isLoading: boolean;
  autoPlay: boolean;
  intersectionRatio: number;
  index: number;
  onPlay?: () => void;
  onPause?: () => void;
  onStateChange?: (state: Partial<VideoInstance>) => void;
}

export class VideoManager {
  private static instance: VideoManager;
  private videoInstances: Map<string, VideoInstance> = new Map();
  private currentlyPlayingVideoId: string | null = null;
  private isInitialized = false;

  static getInstance(): VideoManager {
    if (!VideoManager.instance) {
      VideoManager.instance = new VideoManager();
    }
    return VideoManager.instance;
  }

  /**
   * Register a video instance with the manager
   */
  registerVideo(instance: VideoInstance): string {
    this.videoInstances.set(instance.id, instance);
    console.log('🎬 VideoManager: Registered video instance:', {
      id: instance.id,
      totalInstances: this.videoInstances.size,
      currentlyPlaying: this.currentlyPlayingVideoId
    });
    return instance.id;
  }

  /**
   * Unregister a video instance
   */
  unregisterVideo(id: string): void {
    const instance = this.videoInstances.get(id);
    if (instance) {
      // If this was the currently playing video, clear it
      if (this.currentlyPlayingVideoId === id) {
        this.currentlyPlayingVideoId = null;
      }
      this.videoInstances.delete(id);
      console.log('🎬 VideoManager: Unregistered video instance:', {
        id,
        remainingInstances: this.videoInstances.size
      });
    }
  }

  /**
   * Update video instance state
   */
  updateVideoState(id: string, updates: Partial<VideoInstance>): void {
    const instance = this.videoInstances.get(id);
    if (instance) {
      Object.assign(instance, updates);
      this.videoInstances.set(id, instance);
    }
  }

  /**
   * Update intersection ratio for a video instance
   */
  updateIntersectionRatio(id: string, ratio: number): void {
    const instance = this.videoInstances.get(id);
    if (instance) {
      instance.intersectionRatio = ratio;
      instance.isIntersecting = ratio > 0;
      this.videoInstances.set(id, instance);
    }
  }

  /**
   * Get the video instance with the highest intersection ratio
   */
  getHighestRatioVideo(): VideoInstance | null {
    let highestRatio = 0;
    let highestRatioVideo: VideoInstance | null = null;

    this.videoInstances.forEach((instance) => {
      if (instance.intersectionRatio > highestRatio) {
        highestRatio = instance.intersectionRatio;
        highestRatioVideo = instance;
      }
    });

    return highestRatioVideo;
  }

  /**
   * Get all video instances sorted by intersection ratio (highest first)
   */
  getVideosByIntersectionRatio(): VideoInstance[] {
    return Array.from(this.videoInstances.values())
      .sort((a, b) => b.intersectionRatio - a.intersectionRatio);
  }

  /**
   * Request to play a video - coordinates with other videos
   */
  async requestPlay(id: string): Promise<{ success: boolean; reason?: string }> {
    const instance = this.videoInstances.get(id);
    if (!instance) {
      return { success: false, reason: 'Video instance not found' };
    }

    // If this video is already playing, just return success
    if (instance.isPlaying) {
      return { success: true };
    }

    // If no video is currently playing, allow this one to play
    if (!this.currentlyPlayingVideoId) {
      return { success: true };
    }

    // If another video is playing, pause it first
    const currentPlayingInstance = this.videoInstances.get(this.currentlyPlayingVideoId);
    if (currentPlayingInstance) {
      console.log('🎬 VideoManager: Pausing currently playing video before starting new one:', {
        currentId: this.currentlyPlayingVideoId,
        newId: id
      });

      try {
        currentPlayingInstance.element.pause();
        currentPlayingInstance.isPlaying = false;
        currentPlayingInstance.onPause?.();
        this.currentlyPlayingVideoId = id;
        return { success: true };
      } catch (error) {
        console.warn('🎬 VideoManager: Failed to pause currently playing video:', error);
        return { success: false, reason: 'Failed to pause current video' };
      }
    }

    return { success: true };
  }

  /**
   * Pause a specific video
   */
  pauseVideo(id: string): void {
    const instance = this.videoInstances.get(id);
    if (instance && instance.isPlaying) {
      console.log('🎬 VideoManager: Attempting to pause video:', {
        id,
        element: instance.element,
        currentTime: instance.element.currentTime,
        paused: instance.element.paused,
        readyState: instance.element.readyState
      });

      try {
        // Force pause the video element
        instance.element.pause();

        // Double-check that pause actually worked
        if (!instance.element.paused) {
          console.warn('🎬 VideoManager: Pause command issued but video is still not paused:', id);
          // Try again with more aggressive approach
          setTimeout(() => {
            if (!instance.element.paused) {
              instance.element.pause();
              console.log('🎬 VideoManager: Second pause attempt for:', id);
            }
          }, 100);
        }

        instance.isPlaying = false;
        instance.onPause?.();
        instance.onStateChange?.({ isPlaying: false });
        console.log('🎬 VideoManager: Successfully paused video:', id);
      } catch (error) {
        console.error('🎬 VideoManager: Failed to pause video:', {
          id,
          error: error instanceof Error ? error.message : 'Unknown error',
          element: instance.element
        });

        // Even if pause fails, update our internal state
        instance.isPlaying = false;
        instance.onStateChange?.({ isPlaying: false });
      }
    } else {
      console.log('🎬 VideoManager: Video not playing or instance not found:', {
        id,
        instanceExists: !!instance,
        isPlaying: instance?.isPlaying
      });
    }
  }

  /**
   * Pause all videos except the specified one
   */
  pauseAllExcept(id: string): void {
    console.log('🎬 VideoManager: Pausing all videos except:', { id, totalInstances: this.videoInstances.size });
    this.videoInstances.forEach((instance, instanceId) => {
      if (instanceId !== id && instance.isPlaying) {
        console.log('🎬 VideoManager: Pausing video in pauseAllExcept:', instanceId);
        this.pauseVideo(instanceId);
      }
    });
  }

  /**
   * Force pause all videos (emergency method)
   */
  forcePauseAll(): void {
    console.log('🚨 VideoManager: FORCE PAUSING ALL VIDEOS');
    this.videoInstances.forEach((instance, instanceId) => {
      if (instance.isPlaying || !instance.element.paused) {
        console.log('🚨 VideoManager: Force pausing video:', {
          id: instanceId,
          wasPlaying: instance.isPlaying,
          elementPaused: instance.element.paused,
          currentTime: instance.element.currentTime
        });

        try {
          instance.element.pause();
          instance.isPlaying = false;
          instance.onPause?.();
          instance.onStateChange?.({ isPlaying: false });
        } catch (error) {
          console.error('🚨 VideoManager: Failed to force pause video:', {
            id: instanceId,
            error: error instanceof Error ? error.message : String(error)
          });
        }
      }
    });

    // Clear currently playing video
    if (this.currentlyPlayingVideoId) {
      console.log('🚨 VideoManager: Clearing currently playing video ID:', this.currentlyPlayingVideoId);
      this.currentlyPlayingVideoId = null;
    }
  }

  /**
   * Get the currently playing video ID
   */
  getCurrentlyPlayingVideoId(): string | null {
    return this.currentlyPlayingVideoId;
  }

  /**
   * Check if a video can be played (considering loading state and other constraints)
   */
  canPlayVideo(id: string): boolean {
    const instance = this.videoInstances.get(id);
    if (!instance) return false;

    // Don't play if video is loading
    if (instance.isLoading) {
      console.log('🎬 VideoManager: Cannot play video - still loading:', id);
      return false;
    }

    // Don't play if video is not ready
    if (instance.element.readyState < 2) { // HAVE_CURRENT_DATA
      console.log('🎬 VideoManager: Cannot play video - not ready:', {
        id,
        readyState: instance.element.readyState
      });
      return false;
    }

    return true;
  }

  /**
   * Get all registered video instances
   */
  getAllInstances(): Map<string, VideoInstance> {
    return this.videoInstances;
  }

  /**
   * Initialize the manager (call once on app startup)
   */
  initialize(): void {
    if (this.isInitialized) return;

    console.log('🎬 VideoManager: Initializing global video manager');
    this.isInitialized = true;

    // Add global error handling
    window.addEventListener('beforeunload', () => {
      this.cleanup();
    });
  }

  /**
   * Cleanup all video instances
   */
  cleanup(): void {
    console.log('🎬 VideoManager: Cleaning up all video instances');
    this.videoInstances.forEach((instance) => {
      try {
        instance.element.pause();
        instance.element.removeAttribute('src');
        instance.element.load();
      } catch (error) {
        console.warn('🎬 VideoManager: Error cleaning up video:', error);
      }
    });
    this.videoInstances.clear();
    this.currentlyPlayingVideoId = null;
  }
}

export class VideoUtils {
  private static videoManager = VideoManager.getInstance();

  // Supported video formats by modern browsers
  private static readonly SUPPORTED_FORMATS = [
    'video/mp4',
    'video/webm',
    'video/ogg',
    'video/avi',
    'video/mov',
    'video/wmv',
    'video/flv',
    'video/mkv'
  ];

  // Browser-specific format support detection
  private static readonly BROWSER_FORMAT_SUPPORT: Record<string, string[]> = {
    chrome: ['video/mp4', 'video/webm', 'video/ogg'],
    firefox: ['video/webm', 'video/ogg'],
    safari: ['video/mp4', 'video/mov'],
    edge: ['video/mp4', 'video/webm'],
    default: ['video/mp4', 'video/webm', 'video/ogg']
  };

  /**
   * Detects if a video source is a MinIO object key
   */
  static isMinioObjectKey(src: string): boolean {
    // MinIO object keys for videos start with "videos/" pattern
    // They are not valid HTTP/HTTPS URLs
    return !(!src || src.trim().length === 0) &&
           src.startsWith("videos/") &&
           !this.isValidHttpUrl(src);
  }

  /**
   * Extracts the object key from a signed URL
   * Signed URLs have the format: https://domain.com/bucket/object-key?signature=...
   * We need to extract just the object-key part
   */
  static extractObjectKeyFromSignedUrl(url: string): string | null {
    try {
      // Check for 1024 character limit first
      if (url && url.length >= 1024) {
        console.warn('URL exceeds 1024 character limit, rejecting');
        return null;
      }

      // If it's not a valid HTTP URL, treat it as a direct object key and return as-is
      if (!this.isValidHttpUrl(url)) {
        // Return non-empty object keys as-is, regardless of prefix
        // But reject obviously invalid inputs like "not-a-url" or URLs with empty paths
        if (url && url.length > 0 && !url.includes(' ') && !this.isInvalidObjectKey(url)) {
          return url;
        }
        return null;
      }

      const urlObj = new URL(url);

      // Extract the pathname which should contain the object key
      // For MinIO/S3 signed URLs, the format is typically:
      // https://minio.example.com/bucket-name/object-key?X-Amz-Algorithm=...
      const pathname = urlObj.pathname;

      // Remove leading slash if present
      const objectKey = pathname.startsWith('/') ? pathname.substring(1) : pathname;

      // Validate that we have a reasonable object key
      // Allow any non-empty object key, not just video keys
      if (objectKey && objectKey.length > 0 && !this.isInvalidObjectKey(objectKey)) {
        return objectKey;
      }

      return null;
    } catch (error) {
      console.warn('Failed to extract object key from URL:', error);
      return null;
    }
  }

  /**
   * Checks if a URL is a valid HTTP/HTTPS URL
   */
  static isValidHttpUrl(url: string): boolean {
    try {
      const urlObj = new URL(url);
      return urlObj.protocol === 'http:' || urlObj.protocol === 'https:';
    } catch {
      return false;
    }
  }

  /**
   * Validates a video source URL and checks format compatibility
   */
  static async validateVideoSource(
    src: string,
    options: VideoLoadOptions = {}
  ): Promise<VideoValidationResult> {
    const { timeout = 5000 } = options;

    try {
      // Check if it's a MinIO object key - these are valid but not HTTP URLs
      if (this.isMinioObjectKey(src)) {
        return {
          isValid: true,
          supportedFormats: this.getSupportedFormats()
        };
      }

      // Basic URL validation for non-MinIO sources
      if (!this.isValidUrl(src)) {
        return {
          isValid: false,
          error: 'Please provide a valid video URL or file path. Supported formats include HTTP/HTTPS URLs, data URIs, blob URLs, local file paths, and MinIO object keys.',
          supportedFormats: this.getSupportedFormats()
        };
      }

      // Check if URL is accessible
      const isAccessible = await this.checkUrlAccessibility(src, timeout);
      if (!isAccessible) {
        return {
          isValid: false,
          error: 'Unable to access the video source. Please check if the URL is correct and the video file exists.',
          supportedFormats: this.getSupportedFormats()
        };
      }

      // Detect video format from URL
      const detectedFormat = this.detectVideoFormat(src);
      if (!detectedFormat) {
        return {
          isValid: false,
          error: 'Could not determine the video format. Please ensure the URL includes a proper file extension (e.g., .mp4, .webm, .mov).',
          supportedFormats: this.getSupportedFormats()
        };
      }

      // Check browser compatibility
      const isCompatible = this.isFormatSupported(detectedFormat);
      if (!isCompatible) {
        const recommendedFormat = this.getRecommendedFormat();
        return {
          isValid: false,
          error: `The video format ${detectedFormat} is not supported by your browser. Try using ${recommendedFormat} format instead.`,
          supportedFormats: this.getSupportedFormats(),
          recommendedFormat
        };
      }

      return {
        isValid: true,
        supportedFormats: this.getSupportedFormats()
      };
    } catch (error) {
      return {
        isValid: false,
        error: `Validation failed: ${error instanceof Error ? error.message : 'Unknown error'}`,
        supportedFormats: this.getSupportedFormats()
      };
    }
  }

  /**
   * Checks if a URL is valid
   */
  static isValidUrl(url: string): boolean {
    // Handle undefined/null input
    if (!url) {
      console.warn('VideoUtils.isValidUrl: Received undefined/null URL');
      return false;
    }

    try {
      const urlObj = new URL(url);
      // Allow more URL schemes for video content
      const allowedSchemes = ['http:', 'https:', 'data:', 'blob:', 'file:', 'rtmp:', 'rtsp:', 'mms:'];
      return allowedSchemes.includes(urlObj.protocol);
    } catch {
      // For non-URI strings (like MinIO object keys), allow them if they're not empty and don't contain invalid characters
      if (url && url.trim().length > 0) {
        // Check for invalid characters that would make it unusable as a video source
        // Use a more explicit approach to avoid ESLint control character warnings
        const controlChars = ['\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07',
                              '\x08', '\x09', '\x0A', '\x0B', '\x0C', '\x0D', '\x0E', '\x0F',
                              '\x10', '\x11', '\x12', '\x13', '\x14', '\x15', '\x16', '\x17',
                              '\x18', '\x19', '\x1A', '\x1B', '\x1C', '\x1D', '\x1E', '\x1F', '\x7F'];

        // Reject dangerous schemes even in non-URI strings
        // eslint-disable-next-line no-script-url
        const dangerousSchemes = ['javascript:', 'vbscript:', 'data:text/html'];
        const hasDangerousScheme = dangerousSchemes.some(scheme =>
          url.toLowerCase().startsWith(scheme)
        );

        if (hasDangerousScheme) {
          return false;
        }

        // For non-URI strings, they should look like valid file paths or object keys
        // They should contain at least one of: slash, dot, or alphanumeric characters in a reasonable pattern
        // Must contain at least one slash or dot to be considered a valid path-like string
        const looksLikeValidPath = /^[a-zA-Z0-9_/\-.\s]+$/.test(url) &&
                                  (url.includes('/') || url.includes('.')) &&
                                  url.length > 3; // Must be longer than just a simple word

        return !controlChars.some(char => url.includes(char)) && looksLikeValidPath;
      }
      return false;
    }
  }

  /**
   * Checks if a URL is accessible using backend API for metadata retrieval
   */
  static async checkUrlAccessibility(url: string, timeout: number): Promise<boolean> {
    try {
      // Import the API function dynamically to avoid circular dependencies
      const { activitiesAPI } = await import('./api');

      // Extract object key from signed URLs to avoid 1024 character limit
      const objectKey = this.extractObjectKeyFromSignedUrl(url);

      // For MinIO object keys, use the video metadata endpoint
      if (this.isMinioObjectKey(url)) {
        try {
          await activitiesAPI.getVideoMetadata(objectKey || url);
          return true;
        } catch (apiError) {
          console.warn('Video metadata API check failed:', apiError);
          // Fallback to basic validation for MinIO object keys
          return this.isMinioObjectKey(url);
        }
      }

      // For regular URLs, try to get object metadata from backend
      // The backend will handle the actual accessibility check
      try {
        await activitiesAPI.getObjectMetadata(objectKey || url);
        return true;
      } catch (apiError) {
        console.warn('Object metadata API check failed:', apiError);
        // Fallback to basic URL validation for regular URLs
        return this.isValidUrl(url);
      }
    } catch (error) {
      console.warn('API import or execution failed:', error);
      // Final fallback: basic URL validation
      return this.isValidUrl(url);
    }
  }

  /**
   * Detects video format from URL
   */
  static detectVideoFormat(url: string): string | null {
    try {
      // Handle MinIO object keys (not URLs) - return as-is for processing
      if (this.isMinioObjectKey(url)) {
        const extension = url.toLowerCase().split('.').pop();
        const formatMap: Record<string, string> = {
          'mp4': 'video/mp4',
          'webm': 'video/webm',
          'ogg': 'video/ogg',
          'ogv': 'video/ogg',
          'avi': 'video/avi',
          'mov': 'video/quicktime',
          'wmv': 'video/x-ms-wmv',
          'flv': 'video/x-flv',
          'mkv': 'video/x-matroska'
        };

        return formatMap[extension || ''] || null;
      }

      // For valid URLs, parse to extract pathname and remove query parameters
      const urlObj = new URL(url);
      const pathname = urlObj.pathname;

      // Extract extension from the pathname (before query parameters)
      const extension = pathname.toLowerCase().split('.').pop();

      const formatMap: Record<string, string> = {
        'mp4': 'video/mp4',
        'webm': 'video/webm',
        'ogg': 'video/ogg',
        'ogv': 'video/ogg',
        'avi': 'video/avi',
        'mov': 'video/quicktime',
        'wmv': 'video/x-ms-wmv',
        'flv': 'video/x-flv',
        'mkv': 'video/x-matroska'
      };

      return formatMap[extension || ''] || null;
    } catch (error) {
      // Fallback to original method if URL parsing fails
      console.warn('URL parsing failed in detectVideoFormat, using fallback method:', error);
      const extension = url.toLowerCase().split('.').pop();
      const formatMap: Record<string, string> = {
        'mp4': 'video/mp4',
        'webm': 'video/webm',
        'ogg': 'video/ogg',
        'ogv': 'video/ogg',
        'avi': 'video/avi',
        'mov': 'video/quicktime',
        'wmv': 'video/x-ms-wmv',
        'flv': 'video/x-flv',
        'mkv': 'video/x-matroska'
      };

      return formatMap[extension || ''] || null;
    }
  }

  /**
   * Checks if a video format is supported by the current browser
   */
  static isFormatSupported(format: string): boolean {
    const video = document.createElement('video');
    return video.canPlayType(format) !== '';
  }

  /**
   * Gets supported video formats for the current browser
   */
  static getSupportedFormats(): string[] {
    const browser = this.detectBrowser();
    const supportedFormats = this.BROWSER_FORMAT_SUPPORT[browser] || this.BROWSER_FORMAT_SUPPORT.default;

    return supportedFormats.filter(format => this.isFormatSupported(format));
  }

  /**
   * Gets the recommended video format for the current browser
   */
  static getRecommendedFormat(): string {
    const supportedFormats = this.getSupportedFormats();
    const browser = this.detectBrowser();

    // Browser-specific format preferences
    const formatPreferences: Record<string, string[]> = {
      chrome: ['video/mp4', 'video/webm', 'video/ogg'],
      firefox: ['video/webm', 'video/ogg', 'video/mp4'],
      safari: ['video/mp4', 'video/mov'],
      edge: ['video/mp4', 'video/webm'],
      default: ['video/mp4', 'video/webm', 'video/ogg']
    };

    const preferences = formatPreferences[browser] || formatPreferences.default;
    return preferences.find(format => supportedFormats.includes(format)) ||
           supportedFormats[0] ||
           'video/mp4'; // Fallback to most widely supported format
  }

  /**
   * Detects the current browser
   */
  static detectBrowser(): string {
    const userAgent = navigator.userAgent.toLowerCase();

    if (userAgent.includes('chrome')) return 'chrome';
    if (userAgent.includes('firefox')) return 'firefox';
    if (userAgent.includes('safari') && !userAgent.includes('chrome')) return 'safari';
    if (userAgent.includes('edge')) return 'edge';

    return 'default';
  }

  /**
   * Creates multiple video sources for better browser compatibility
   */
  static createMultipleSources(baseSrc: string): VideoSource[] {
    const sources: VideoSource[] = [];
    const baseUrl = baseSrc.substring(0, baseSrc.lastIndexOf('.'));
    const extension = baseSrc.substring(baseSrc.lastIndexOf('.') + 1).toLowerCase();

    // Add original source
    const originalFormat = this.detectVideoFormat(baseSrc);
    if (originalFormat) {
      sources.push({
        src: baseSrc,
        type: originalFormat,
        quality: 'high'
      });
    }

    // Add alternative formats if possible
    const alternativeFormats = this.getSupportedFormats().filter(format =>
      format !== originalFormat
    );

    alternativeFormats.forEach(format => {
      const formatExtension = this.getFormatExtension(format);
      if (formatExtension && formatExtension !== extension) {
        sources.push({
          src: `${baseUrl}.${formatExtension}`,
          type: format,
          quality: 'medium'
        });
      }
    });

    return sources;
  }

  /**
   * Gets file extension for a video format
   */
  static getFormatExtension(format: string): string | null {
    const extensionMap: Record<string, string> = {
      'video/mp4': 'mp4',
      'video/webm': 'webm',
      'video/ogg': 'ogg',
      'video/avi': 'avi',
      'video/quicktime': 'mov',
      'video/x-ms-wmv': 'wmv',
      'video/x-flv': 'flv',
      'video/x-matroska': 'mkv'
    };

    return extensionMap[format] || null;
  }

  /**
   * Checks if auto-play is supported with user interaction
   */
  static async checkAutoplaySupport(): Promise<boolean> {
    const video = document.createElement('video');
    video.muted = true;
    video.autoplay = true;

    try {
      // Try to play without user interaction
      await video.play();
      return true;
    } catch {
      return false;
    }
  }

  /**
   * Enhanced auto-play support detection with more detailed information
   */
  static async getAutoplaySupportDetails(): Promise<{
    supported: boolean;
    requiresInteraction: boolean;
    mutedOnly: boolean;
    error?: string;
  }> {
    const video = document.createElement('video');
    video.muted = true;
    video.autoplay = true;

    try {
      // Try to play without user interaction
      await video.play();
      return {
        supported: true,
        requiresInteraction: false,
        mutedOnly: true, // We tested with muted=true
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';

      // Check if this is a typical autoplay restriction
      if (errorMessage.includes('NotAllowedError') ||
          errorMessage.includes('AbortError') ||
          errorMessage.includes('interrupted')) {
        return {
          supported: false,
          requiresInteraction: true,
          mutedOnly: true,
          error: 'Browser requires user interaction for autoplay',
        };
      }

      return {
        supported: false,
        requiresInteraction: true,
        mutedOnly: false,
        error: errorMessage,
      };
    }
  }

  /**
   * Attempts to play a video with proper error handling for autoplay restrictions
   */
  static async attemptPlay(videoElement: HTMLVideoElement): Promise<{
    success: boolean;
    error?: string;
    requiresInteraction: boolean;
  }> {
    try {
      await videoElement.play();
      return {
        success: true,
        requiresInteraction: false,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';

      // Check for autoplay restriction errors
      if (errorMessage.includes('NotAllowedError') ||
          errorMessage.includes('AbortError') ||
          errorMessage.includes('interrupted')) {
        return {
          success: false,
          error: 'Autoplay blocked by browser policy',
          requiresInteraction: true,
        };
      }

      return {
        success: false,
        error: errorMessage,
        requiresInteraction: false,
      };
    }
  }

  /**
   * Gets video metadata using backend API instead of HTML5 video element
   */
  static async getVideoMetadata(src: string): Promise<{
    duration?: number;
    width?: number;
    height?: number;
    size?: number;
    contentType?: string;
    lastModified?: Date;
    error?: string;
  }> {
    try {
      // Import the API function dynamically to avoid circular dependencies
      const { activitiesAPI } = await import('./api');

      // Extract object key from signed URLs to avoid 1024 character limit
      const objectKey = this.extractObjectKeyFromSignedUrl(src);

      let metadata;

      // For MinIO object keys, use the video metadata endpoint
      if (this.isMinioObjectKey(src)) {
        metadata = await activitiesAPI.getVideoMetadata(objectKey || src);
      } else {
        // For regular URLs, use the object metadata endpoint
        metadata = await activitiesAPI.getObjectMetadata(objectKey || src);
      }

      // Convert the backend ObjectMetadata to the expected format
      // The metadata is in the response.data property
      const metadataData = metadata.data;

      return {
        duration: metadataData.duration, // Note: backend may not provide duration for all objects
        width: metadataData.width, // Note: backend may not provide dimensions for all objects
        height: metadataData.height, // Note: backend may not provide dimensions for all objects
        size: metadataData.size,
        contentType: metadataData.contentType,
        lastModified: metadataData.lastModified ? new Date(metadataData.lastModified) : undefined
      };
    } catch (error) {
      console.warn('Failed to get video metadata from backend API:', error);
      return {
        error: error instanceof Error ? error.message : 'Failed to retrieve video metadata'
      };
    }
  }

  /**
   * Checks if an object key is invalid
   */
  static isInvalidObjectKey(key: string): boolean {
    // Check for obviously invalid inputs
    if (!key || key.trim().length === 0) {
      return true;
    }

    // Check for spaces (invalid in object keys)
    if (key.includes(' ')) {
      return true;
    }

    // Check for obviously invalid patterns that look like URLs but aren't valid object keys
    if (key.includes('://') && !this.isValidHttpUrl(key)) {
      return true;
    }

    // Check for incomplete or malformed paths
    if (key.startsWith('/') && key.length < 2) {
      return true;
    }

    // Check for paths that end with '/' (incomplete paths)
    if (key.endsWith('/')) {
      return true;
    }

    // Check for control characters or other invalid characters
    const controlChars = ['\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07',
                         '\x08', '\x09', '\x0A', '\x0B', '\x0C', '\x0D', '\x0E', '\x0F',
                         '\x10', '\x11', '\x12', '\x13', '\x14', '\x15', '\x16', '\x17',
                         '\x18', '\x19', '\x1A', '\x1B', '\x1C', '\x1D', '\x1E', '\x1F', '\x7F'];

    if (controlChars.some(char => key.includes(char))) {
      return true;
    }

    // Check for strings that are clearly not valid object keys
    // These are strings that don't look like file paths or object keys
    const invalidPatterns = [
      /^[^/]*$/, // Strings without any slashes (like "not-a-url")
      /^[^/]*\/$/, // Strings ending with slash but no filename (like "activity-videos/")
      /^[a-zA-Z0-9_-]+$/, // Simple alphanumeric strings without dots or slashes
    ];

    // If it matches any invalid pattern, it's likely not a valid object key
    if (invalidPatterns.some(pattern => pattern.test(key))) {
      return true;
    }

    // Additional validation: should look like a file path or object key
    // Must contain at least one slash and a dot (indicating a file extension)
    // or be a valid MinIO object key pattern
    const looksLikeValidObjectKey = (
      (key.includes('/') && key.includes('.')) || // Has both slash and dot (file path)
      key.startsWith('videos/') || // MinIO video object key
      key.startsWith('images/') || // MinIO image object key
      key.startsWith('documents/') || // MinIO document object key
      key.startsWith('audio/') // MinIO audio object key
    );

    return !looksLikeValidObjectKey;
  }
}
// Export empty object to make this a module
export {};
export default VideoUtils;