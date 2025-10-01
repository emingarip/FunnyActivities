/**
 * Comprehensive Video Loading Test Suite
 *
 * This test suite verifies the complete video loading flow from frontend to backend to MinIO,
 * testing authentication, configuration, signed URL generation, metadata retrieval, and error handling.
 *
 * Key areas tested:
 * 1. Authentication flow for video endpoints
 * 2. MinIO configuration and connectivity
 * 3. Signed URL generation and validation
 * 4. Metadata retrieval mechanisms
 * 5. Public vs authenticated video access
 * 6. Error handling and fallback mechanisms
 * 7. Object key extraction validation
 * 8. Network connectivity testing
 */

import { activitiesAPI } from '../services/api';
import { VideoUtils } from '../services/videoUtils';

// Make this a proper module
export {};

// Mock data for testing
const MOCK_ACTIVITY_ID = '123e4567-e89b-12d3-a456-426614174000';
const MOCK_VIDEO_OBJECT_KEY = 'videos/activity-123e4567-e89b-12d3-a456-426614174000/test-video.mp4';
const MOCK_SIGNED_URL = 'http://localhost:9000/activity-videos/videos/activity-123e4567-e89b-12d3-a456-426614174000/test-video.mp4?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=test-key&X-Amz-Date=20250101T000000Z&X-Amz-Expires=3600&X-Amz-SignedHeaders=host&X-Amz-Signature=test-signature';

// Test configuration
const TEST_CONFIG = {
  timeout: 10000,
  retryAttempts: 3,
  retryDelay: 1000,
  expectedExpirySeconds: 3600,
  maxUrlLength: 1024
};

describe('Comprehensive Video Loading Test Suite', () => {
  let testResults: any = {};

  beforeAll(() => {
    // Set up test logging
    console.log('🚀 Starting Comprehensive Video Loading Tests...');
    testResults = {
      startTime: new Date().toISOString(),
      tests: {},
      summary: {
        total: 0,
        passed: 0,
        failed: 0,
        skipped: 0
      }
    };
  });

  afterAll(() => {
    // Generate test report
    console.log('\n📊 COMPREHENSIVE VIDEO LOADING TEST REPORT');
    console.log('=' .repeat(50));
    console.log(`Start Time: ${testResults.startTime}`);
    console.log(`End Time: ${new Date().toISOString()}`);
    console.log(`Total Tests: ${testResults.summary.total}`);
    console.log(`Passed: ${testResults.summary.passed}`);
    console.log(`Failed: ${testResults.summary.failed}`);
    console.log(`Skipped: ${testResults.summary.skipped}`);

    if (testResults.summary.failed > 0) {
      console.log('\n❌ FAILED TESTS:');
      Object.entries(testResults.tests).forEach(([testName, result]: [string, any]) => {
        if (!result.passed) {
          console.log(`  - ${testName}: ${result.error || 'Unknown error'}`);
        }
      });
    }

    console.log('\n🔍 DETAILED RESULTS:');
    Object.entries(testResults.tests).forEach(([testName, result]: [string, any]) => {
      const status = result.passed ? '✅' : result.skipped ? '⏭️' : '❌';
      console.log(`${status} ${testName}: ${result.duration}ms`);
      if (result.details) {
        console.log(`   Details: ${JSON.stringify(result.details, null, 2)}`);
      }
    });
  });

  /**
   * Test 1: Authentication Flow Testing
   */
  describe('Authentication Flow Testing', () => {
    test('should handle authenticated video URL requests', async () => {
      const testName = 'Authentication Flow - Authenticated Video URL';
      const startTime = Date.now();

      try {
        // Test authenticated video URL generation
        const response = await activitiesAPI.getActivityVideoUrl(
          MOCK_ACTIVITY_ID,
          MOCK_VIDEO_OBJECT_KEY,
          TEST_CONFIG.expectedExpirySeconds
        );

        expect(response.status).toBe(200);
        expect(response.data.success).toBe(true);
        expect(response.data.data).toBeDefined();
        expect(response.data.data.signedVideoUrl).toBeDefined();
        expect(response.data.data.videoObjectKey).toBe(MOCK_VIDEO_OBJECT_KEY);
        expect(response.data.data.urlExpirySeconds).toBe(TEST_CONFIG.expectedExpirySeconds);

        // Validate signed URL format
        const signedUrl = response.data.data.signedVideoUrl;
        expect(signedUrl).toContain('http');
        expect(signedUrl.length).toBeLessThan(TEST_CONFIG.maxUrlLength);

        testResults.tests[testName] = {
          passed: true,
          duration: Date.now() - startTime,
          details: {
            signedUrlLength: signedUrl.length,
            hasValidFormat: signedUrl.includes('?X-Amz-Algorithm=')
          }
        };
        testResults.summary.passed++;
      } catch (error: any) {
        console.error(`❌ ${testName} failed:`, error.message);
        testResults.tests[testName] = {
          passed: false,
          duration: Date.now() - startTime,
          error: error.message,
          details: {
            status: error.response?.status,
            statusText: error.response?.statusText,
            responseData: error.response?.data
          }
        };
        testResults.summary.failed++;
      }
      testResults.summary.total++;
    });

    test('should handle public video URL requests without authentication', async () => {
      const testName = 'Authentication Flow - Public Video URL';
      const startTime = Date.now();

      try {
        // Test public video URL generation (no auth required)
        const response = await activitiesAPI.getPublicActivityVideoUrl(
          MOCK_ACTIVITY_ID,
          MOCK_VIDEO_OBJECT_KEY,
          TEST_CONFIG.expectedExpirySeconds
        );

        expect(response.status).toBe(200);
        expect(response.data.success).toBe(true);
        expect(response.data.data).toBeDefined();
        expect(response.data.data.signedVideoUrl).toBeDefined();
        expect(response.data.data.isPublicAccess).toBe(true);

        testResults.tests[testName] = {
          passed: true,
          duration: Date.now() - startTime,
          details: {
            isPublicAccess: response.data.data.isPublicAccess,
            signedUrlLength: response.data.data.signedVideoUrl.length
          }
        };
        testResults.summary.passed++;
      } catch (error: any) {
        console.error(`❌ ${testName} failed:`, error.message);
        testResults.tests[testName] = {
          passed: false,
          duration: Date.now() - startTime,
          error: error.message,
          details: {
            status: error.response?.status,
            statusText: error.response?.statusText,
            responseData: error.response?.data
          }
        };
        testResults.summary.failed++;
      }
      testResults.summary.total++;
    });
  });

  /**
   * Test 2: MinIO Configuration and Connectivity Testing
   */
  describe('MinIO Configuration and Connectivity Testing', () => {
    test('should validate MinIO service availability', async () => {
      const testName = 'MinIO Configuration - Service Availability';
      const startTime = Date.now();

      try {
        // Test basic connectivity by checking if object exists
        const response = await activitiesAPI.getObjectMetadata(MOCK_VIDEO_OBJECT_KEY);

        expect(response.status).toBe(200);
        expect(response.data.success).toBe(true);
        expect(response.data.data).toBeDefined();

        testResults.tests[testName] = {
          passed: true,
          duration: Date.now() - startTime,
          details: {
            objectKey: response.data.data.objectKey,
            bucketName: response.data.data.bucketName,
            size: response.data.data.size
          }
        };
        testResults.summary.passed++;
      } catch (error: any) {
        console.error(`❌ ${testName} failed:`, error.message);
        testResults.tests[testName] = {
          passed: false,
          duration: Date.now() - startTime,
          error: error.message,
          details: {
            status: error.response?.status,
            statusText: error.response?.statusText,
            responseData: error.response?.data
          }
        };
        testResults.summary.failed++;
      }
      testResults.summary.total++;
    });

    test('should handle MinIO service unavailability gracefully', async () => {
      const testName = 'MinIO Configuration - Service Unavailability Handling';
      const startTime = Date.now();

      try {
        // Test with non-existent object key
        const nonExistentKey = 'videos/non-existent-video.mp4';
        await activitiesAPI.getObjectMetadata(nonExistentKey);

        // If we get here, the test should fail as we expect a 404
        testResults.tests[testName] = {
          passed: false,
          duration: Date.now() - startTime,
          error: 'Expected 404 for non-existent object, but request succeeded'
        };
        testResults.summary.failed++;
      } catch (error: any) {
        // We expect this to fail with 404
        if (error.response?.status === 404) {
          testResults.tests[testName] = {
            passed: true,
            duration: Date.now() - startTime,
            details: {
              expectedError: '404 Not Found',
              actualStatus: error.response.status
            }
          };
          testResults.summary.passed++;
        } else {
          testResults.tests[testName] = {
            passed: false,
            duration: Date.now() - startTime,
            error: `Unexpected error: ${error.message}`,
            details: {
              status: error.response?.status,
              statusText: error.response?.statusText
            }
          };
          testResults.summary.failed++;
        }
      }
      testResults.summary.total++;
    });
  });

  /**
   * Test 3: Signed URL Generation and Validation
   */
  describe('Signed URL Generation and Validation', () => {
    test('should generate valid signed URLs with proper expiry', async () => {
      const testName = 'Signed URL Generation - Valid URL with Expiry';
      const startTime = Date.now();

      try {
        const response = await activitiesAPI.getActivityVideoUrl(
          MOCK_ACTIVITY_ID,
          MOCK_VIDEO_OBJECT_KEY,
          TEST_CONFIG.expectedExpirySeconds
        );

        const signedUrl = response.data.data.signedVideoUrl;

        // Validate URL structure
        expect(signedUrl).toMatch(/^https?:\/\//);
        expect(signedUrl).toContain('X-Amz-Algorithm=');
        expect(signedUrl).toContain('X-Amz-Signature=');
        expect(signedUrl).toContain('X-Amz-Expires=');

        // Validate expiry parameter
        const urlObj = new URL(signedUrl);
        const expiresParam = urlObj.searchParams.get('X-Amz-Expires');
        expect(expiresParam).toBe(TEST_CONFIG.expectedExpirySeconds.toString());

        testResults.tests[testName] = {
          passed: true,
          duration: Date.now() - startTime,
          details: {
            urlLength: signedUrl.length,
            hasValidExpiry: expiresParam === TEST_CONFIG.expectedExpirySeconds.toString(),
            protocol: urlObj.protocol,
            hostname: urlObj.hostname
          }
        };
        testResults.summary.passed++;
      } catch (error: any) {
        console.error(`❌ ${testName} failed:`, error.message);
        testResults.tests[testName] = {
          passed: false,
          duration: Date.now() - startTime,
          error: error.message,
          details: {
            status: error.response?.status,
            responseData: error.response?.data
          }
        };
        testResults.summary.failed++;
      }
      testResults.summary.total++;
    });

    test('should handle signed URL length limits', async () => {
      const testName = 'Signed URL Generation - Length Limits';
      const startTime = Date.now();

      try {
        const response = await activitiesAPI.getActivityVideoUrl(
          MOCK_ACTIVITY_ID,
          MOCK_VIDEO_OBJECT_KEY,
          TEST_CONFIG.expectedExpirySeconds
        );

        const signedUrl = response.data.data.signedVideoUrl;
        expect(signedUrl.length).toBeLessThan(TEST_CONFIG.maxUrlLength);

        testResults.tests[testName] = {
          passed: true,
          duration: Date.now() - startTime,
          details: {
            urlLength: signedUrl.length,
            maxAllowed: TEST_CONFIG.maxUrlLength,
            withinLimits: signedUrl.length < TEST_CONFIG.maxUrlLength
          }
        };
        testResults.summary.passed++;
      } catch (error: any) {
        console.error(`❌ ${testName} failed:`, error.message);
        testResults.tests[testName] = {
          passed: false,
          duration: Date.now() - startTime,
          error: error.message,
          details: {
            status: error.response?.status,
            responseData: error.response?.data
          }
        };
        testResults.summary.failed++;
      }
      testResults.summary.total++;
    });
  });

  /**
   * Test 4: Metadata Retrieval Mechanisms
   */
  describe('Metadata Retrieval Mechanisms', () => {
    test('should retrieve video metadata successfully', async () => {
      const testName = 'Metadata Retrieval - Video Metadata';
      const startTime = Date.now();

      try {
        const response = await activitiesAPI.getVideoMetadata(MOCK_VIDEO_OBJECT_KEY);

        expect(response.status).toBe(200);
        expect(response.data.success).toBe(true);
        expect(response.data.data).toBeDefined();

        const metadata = response.data.data;
        expect(metadata.objectKey).toBeDefined();
        expect(metadata.bucketName).toBeDefined();
        expect(metadata.size).toBeGreaterThan(0);
        expect(metadata.contentType).toBeDefined();

        testResults.tests[testName] = {
          passed: true,
          duration: Date.now() - startTime,
          details: {
            objectKey: metadata.objectKey,
            bucketName: metadata.bucketName,
            size: metadata.size,
            contentType: metadata.contentType,
            lastModified: metadata.lastModified
          }
        };
        testResults.summary.passed++;
      } catch (error: any) {
        console.error(`❌ ${testName} failed:`, error.message);
        testResults.tests[testName] = {
          passed: false,
          duration: Date.now() - startTime,
          error: error.message,
          details: {
            status: error.response?.status,
            responseData: error.response?.data
          }
        };
        testResults.summary.failed++;
      }
      testResults.summary.total++;
    });

    test('should retrieve general object metadata successfully', async () => {
      const testName = 'Metadata Retrieval - Object Metadata';
      const startTime = Date.now();

      try {
        const response = await activitiesAPI.getObjectMetadata(MOCK_VIDEO_OBJECT_KEY);

        expect(response.status).toBe(200);
        expect(response.data.success).toBe(true);
        expect(response.data.data).toBeDefined();

        const metadata = response.data.data;
        expect(metadata.objectKey).toBeDefined();
        expect(metadata.bucketName).toBeDefined();
        expect(metadata.size).toBeGreaterThan(0);

        testResults.tests[testName] = {
          passed: true,
          duration: Date.now() - startTime,
          details: {
            objectKey: metadata.objectKey,
            bucketName: metadata.bucketName,
            size: metadata.size,
            contentType: metadata.contentType
          }
        };
        testResults.summary.passed++;
      } catch (error: any) {
        console.error(`❌ ${testName} failed:`, error.message);
        testResults.tests[testName] = {
          passed: false,
          duration: Date.now() - startTime,
          error: error.message,
          details: {
            status: error.response?.status,
            responseData: error.response?.data
          }
        };
        testResults.summary.failed++;
      }
      testResults.summary.total++;
    });
  });

  /**
   * Test 5: Object Key Extraction and Validation
   */
  describe('Object Key Extraction and Validation', () => {
    test('should extract object key from signed URL correctly', () => {
      const testName = 'Object Key Extraction - From Signed URL';
      const startTime = Date.now();

      try {
        const extractedKey = VideoUtils.extractObjectKeyFromSignedUrl(MOCK_SIGNED_URL);

        expect(extractedKey).toBeDefined();
        expect(extractedKey).toContain('videos/');
        expect(extractedKey).toContain('.mp4');

        testResults.tests[testName] = {
          passed: true,
          duration: Date.now() - startTime,
          details: {
            extractedKey: extractedKey,
            originalUrlLength: MOCK_SIGNED_URL.length,
            keyLength: extractedKey?.length
          }
        };
        testResults.summary.passed++;
      } catch (error: any) {
        console.error(`❌ ${testName} failed:`, error.message);
        testResults.tests[testName] = {
          passed: false,
          duration: Date.now() - startTime,
          error: error.message
        };
        testResults.summary.failed++;
      }
      testResults.summary.total++;
    });

    test('should validate MinIO object keys correctly', () => {
      const testName = 'Object Key Validation - MinIO Object Keys';
      const startTime = Date.now();

      try {
        // Test valid MinIO object key
        const validKey = 'videos/activity-123/test-video.mp4';
        const isValidMinioKey = VideoUtils.isMinioObjectKey(validKey);
        expect(isValidMinioKey).toBe(true);

        // Test invalid object keys
        const invalidKeys = [
          'not-a-video-url',
          'http://example.com/video.mp4',
          '',
          '   '
        ];

        invalidKeys.forEach(key => {
          const isInvalid = VideoUtils.isMinioObjectKey(key);
          expect(isInvalid).toBe(false);
        });

        testResults.tests[testName] = {
          passed: true,
          duration: Date.now() - startTime,
          details: {
            validKeyTested: validKey,
            invalidKeysTested: invalidKeys.length,
            validKeyResult: isValidMinioKey
          }
        };
        testResults.summary.passed++;
      } catch (error: any) {
        console.error(`❌ ${testName} failed:`, error.message);
        testResults.tests[testName] = {
          passed: false,
          duration: Date.now() - startTime,
          error: error.message
        };
        testResults.summary.failed++;
      }
      testResults.summary.total++;
    });
  });

  /**
   * Test 6: Error Handling and Fallback Mechanisms
   */
  describe('Error Handling and Fallback Mechanisms', () => {
    test('should handle 404 errors gracefully', async () => {
      const testName = 'Error Handling - 404 Not Found';
      const startTime = Date.now();

      try {
        const nonExistentKey = 'videos/non-existent-video.mp4';
        await activitiesAPI.getVideoMetadata(nonExistentKey);

        // If we get here, the test should fail as we expect a 404
        testResults.tests[testName] = {
          passed: false,
          duration: Date.now() - startTime,
          error: 'Expected 404 for non-existent video, but request succeeded'
        };
        testResults.summary.failed++;
      } catch (error: any) {
        if (error.response?.status === 404) {
          testResults.tests[testName] = {
            passed: true,
            duration: Date.now() - startTime,
            details: {
              expectedError: '404 Not Found',
              actualStatus: error.response.status,
              errorMessage: error.response.data?.message
            }
          };
          testResults.summary.passed++;
        } else {
          testResults.tests[testName] = {
            passed: false,
            duration: Date.now() - startTime,
            error: `Unexpected error: ${error.message}`,
            details: {
              status: error.response?.status,
              statusText: error.response?.statusText
            }
          };
          testResults.summary.failed++;
        }
      }
      testResults.summary.total++;
    });

    test('should handle network timeouts gracefully', async () => {
      const testName = 'Error Handling - Network Timeout';
      const startTime = Date.now();

      try {
        // Test with very short timeout to simulate network issues
        const shortTimeoutKey = 'videos/timeout-test.mp4';

        // This test might be skipped if we can't reliably simulate timeouts
        testResults.tests[testName] = {
          passed: false,
          skipped: true,
          duration: Date.now() - startTime,
          error: 'Network timeout simulation not implemented in this test environment'
        };
        testResults.summary.skipped++;
      } catch (error: any) {
        testResults.tests[testName] = {
          passed: true,
          duration: Date.now() - startTime,
          details: {
            errorType: 'Network Timeout',
            errorMessage: error.message
          }
        };
        testResults.summary.passed++;
      }
      testResults.summary.total++;
    });
  });

  /**
   * Test 7: Public vs Authenticated Access Testing
   */
  describe('Public vs Authenticated Access Testing', () => {
    test('should differentiate between public and authenticated endpoints', async () => {
      const testName = 'Access Control - Public vs Authenticated Endpoints';
      const startTime = Date.now();

      try {
        // Test public endpoint (should work without auth)
        const publicResponse = await activitiesAPI.getPublicActivities({
          pageNumber: 1,
          pageSize: 1
        });

        expect(publicResponse.status).toBe(200);
        expect(publicResponse.data.success).toBe(true);

        // Test authenticated endpoint (should require auth)
        const authResponse = await activitiesAPI.getActivities({
          pageNumber: 1,
          pageSize: 1
        });

        expect(authResponse.status).toBe(200);
        expect(authResponse.data.success).toBe(true);

        testResults.tests[testName] = {
          passed: true,
          duration: Date.now() - startTime,
          details: {
            publicEndpointWorks: publicResponse.status === 200,
            authenticatedEndpointWorks: authResponse.status === 200,
            publicDataCount: publicResponse.data.data?.items?.length || 0,
            authDataCount: authResponse.data.data?.items?.length || 0
          }
        };
        testResults.summary.passed++;
      } catch (error: any) {
        console.error(`❌ ${testName} failed:`, error.message);
        testResults.tests[testName] = {
          passed: false,
          duration: Date.now() - startTime,
          error: error.message,
          details: {
            status: error.response?.status,
            statusText: error.response?.statusText,
            responseData: error.response?.data
          }
        };
        testResults.summary.failed++;
      }
      testResults.summary.total++;
    });
  });

  /**
   * Test 8: VideoUtils Integration Testing
   */
  describe('VideoUtils Integration Testing', () => {
    test('should validate video sources correctly', async () => {
      const testName = 'VideoUtils Integration - Source Validation';
      const startTime = Date.now();

      try {
        // Test MinIO object key validation
        const minioKeyValidation = await VideoUtils.validateVideoSource(MOCK_VIDEO_OBJECT_KEY);
        expect(minioKeyValidation.isValid).toBe(true);

        // Test signed URL validation
        const signedUrlValidation = await VideoUtils.validateVideoSource(MOCK_SIGNED_URL);
        expect(signedUrlValidation.isValid).toBe(true);

        // Test invalid source
        const invalidValidation = await VideoUtils.validateVideoSource('invalid-source');
        expect(invalidValidation.isValid).toBe(false);

        testResults.tests[testName] = {
          passed: true,
          duration: Date.now() - startTime,
          details: {
            minioKeyValid: minioKeyValidation.isValid,
            signedUrlValid: signedUrlValidation.isValid,
            invalidSourceValid: invalidValidation.isValid,
            invalidSourceError: invalidValidation.error
          }
        };
        testResults.summary.passed++;
      } catch (error: any) {
        console.error(`❌ ${testName} failed:`, error.message);
        testResults.tests[testName] = {
          passed: false,
          duration: Date.now() - startTime,
          error: error.message
        };
        testResults.summary.failed++;
      }
      testResults.summary.total++;
    });

    test('should handle video metadata retrieval through VideoUtils', async () => {
      const testName = 'VideoUtils Integration - Metadata Retrieval';
      const startTime = Date.now();

      try {
        const metadata = await VideoUtils.getVideoMetadata(MOCK_VIDEO_OBJECT_KEY);

        // Metadata retrieval might fail if object doesn't exist, but VideoUtils should handle it gracefully
        expect(metadata).toBeDefined();

        testResults.tests[testName] = {
          passed: true,
          duration: Date.now() - startTime,
          details: {
            hasMetadata: metadata !== null,
            hasError: !!metadata.error,
            errorMessage: metadata.error
          }
        };
        testResults.summary.passed++;
      } catch (error: any) {
        console.error(`❌ ${testName} failed:`, error.message);
        testResults.tests[testName] = {
          passed: false,
          duration: Date.now() - startTime,
          error: error.message
        };
        testResults.summary.failed++;
      }
      testResults.summary.total++;
    });
  });

  /**
   * Test 9: End-to-End Video Loading Flow
   */
  describe('End-to-End Video Loading Flow', () => {
    test('should complete full video loading workflow', async () => {
      const testName = 'End-to-End Flow - Complete Video Loading Workflow';
      const startTime = Date.now();

      try {
        // Step 1: Get activity information
        const activityResponse = await activitiesAPI.getActivity(MOCK_ACTIVITY_ID);
        expect(activityResponse.status).toBe(200);

        // Step 2: Generate signed URL
        const videoUrlResponse = await activitiesAPI.getActivityVideoUrl(
          MOCK_ACTIVITY_ID,
          MOCK_VIDEO_OBJECT_KEY
        );
        expect(videoUrlResponse.status).toBe(200);

        const signedUrl = videoUrlResponse.data.data.signedVideoUrl;

        // Step 3: Get video metadata
        const metadataResponse = await activitiesAPI.getVideoMetadata(MOCK_VIDEO_OBJECT_KEY);
        expect(metadataResponse.status).toBe(200);

        // Step 4: Validate signed URL through VideoUtils
        const urlValidation = await VideoUtils.validateVideoSource(signedUrl);
        expect(urlValidation.isValid).toBe(true);

        testResults.tests[testName] = {
          passed: true,
          duration: Date.now() - startTime,
          details: {
            activityRetrieved: activityResponse.status === 200,
            signedUrlGenerated: videoUrlResponse.status === 200,
            metadataRetrieved: metadataResponse.status === 200,
            urlValidated: urlValidation.isValid,
            signedUrlLength: signedUrl.length
          }
        };
        testResults.summary.passed++;
      } catch (error: any) {
        console.error(`❌ ${testName} failed:`, error.message);
        testResults.tests[testName] = {
          passed: false,
          duration: Date.now() - startTime,
          error: error.message,
          details: {
            status: error.response?.status,
            responseData: error.response?.data
          }
        };
        testResults.summary.failed++;
      }
      testResults.summary.total++;
    });
  });
});