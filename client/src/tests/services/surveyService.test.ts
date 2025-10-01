import SurveyService from '../../services/surveyService';
import api from '../../services/api';
import {
  Survey,
  SurveyListItem,
  SurveyResults,
  CreateSurveyRequest,
  UpdateSurveyRequest,
  SurveyResponse,
  SurveyListApiResponse,
  SurveyResultsResponse,
  GetSurveysParams,
  SurveyFormData,
  SurveyFormErrors,
} from '../../types/survey.types';

// Mock the API module
jest.mock('../../services/api', () => ({
  __esModule: true,
  default: {
    get: jest.fn(),
    post: jest.fn(),
    put: jest.fn(),
    delete: jest.fn(),
  },
}));

const mockedApi = api as jest.Mocked<typeof api>;

describe('SurveyService', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('getSurveys', () => {
    it('should return surveys list successfully', async () => {
      const mockResponse: SurveyListApiResponse = {
        success: true,
        data: {
          surveys: [
            {
              id: '123e4567-e89b-12d3-a456-426614174000',
              title: 'Test Survey 1',
              description: 'Description 1',
              createdByUserName: 'Test User',
              isActive: true,
              isCurrentlyActive: true,
              startDate: '2024-01-01T00:00:00Z',
              endDate: '2024-12-31T23:59:59Z',
              participantCount: 5,
              maxParticipants: 100,
              createdAt: '2024-01-01T00:00:00Z',
              updatedAt: '2024-01-01T00:00:00Z',
              hasReachedMaxParticipants: false,
              shareToken: 'abc123def456',
            },
            {
              id: '123e4567-e89b-12d3-a456-426614174001',
              title: 'Test Survey 2',
              description: 'Description 2',
              createdByUserName: 'Another User',
              isActive: false,
              isCurrentlyActive: false,
              startDate: '2024-02-01T00:00:00Z',
              endDate: '2024-02-28T23:59:59Z',
              participantCount: 0,
              maxParticipants: 50,
              createdAt: '2024-01-15T00:00:00Z',
              updatedAt: '2024-01-15T00:00:00Z',
              hasReachedMaxParticipants: false,
              shareToken: 'def456ghi789',
            },
          ],
          totalCount: 2,
          page: 1,
          pageSize: 10,
          totalPages: 1,
        },
      };

      mockedApi.get.mockResolvedValueOnce({ data: mockResponse });

      const result = await SurveyService.getSurveys();

      expect(mockedApi.get).toHaveBeenCalledWith('/surveys', {
        params: {
          pageNumber: 1,
          pageSize: 10,
          sortBy: 'createdAt',
          sortOrder: 'desc',
        },
      });
      expect(result).toEqual(mockResponse);
    });

    it('should handle API errors gracefully', async () => {
      const errorMessage = 'Failed to fetch surveys';
      mockedApi.get.mockRejectedValueOnce(new Error(errorMessage));

      await expect(SurveyService.getSurveys()).rejects.toThrow(errorMessage);
    });
  });

  describe('getSurvey', () => {
    it('should return single survey successfully', async () => {
      const surveyId = '123e4567-e89b-12d3-a456-426614174000';
      const mockResponse: SurveyResponse = {
        success: true,
        data: {
          id: surveyId,
          title: 'Test Survey',
          description: 'Test Description',
          createdByUserId: 'user123',
          createdByUserName: 'Test User',
          isActive: true,
          startDate: '2024-01-01T00:00:00Z',
          endDate: '2024-12-31T23:59:59Z',
          maxParticipants: 100,
          participantCount: 5,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
          activities: [],
          isCurrentlyActive: true,
          hasReachedMaxParticipants: false,
          shareToken: 'testShareToken123',
        },
      };

      mockedApi.get.mockResolvedValueOnce({ data: mockResponse });

      const result = await SurveyService.getSurvey(surveyId);

      expect(mockedApi.get).toHaveBeenCalledWith(`/surveys/${surveyId}`);
      expect(result).toEqual(mockResponse);
    });

    it('should handle API errors gracefully', async () => {
      const surveyId = '123e4567-e89b-12d3-a456-426614174000';
      const errorMessage = 'Survey not found';
      mockedApi.get.mockRejectedValueOnce(new Error(errorMessage));

      await expect(SurveyService.getSurvey(surveyId)).rejects.toThrow(errorMessage);
    });
  });

  describe('createSurvey', () => {
    it('should create survey successfully', async () => {
      const request: CreateSurveyRequest = {
        title: 'New Survey',
        description: 'New Description',
        startDate: '2024-01-01T00:00:00Z',
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 100,
        activityIds: ['123e4567-e89b-12d3-a456-426614174000'],
      };

      const mockResponse: SurveyResponse = {
        success: true,
        data: {
          id: '123e4567-e89b-12d3-a456-426614174002',
          title: request.title,
          description: request.description || '',
          createdByUserId: 'user123',
          createdByUserName: 'Test User',
          isActive: false,
          startDate: request.startDate,
          endDate: request.endDate,
          maxParticipants: request.maxParticipants || 0,
          participantCount: 0,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
          activities: [],
          isCurrentlyActive: false,
          hasReachedMaxParticipants: false,
          shareToken: 'testShareToken123',
        },
      };

      mockedApi.post.mockResolvedValueOnce({ data: mockResponse });

      const result = await SurveyService.createSurvey(request);

      expect(mockedApi.post).toHaveBeenCalledWith('/surveys', request);
      expect(result).toEqual(mockResponse);
    });

    it('should handle validation errors', async () => {
      const request: CreateSurveyRequest = {
        title: '',
        description: '',
        startDate: '2024-01-01T00:00:00Z',
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 0,
        activityIds: [],
      };

      const errorMessage = 'Validation failed';
      mockedApi.post.mockRejectedValueOnce(new Error(errorMessage));

      await expect(SurveyService.createSurvey(request)).rejects.toThrow(errorMessage);
    });
  });

  describe('updateSurvey', () => {
    it('should update survey successfully', async () => {
      const surveyId = '123e4567-e89b-12d3-a456-426614174000';
      const request: UpdateSurveyRequest = {
        title: 'Updated Survey',
        description: 'Updated Description',
        startDate: '2024-01-01T00:00:00Z',
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 50,
        activityIds: ['123e4567-e89b-12d3-a456-426614174000'],
      };

      const mockResponse: SurveyResponse = {
        success: true,
        data: {
          id: surveyId,
          title: request.title || '',
          description: request.description || '',
          createdByUserId: 'user123',
          createdByUserName: 'Test User',
          isActive: true,
          startDate: request.startDate || '',
          endDate: request.endDate,
          maxParticipants: request.maxParticipants || 0,
          participantCount: 10,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: new Date().toISOString(),
          activities: [],
          isCurrentlyActive: true,
          hasReachedMaxParticipants: false,
          shareToken: 'testShareToken123',
        },
      };

      mockedApi.put.mockResolvedValueOnce({ data: mockResponse });

      const result = await SurveyService.updateSurvey(surveyId, request);

      expect(mockedApi.put).toHaveBeenCalledWith(`/surveys/${surveyId}`, request);
      expect(result).toEqual(mockResponse);
    });

    it('should handle API errors gracefully', async () => {
      const surveyId = '123e4567-e89b-12d3-a456-426614174000';
      const request: UpdateSurveyRequest = {
        title: 'Updated Survey',
        description: 'Updated Description',
        startDate: '2024-01-01T00:00:00Z',
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 50,
        activityIds: ['123e4567-e89b-12d3-a456-426614174000'],
      };

      const errorMessage = 'Survey not found';
      mockedApi.put.mockRejectedValueOnce(new Error(errorMessage));

      await expect(SurveyService.updateSurvey(surveyId, request)).rejects.toThrow(errorMessage);
    });
  });

  describe('deleteSurvey', () => {
    it('should delete survey successfully', async () => {
      const surveyId = '123e4567-e89b-12d3-a456-426614174000';
      const mockResponse = {
        success: true,
        message: 'Survey deleted successfully',
      };

      mockedApi.delete.mockResolvedValueOnce({ data: mockResponse });

      const result = await SurveyService.deleteSurvey(surveyId);

      expect(mockedApi.delete).toHaveBeenCalledWith(`/surveys/${surveyId}`);
      expect(result).toEqual(mockResponse);
    });

    it('should handle API errors gracefully', async () => {
      const surveyId = '123e4567-e89b-12d3-a456-426614174000';
      const errorMessage = 'Survey not found';
      mockedApi.delete.mockRejectedValueOnce(new Error(errorMessage));

      await expect(SurveyService.deleteSurvey(surveyId)).rejects.toThrow(errorMessage);
    });
  });

  describe('getSurveyResults', () => {
    it('should return survey results successfully', async () => {
      const surveyId = '123e4567-e89b-12d3-a456-426614174000';
      const mockResponse: SurveyResultsResponse = {
        success: true,
        data: {
          surveyId: surveyId,
          surveyTitle: 'Test Survey',
          totalParticipants: 10,
          completedCount: 8,
          completionRate: 80,
          activityResults: [
            {
              surveyActivityId: '123e4567-e89b-12d3-a456-426614174001',
              activityId: '123e4567-e89b-12d3-a456-426614174003',
              activityName: 'Activity 1',
              activityDescription: 'Description 1',
              averageVote: 4.2,
              voteCount: 10,
              voteDistribution: { 1: 0, 2: 1, 3: 2, 4: 4, 5: 3 },
            },
            {
              surveyActivityId: '123e4567-e89b-12d3-a456-426614174002',
              activityId: '123e4567-e89b-12d3-a456-426614174004',
              activityName: 'Activity 2',
              activityDescription: 'Description 2',
              averageVote: 3.8,
              voteCount: 15,
              voteDistribution: { 1: 1, 2: 2, 3: 4, 4: 5, 5: 3 },
            },
          ],
          votes: [],
        },
      };

      mockedApi.get.mockResolvedValueOnce({ data: mockResponse });

      const result = await SurveyService.getSurveyResults(surveyId);

      expect(mockedApi.get).toHaveBeenCalledWith(`/surveys/${surveyId}/results`);
      expect(result).toEqual(mockResponse);
    });

    it('should handle API errors gracefully', async () => {
      const surveyId = '123e4567-e89b-12d3-a456-426614174000';
      const errorMessage = 'Survey not found';
      mockedApi.get.mockRejectedValueOnce(new Error(errorMessage));

      await expect(SurveyService.getSurveyResults(surveyId)).rejects.toThrow(errorMessage);
    });
  });


  describe('validateSurveyForm', () => {
    it('should return no errors for valid form data', () => {
      const formData: SurveyFormData = {
        title: 'Valid Survey Title',
        description: 'Valid description',
        startDate: '2024-12-01T00:00:00Z',
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 100,
        activityIds: ['123e4567-e89b-12d3-a456-426614174000'],
      };

      const errors = SurveyService.validateSurveyForm(formData);

      expect(errors).toEqual({});
    });

    it('should return error for empty title', () => {
      const formData: SurveyFormData = {
        title: '',
        description: 'Valid description',
        startDate: '2024-12-01T00:00:00Z',
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 100,
        activityIds: ['123e4567-e89b-12d3-a456-426614174000'],
      };

      const errors = SurveyService.validateSurveyForm(formData);

      expect(errors.title).toBe('Survey title is required');
    });

    it('should return error for title too long', () => {
      const formData: SurveyFormData = {
        title: 'A'.repeat(201), // 201 characters
        description: 'Valid description',
        startDate: '2024-12-01T00:00:00Z',
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 100,
        activityIds: ['123e4567-e89b-12d3-a456-426614174000'],
      };

      const errors = SurveyService.validateSurveyForm(formData);

      expect(errors.title).toBe('Survey title cannot exceed 200 characters');
    });

    it('should return error for description too long', () => {
      const formData: SurveyFormData = {
        title: 'Valid Title',
        description: 'A'.repeat(1001), // 1001 characters
        startDate: '2024-12-01T00:00:00Z',
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 100,
        activityIds: ['123e4567-e89b-12d3-a456-426614174000'],
      };

      const errors = SurveyService.validateSurveyForm(formData);

      expect(errors.description).toBe('Survey description cannot exceed 1000 characters');
    });

    it('should return error for start date in the past', () => {
      const formData: SurveyFormData = {
        title: 'Valid Title',
        description: 'Valid description',
        startDate: '2024-01-01T00:00:00Z', // Past date
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 100,
        activityIds: ['123e4567-e89b-12d3-a456-426614174000'],
      };

      const errors = SurveyService.validateSurveyForm(formData);

      expect(errors.startDate).toBe('Start date must be in the future');
    });

    it('should return error for end date before start date', () => {
      const formData: SurveyFormData = {
        title: 'Valid Title',
        description: 'Valid description',
        startDate: '2024-12-31T00:00:00Z',
        endDate: '2024-12-01T00:00:00Z', // Before start date
        maxParticipants: 100,
        activityIds: ['123e4567-e89b-12d3-a456-426614174000'],
      };

      const errors = SurveyService.validateSurveyForm(formData);

      expect(errors.endDate).toBe('End date must be after start date');
    });

    it('should return error for invalid max participants', () => {
      const formData: SurveyFormData = {
        title: 'Valid Title',
        description: 'Valid description',
        startDate: '2024-12-01T00:00:00Z',
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 0, // Invalid
        activityIds: ['123e4567-e89b-12d3-a456-426614174000'],
      };

      const errors = SurveyService.validateSurveyForm(formData);

      expect(errors.maxParticipants).toBe('Maximum participants must be greater than 0');
    });

    it('should return error for too many max participants', () => {
      const formData: SurveyFormData = {
        title: 'Valid Title',
        description: 'Valid description',
        startDate: '2024-12-01T00:00:00Z',
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 10001, // Too many
        activityIds: ['123e4567-e89b-12d3-a456-426614174000'],
      };

      const errors = SurveyService.validateSurveyForm(formData);

      expect(errors.maxParticipants).toBe('Maximum participants cannot exceed 10,000');
    });

    it('should return error for no activities', () => {
      const formData: SurveyFormData = {
        title: 'Valid Title',
        description: 'Valid description',
        startDate: '2024-12-01T00:00:00Z',
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 100,
        activityIds: [], // No activities
      };

      const errors = SurveyService.validateSurveyForm(formData);

      expect(errors.activityIds).toBe('At least one activity is required');
    });
  });

  describe('formatSurveyForDisplay', () => {
    it('should format survey data correctly', () => {
      const survey: Survey = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        title: 'Test Survey',
        description: 'Test Description',
        createdByUserId: 'user123',
        createdByUserName: 'Test User',
        isActive: true,
        startDate: '2024-01-01T00:00:00Z',
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 100,
        participantCount: 5,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
        activities: [
          {
            id: 'activity1',
            surveyId: '123e4567-e89b-12d3-a456-426614174000',
            activityId: 'act123',
            activityName: 'Test Activity',
            activityDescription: 'Activity Description',
            averageVote: 4.25,
            voteCount: 10,
            order: 1,
          },
        ],
        isCurrentlyActive: true,
        hasReachedMaxParticipants: false,
      };

      const result = SurveyService.formatSurveyForDisplay(survey);

      expect(result.startDate).toBe('1/1/2024');
      expect(result.endDate).toBe('12/31/2024');
      expect(result.createdAt).toContain('1/1/2024');
      expect(result.activities[0].averageVote).toBe(4.3); // Rounded to 1 decimal
    });
  });

  describe('calculateCompletionPercentage', () => {
    it('should calculate completion percentage correctly', () => {
      const survey: Survey = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        title: 'Test Survey',
        description: 'Test Description',
        createdByUserId: 'user123',
        createdByUserName: 'Test User',
        isActive: true,
        startDate: '2024-01-01T00:00:00Z',
        maxParticipants: 100,
        participantCount: 25,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
        activities: [],
        isCurrentlyActive: true,
        hasReachedMaxParticipants: false,
      };

      const result = SurveyService.calculateCompletionPercentage(survey);

      expect(result).toBe(25);
    });

    it('should return 0 when no max participants', () => {
      const survey: Survey = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        title: 'Test Survey',
        description: 'Test Description',
        createdByUserId: 'user123',
        createdByUserName: 'Test User',
        isActive: true,
        startDate: '2024-01-01T00:00:00Z',
        participantCount: 25,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
        activities: [],
        isCurrentlyActive: true,
        hasReachedMaxParticipants: false,
      };

      const result = SurveyService.calculateCompletionPercentage(survey);

      expect(result).toBe(0);
    });
  });

  describe('isSurveyActive', () => {
    it('should return true for active survey', () => {
      const survey: Survey = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        title: 'Test Survey',
        description: 'Test Description',
        createdByUserId: 'user123',
        createdByUserName: 'Test User',
        isActive: true,
        startDate: '2024-01-01T00:00:00Z',
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 100,
        participantCount: 25,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
        activities: [],
        isCurrentlyActive: true,
        hasReachedMaxParticipants: false,
      };

      const result = SurveyService.isSurveyActive(survey);

      expect(result).toBe(true);
    });

    it('should return false for inactive survey', () => {
      const survey: Survey = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        title: 'Test Survey',
        description: 'Test Description',
        createdByUserId: 'user123',
        createdByUserName: 'Test User',
        isActive: false,
        startDate: '2024-01-01T00:00:00Z',
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 100,
        participantCount: 25,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
        activities: [],
        isCurrentlyActive: true,
        hasReachedMaxParticipants: false,
      };

      const result = SurveyService.isSurveyActive(survey);

      expect(result).toBe(false);
    });

    it('should return false for survey that has not started', () => {
      const survey: Survey = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        title: 'Test Survey',
        description: 'Test Description',
        createdByUserId: 'user123',
        createdByUserName: 'Test User',
        isActive: true,
        startDate: '2024-12-01T00:00:00Z', // Future date
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 100,
        participantCount: 25,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
        activities: [],
        isCurrentlyActive: true,
        hasReachedMaxParticipants: false,
      };

      const result = SurveyService.isSurveyActive(survey);

      expect(result).toBe(false);
    });

    it('should return false for survey that has ended', () => {
      const survey: Survey = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        title: 'Test Survey',
        description: 'Test Description',
        createdByUserId: 'user123',
        createdByUserName: 'Test User',
        isActive: true,
        startDate: '2024-01-01T00:00:00Z',
        endDate: '2024-01-31T23:59:59Z', // Past date
        maxParticipants: 100,
        participantCount: 25,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
        activities: [],
        isCurrentlyActive: true,
        hasReachedMaxParticipants: false,
      };

      const result = SurveyService.isSurveyActive(survey);

      expect(result).toBe(false);
    });

    it('should return false for survey at max capacity', () => {
      const survey: Survey = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        title: 'Test Survey',
        description: 'Test Description',
        createdByUserId: 'user123',
        createdByUserName: 'Test User',
        isActive: true,
        startDate: '2024-01-01T00:00:00Z',
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 100,
        participantCount: 100, // At max capacity
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
        activities: [],
        isCurrentlyActive: true,
        hasReachedMaxParticipants: true,
      };

      const result = SurveyService.isSurveyActive(survey);

      expect(result).toBe(false);
    });
  });

  describe('getSurveyStatusText', () => {
    it('should return "Active" for active survey', () => {
      const survey: Survey = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        title: 'Test Survey',
        description: 'Test Description',
        createdByUserId: 'user123',
        createdByUserName: 'Test User',
        isActive: true,
        startDate: '2024-01-01T00:00:00Z',
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 100,
        participantCount: 25,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
        activities: [],
        isCurrentlyActive: true,
        hasReachedMaxParticipants: false,
      };

      const result = SurveyService.getSurveyStatusText(survey);

      expect(result).toBe('Active');
    });

    it('should return "Inactive" for inactive survey', () => {
      const survey: Survey = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        title: 'Test Survey',
        description: 'Test Description',
        createdByUserId: 'user123',
        createdByUserName: 'Test User',
        isActive: false,
        startDate: '2024-01-01T00:00:00Z',
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 100,
        participantCount: 25,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
        activities: [],
        isCurrentlyActive: true,
        hasReachedMaxParticipants: false,
      };

      const result = SurveyService.getSurveyStatusText(survey);

      expect(result).toBe('Inactive');
    });

    it('should return "Scheduled" for survey that has not started', () => {
      const survey: Survey = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        title: 'Test Survey',
        description: 'Test Description',
        createdByUserId: 'user123',
        createdByUserName: 'Test User',
        isActive: true,
        startDate: '2024-12-01T00:00:00Z', // Future date
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 100,
        participantCount: 25,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
        activities: [],
        isCurrentlyActive: true,
        hasReachedMaxParticipants: false,
      };

      const result = SurveyService.getSurveyStatusText(survey);

      expect(result).toBe('Scheduled');
    });

    it('should return "Completed" for survey that has ended', () => {
      const survey: Survey = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        title: 'Test Survey',
        description: 'Test Description',
        createdByUserId: 'user123',
        createdByUserName: 'Test User',
        isActive: true,
        startDate: '2024-01-01T00:00:00Z',
        endDate: '2024-01-31T23:59:59Z', // Past date
        maxParticipants: 100,
        participantCount: 25,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
        activities: [],
        isCurrentlyActive: true,
        hasReachedMaxParticipants: false,
      };

      const result = SurveyService.getSurveyStatusText(survey);

      expect(result).toBe('Completed');
    });

    it('should return "Full" for survey at max capacity', () => {
      const survey: Survey = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        title: 'Test Survey',
        description: 'Test Description',
        createdByUserId: 'user123',
        createdByUserName: 'Test User',
        isActive: true,
        startDate: '2024-01-01T00:00:00Z',
        endDate: '2024-12-31T23:59:59Z',
        maxParticipants: 100,
        participantCount: 100, // At max capacity
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
        activities: [],
        isCurrentlyActive: true,
        hasReachedMaxParticipants: true,
      };

      const result = SurveyService.getSurveyStatusText(survey);

      expect(result).toBe('Full');
    });
  });
});