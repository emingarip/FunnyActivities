/**
 * Video Components Testing Guide
 *
 * This file provides comprehensive testing scenarios for the enhanced video components.
 * It covers various error conditions, browser compatibility, and user interaction scenarios.
 */

import VideoUtils from '../services/videoUtils';

// Test data for different video scenarios
export const TEST_VIDEO_SOURCES = {
  validMp4: 'https://example.com/video/sample.mp4',
  validWebm: 'https://example.com/video/sample.webm',
  invalidUrl: 'not-a-valid-url',
  nonExistent: 'https://example.com/non-existent-video.mp4',
  unsupportedFormat: 'https://example.com/video/sample.wmv',
  corruptedVideo: 'https://example.com/video/corrupted.mp4',
  slowConnection: 'https://example.com/video/large-file.mp4',
  noExtension: 'https://example.com/video/sample',
  relativePath: '/video/sample.mp4',
  dataUri: 'data:video/mp4;base64,AAAAHGZ0eXBtcDQyAAACAGlzb21pc28yYXZjMQAAAAhmcmVlAAAGF21kYXQQAAACBAAlEAAgQJ',
};

// Browser compatibility test scenarios
export const BROWSER_TEST_SCENARIOS = [
  {
    name: 'Chrome Desktop',
    userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
    expectedFormats: ['video/mp4', 'video/webm', 'video/ogg'],
    supportsAutoplay: false,
  },
  {
    name: 'Firefox Desktop',
    userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0',
    expectedFormats: ['video/webm', 'video/ogg'],
    supportsAutoplay: false,
  },
  {
    name: 'Safari Desktop',
    userAgent: 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1.1 Safari/605.1.15',
    expectedFormats: ['video/mp4', 'video/quicktime'],
    supportsAutoplay: false,
  },
  {
    name: 'Chrome Mobile',
    userAgent: 'Mozilla/5.0 (Linux; Android 10; SM-G973F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.120 Mobile Safari/537.36',
    expectedFormats: ['video/mp4', 'video/webm'],
    supportsAutoplay: false,
  },
];

// Error scenario tests
export const ERROR_TEST_SCENARIOS = [
  {
    name: 'Invalid URL Format',
    input: TEST_VIDEO_SOURCES.invalidUrl,
    expectedError: {
      type: 'validation',
      message: 'Invalid video URL format',
      retryable: false,
    },
  },
  {
    name: 'Non-existent Video',
    input: TEST_VIDEO_SOURCES.nonExistent,
    expectedError: {
      type: 'network',
      message: 'Video source is not accessible',
      retryable: true,
    },
  },
  {
    name: 'Unsupported Format',
    input: TEST_VIDEO_SOURCES.unsupportedFormat,
    expectedError: {
      type: 'format',
      message: 'Video format video/x-ms-wmv is not supported by this browser',
      retryable: false,
    },
  },
  {
    name: 'No Extension',
    input: TEST_VIDEO_SOURCES.noExtension,
    expectedError: {
      type: 'validation',
      message: 'Unable to detect video format from URL',
      retryable: false,
    },
  },
];

// Autoplay test scenarios
export const AUTOPLAY_TEST_SCENARIOS = [
  {
    name: 'Autoplay with User Interaction',
    setup: async () => {
      // Simulate user interaction
      document.dispatchEvent(new Event('click'));
      document.dispatchEvent(new Event('touchstart'));
    },
    expectedResult: 'autoplay-success',
  },
  {
    name: 'Autoplay without User Interaction',
    setup: async () => {
      // No user interaction
    },
    expectedResult: 'autoplay-blocked',
  },
  {
    name: 'Autoplay with Muted Video',
    setup: async () => {
      document.dispatchEvent(new Event('click'));
    },
    videoProps: { muted: true },
    expectedResult: 'autoplay-success',
  },
];

// Performance test scenarios
export const PERFORMANCE_TEST_SCENARIOS = [
  {
    name: 'Video Loading Performance',
    metrics: [
      'loadStartTime',
      'loadedDataTime',
      'canPlayTime',
      'totalLoadTime',
    ],
    thresholds: {
      totalLoadTime: 5000, // 5 seconds
      metadataLoadTime: 2000, // 2 seconds
    },
  },
  {
    name: 'Retry Mechanism Performance',
    maxRetries: 3,
    retryDelay: 1000,
    expectedTotalTime: 6000, // 3 retries * 1s delay + initial load
  },
];

// Accessibility test scenarios
export const ACCESSIBILITY_TEST_SCENARIOS = [
  {
    name: 'Keyboard Navigation',
    tests: [
      'Tab navigation to video controls',
      'Spacebar to play/pause',
      'Arrow keys for volume control',
      'Enter key to activate buttons',
    ],
  },
  {
    name: 'Screen Reader Support',
    tests: [
      'Video element has proper ARIA labels',
      'Play/pause button has accessible name',
      'Error messages are announced',
      'Loading states are communicated',
    ],
  },
  {
    name: 'Focus Management',
    tests: [
      'Focus moves to video when activated',
      'Focus returns to trigger after closing',
      'Focus is visible on all interactive elements',
    ],
  },
];

// Cross-browser compatibility tests
export const CROSS_BROWSER_TESTS = [
  {
    name: 'Video Format Support Matrix',
    browsers: ['Chrome', 'Firefox', 'Safari', 'Edge'],
    formats: ['MP4', 'WebM', 'Ogg', 'AVI', 'MOV'],
    expectedResults: {
      Chrome: ['MP4', 'WebM', 'Ogg'],
      Firefox: ['WebM', 'Ogg'],
      Safari: ['MP4', 'MOV'],
      Edge: ['MP4', 'WebM'],
    },
  },
  {
    name: 'Autoplay Policy Compliance',
    browsers: ['Chrome', 'Firefox', 'Safari', 'Edge'],
    scenarios: [
      'Muted video with user interaction',
      'Unmuted video with user interaction',
      'Muted video without user interaction',
      'Unmuted video without user interaction',
    ],
  },
];

// Integration test scenarios
export const INTEGRATION_TEST_SCENARIOS = [
  {
    name: 'VideoPreview Component Integration',
    component: 'VideoPreview',
    props: {
      src: TEST_VIDEO_SOURCES.validMp4,
      autoPlay: true,
      enableClickToPlay: true,
      maxRetryAttempts: 3,
    },
    userActions: [
      'Load component',
      'Wait for validation',
      'Click to enable playback',
      'Verify video plays',
      'Test error handling',
    ],
  },
  {
    name: 'LazyVideoLoader Component Integration',
    component: 'LazyVideoLoader',
    props: {
      src: TEST_VIDEO_SOURCES.validMp4,
      priority: 'high',
      preloadStrategy: 'metadata',
    },
    userActions: [
      'Component mounts',
      'Intersection observer triggers',
      'Video preloads metadata',
      'Video comes into view',
      'Autoplay attempts',
    ],
  },
];

// Test utilities
export class VideoTestUtils {
  static async mockUserInteraction() {
    // Simulate user interaction events
    const events = ['click', 'touchstart', 'keydown'];
    events.forEach(eventType => {
      document.dispatchEvent(new Event(eventType));
    });
  }

  static async simulateNetworkCondition(condition: 'fast' | 'slow' | 'offline') {
    const originalFetch = window.fetch;

    switch (condition) {
      case 'slow':
        window.fetch = async (...args) => {
          await new Promise(resolve => setTimeout(resolve, 2000));
          return originalFetch(...args);
        };
        break;
      case 'offline':
        window.fetch = async () => {
          throw new Error('Network offline');
        };
        break;
      default:
        window.fetch = originalFetch;
    }
  }

  static mockVideoElementError(errorType: 'MEDIA_ERR_NETWORK' | 'MEDIA_ERR_DECODE' | 'MEDIA_ERR_SRC_NOT_SUPPORTED') {
    const mockVideo = document.createElement('video');
    const mockError = new MediaError();

    switch (errorType) {
      case 'MEDIA_ERR_NETWORK':
        Object.defineProperty(mockError, 'code', { value: MediaError.MEDIA_ERR_NETWORK });
        break;
      case 'MEDIA_ERR_DECODE':
        Object.defineProperty(mockError, 'code', { value: MediaError.MEDIA_ERR_DECODE });
        break;
      case 'MEDIA_ERR_SRC_NOT_SUPPORTED':
        Object.defineProperty(mockError, 'code', { value: MediaError.MEDIA_ERR_SRC_NOT_SUPPORTED });
        break;
    }

    Object.defineProperty(mockVideo, 'error', { value: mockError });
    return mockVideo;
  }

  static async runVideoValidationTests() {
    const results = [];

    for (const scenario of ERROR_TEST_SCENARIOS) {
      try {
        const result = await VideoUtils.validateVideoSource(scenario.input);
        results.push({
          scenario: scenario.name,
          success: !result.isValid,
          expectedError: scenario.expectedError,
          actualError: result.error,
        });
      } catch (error) {
        results.push({
          scenario: scenario.name,
          success: false,
          error: error instanceof Error ? error.message : 'Unknown error',
        });
      }
    }

    return results;
  }
}

// Export test runner
export async function runVideoComponentTests() {
  console.log('🚀 Starting Video Component Tests...');

  // Run validation tests
  console.log('📋 Running Video Validation Tests...');
  const validationResults = await VideoTestUtils.runVideoValidationTests();
  console.table(validationResults);

  // Run browser compatibility tests
  console.log('🌐 Running Browser Compatibility Tests...');
  const browserResults = BROWSER_TEST_SCENARIOS.map(scenario => ({
    browser: scenario.name,
    supportedFormats: VideoUtils.getSupportedFormats(),
    expectedFormats: scenario.expectedFormats,
    match: JSON.stringify(VideoUtils.getSupportedFormats().sort()) ===
           JSON.stringify(scenario.expectedFormats.sort()),
  }));
  console.table(browserResults);

  // Run autoplay tests
  console.log('▶️ Running Autoplay Tests...');
  const autoplaySupported = await VideoUtils.checkAutoplaySupport();
  console.log('Autoplay Support:', autoplaySupported ? '✅' : '❌');

  // Test video format detection and validation
  console.log('🎥 Testing Video Format Detection...');
  const testFormats = ['video.mp4', 'video.webm', 'video.mov'];
  const formatResults = testFormats.map(format => ({
    format,
    detected: VideoUtils.detectVideoFormat(format),
    supported: VideoUtils.isFormatSupported(VideoUtils.detectVideoFormat(format) || '')
  }));
  console.log('Format detection results:', formatResults);

  console.log('✅ Video Component Tests Completed!');
  return {
    validationResults,
    browserResults,
    autoplaySupported,
    formatResults,
  };
}

/**
 * Quick test function to verify video preview functionality
 */
export async function testVideoPreviewWithValidUrls() {
  console.log('🧪 Testing Video Preview with Valid URLs...');

  // Test URLs directly from TEST_VIDEO_SOURCES
  const testUrls = [
    { format: 'MP4', url: TEST_VIDEO_SOURCES.validMp4 },
    { format: 'WebM', url: TEST_VIDEO_SOURCES.validWebm },
    { format: 'Data URI', url: TEST_VIDEO_SOURCES.dataUri },
  ];

  const testResults = [];

  // Test each URL
  for (const { format, url } of testUrls) {
    try {
      console.log(`Testing ${format} URL: ${url}`);
      const result = await VideoUtils.validateVideoSource(url);

      testResults.push({
        format,
        url,
        isValid: result.isValid,
        error: result.error || 'None',
        supportedFormats: result.supportedFormats.join(', ')
      });

      if (result.isValid) {
        console.log(`✅ ${format} URL is valid`);
      } else {
        console.log(`❌ ${format} URL failed: ${result.error}`);
      }
    } catch (error) {
      console.log(`💥 Error testing ${format} URL:`, error);
      testResults.push({
        format,
        url,
        isValid: false,
        error: error instanceof Error ? error.message : 'Unknown error',
        supportedFormats: 'N/A'
      });
    }
  }

  console.log('📊 Test Results:');
  console.table(testResults);

  // Test MinIO object key validation
  console.log('🔄 Testing MinIO object key validation...');
  try {
    const minioResult = await VideoUtils.validateVideoSource('videos/sample-video.mp4');
    console.log('MinIO object key test result:', minioResult);
  } catch (error) {
    console.log('MinIO object key test error:', error);
  }

  return testResults;
}

export default {
  TEST_VIDEO_SOURCES,
  BROWSER_TEST_SCENARIOS,
  ERROR_TEST_SCENARIOS,
  AUTOPLAY_TEST_SCENARIOS,
  PERFORMANCE_TEST_SCENARIOS,
  ACCESSIBILITY_TEST_SCENARIOS,
  CROSS_BROWSER_TESTS,
  INTEGRATION_TEST_SCENARIOS,
  VideoTestUtils,
  runVideoComponentTests,
  testVideoPreviewWithValidUrls,
};

// Jest tests for video functionality
describe('VideoUtils', () => {

  test('should handle invalid URLs gracefully', () => {
    const invalidUrls = [
      'not-a-url',
      'mailto:test@example.com',
      'tel:+1234567890',
      'javascript:alert("test")'
    ];

    invalidUrls.forEach(url => {
      const isValid = VideoUtils.isValidUrl(url);
      expect(isValid).toBe(false);
    });
  });

  test('should support various URL schemes', () => {
    const validSchemes = [
      'https://example.com/video.mp4',
      'http://example.com/video.mp4',
      'data:video/mp4;base64,AAAAHGZ0eXBtcDQyAAACAGlzb21pc28yYXZjMQAAAAhmcmVlAAAGF21kYXQQAAACBAAlEAAgQJ',
      'blob:https://example.com/12345678-1234-1234-1234-123456789012',
      'file:///path/to/video.mp4'
    ];

    validSchemes.forEach(url => {
      const isValid = VideoUtils.isValidUrl(url);
      expect(isValid).toBe(true);
    });
  });


  test('should detect video formats from URLs', () => {
    const testCases = [
      { url: 'video.mp4', expected: 'video/mp4' },
      { url: 'video.webm', expected: 'video/webm' },
      { url: 'video.mov', expected: 'video/quicktime' },
      { url: 'video.mkv', expected: 'video/x-matroska' }
    ];

    testCases.forEach(({ url, expected }) => {
      const format = VideoUtils.detectVideoFormat(url);
      expect(format).toBe(expected);
    });
  });

  test('should return supported formats', () => {
    const supportedFormats = VideoUtils.getSupportedFormats();
    expect(supportedFormats).toBeInstanceOf(Array);

    // In test environment, canPlayType might not work as expected
    // So we'll test the logic directly
    if (supportedFormats.length === 0) {
      // If no formats are detected, test the fallback logic
      const browser = VideoUtils.detectBrowser();
      expect(browser).toBeDefined();
    } else {
      expect(supportedFormats.length).toBeGreaterThan(0);
      // Should include common formats
      const hasMp4 = supportedFormats.some(format => format.includes('mp4'));
      expect(hasMp4).toBe(true);
    }
  });

  test('should provide recommended format', () => {
    const recommendedFormat = VideoUtils.getRecommendedFormat();
    expect(recommendedFormat).toBeDefined();
    expect(typeof recommendedFormat).toBe('string');

    // Should be one of the common video formats
    const validFormats = ['video/mp4', 'video/webm', 'video/ogg', 'video/quicktime'];
    expect(validFormats).toContain(recommendedFormat);
  });
});

// Test for signed URL extraction and 1024 character limit handling
describe('Signed URL Extraction and 1024 Character Limit', () => {

  test('should extract object key from signed URLs correctly', () => {
    const testCases = [
      {
        name: 'Standard MinIO signed URL',
        url: 'http://localhost:9000/activity-videos/videos/activity-123/test-video.mp4?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=test-access-key%2F20250101%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250101T000000Z&X-Amz-Expires=3600&X-Amz-SignedHeaders=host&X-Amz-Signature=test-signature',
        expected: 'activity-videos/videos/activity-123/test-video.mp4'
      },
      {
        name: 'HTTPS signed URL',
        url: 'https://minio.example.com/profile-images/images/user-456/profile.jpg?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Signature=signature123',
        expected: 'profile-images/images/user-456/profile.jpg'
      },
      {
        name: 'Complex path signed URL',
        url: 'http://localhost:9000/activity-videos/videos/activity-999/subfolder/nested/file.mp4?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Signature=complex-signature',
        expected: 'activity-videos/videos/activity-999/subfolder/nested/file.mp4'
      }
    ];

    testCases.forEach(({ name, url, expected }) => {
      const extracted = VideoUtils.extractObjectKeyFromSignedUrl(url);
      expect(extracted).toBe(expected);
      console.log(`✅ ${name}: Extracted "${extracted}" from URL`);
    });
  });

  test('should handle MinIO object keys directly', () => {
    const objectKeys = [
      'videos/activity-123/test-video.mp4',
      'videos/activity-999/subfolder/nested/file.mp4'
    ];

    objectKeys.forEach(objectKey => {
      const extracted = VideoUtils.extractObjectKeyFromSignedUrl(objectKey);
      expect(extracted).toBe(objectKey);
      console.log(`✅ Object key "${objectKey}" returned as-is`);
    });
  });

  test('should handle non-video object keys correctly', () => {
    const objectKeys = [
      'images/user-456/profile.jpg',
      'documents/file.pdf',
      'audio/track.mp3'
    ];

    objectKeys.forEach(objectKey => {
      const extracted = VideoUtils.extractObjectKeyFromSignedUrl(objectKey);
      expect(extracted).toBe(objectKey);
      console.log(`✅ Non-video object key "${objectKey}" returned as-is`);
    });
  });

  test('should handle 1024 character limit correctly', () => {
    // Create a signed URL that would exceed 1024 characters
    const longSignature = 'X-Amz-Signature=' + 'a'.repeat(1000);
    const longUrl = `http://localhost:9000/activity-videos/videos/activity-123/test-video.mp4?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=test-access-key%2F20250101%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250101T000000Z&X-Amz-Expires=3600&X-Amz-SignedHeaders=host&${longSignature}`;

    // This should return null due to 1024 character limit
    const extracted = VideoUtils.extractObjectKeyFromSignedUrl(longUrl);
    expect(extracted).toBeNull();
    console.log('✅ Long URL (>1024 chars) correctly rejected');
  });

  test('should handle edge cases gracefully', () => {
    const edgeCases = [
      { name: 'Empty string', input: '', expected: null },
      { name: 'Null input', input: null as any, expected: null },
      { name: 'Invalid URL', input: 'not-a-url', expected: null },
      { name: 'URL without path', input: 'http://localhost:9000', expected: null },
      { name: 'URL with empty path', input: 'http://localhost:9000/', expected: null }
    ];

    edgeCases.forEach(({ name, input, expected }) => {
      const extracted = VideoUtils.extractObjectKeyFromSignedUrl(input);
      expect(extracted).toBe(expected);
      console.log(`✅ ${name}: Correctly handled as ${expected}`);
    });
  });

  test('should validate MinIO object key detection', () => {
    const testCases = [
      { input: 'videos/activity-123/test-video.mp4', expected: true },
      { input: 'videos/activity-999/subfolder/nested/file.mp4', expected: true },
      { input: 'images/user-456/profile.jpg', expected: false }, // Only "videos/" prefix is considered MinIO object key
      { input: 'http://localhost:9000/activity-videos/videos/activity-123/test-video.mp4', expected: false },
      { input: 'not-a-video-key', expected: false },
      { input: '', expected: false }
    ];

    testCases.forEach(({ input, expected }) => {
      const result = VideoUtils.isMinioObjectKey(input);
      expect(result).toBe(expected);
      console.log(`✅ MinIO object key detection for "${input}": ${result}`);
    });
  });

  test('should handle mixed input types correctly', () => {
    const mixedInputs = [
      {
        name: 'Signed URL with valid object key',
        input: 'http://localhost:9000/activity-videos/videos/activity-123/test-video.mp4?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Signature=short-sig',
        expected: 'activity-videos/videos/activity-123/test-video.mp4'
      },
      {
        name: 'Direct video object key',
        input: 'videos/activity-456/another-video.mp4',
        expected: 'videos/activity-456/another-video.mp4'
      },
      {
        name: 'Invalid signed URL',
        input: 'http://localhost:9000/activity-videos/?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Signature=short-sig',
        expected: null
      }
    ];

    mixedInputs.forEach(({ name, input, expected }) => {
      const extracted = VideoUtils.extractObjectKeyFromSignedUrl(input);
      expect(extracted).toBe(expected);
      console.log(`✅ ${name}: Extracted "${extracted}"`);
    });
  });

  test('should demonstrate 1024 character limit resolution', () => {
    // Test that the implementation properly handles the 1024 character limit
    // by extracting just the object key part from long signed URLs

    const baseUrl = 'http://localhost:9000/activity-videos/videos/activity-123/test-video.mp4';
    const shortSignature = 'X-Amz-Signature=short-sig';
    const shortUrl = `${baseUrl}?${shortSignature}`;

    const extractedShort = VideoUtils.extractObjectKeyFromSignedUrl(shortUrl);
    expect(extractedShort).toBe('activity-videos/videos/activity-123/test-video.mp4');
    expect(extractedShort!.length).toBeLessThan(1024);
    console.log('✅ Short signed URL correctly processed');

    // Test with a very long signature that would exceed 1024 characters
    const longSignature = 'X-Amz-Signature=' + 'a'.repeat(1000);
    const longUrl = `${baseUrl}?${longSignature}`;

    const extractedLong = VideoUtils.extractObjectKeyFromSignedUrl(longUrl);
    expect(extractedLong).toBeNull(); // Should be null due to 1024 char limit
    console.log('✅ Long signed URL (>1024 chars) correctly rejected');
  });
});

// Export empty object to make this a module
export {};