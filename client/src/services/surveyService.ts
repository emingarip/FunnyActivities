import api from './api';
import {
  Survey,
  CreateSurveyRequest,
  UpdateSurveyRequest,
  SurveyResponse,
  SurveyListApiResponse,
  SurveyResultsResponse,
  SurveyParticipantsResponse,
  GetSurveysParams,
  SurveyFormData,
  SurveyFormErrors,
} from '../types/survey.types';

/**
 * Survey Service for admin operations
 * Handles authenticated survey management operations
 */
export class SurveyService {
  /**
   * Get paginated list of surveys with filtering and sorting
   */
  static async getSurveys(params?: GetSurveysParams): Promise<SurveyListApiResponse> {
    const queryParams = {
      pageNumber: params?.pageNumber || 1,
      pageSize: params?.pageSize || 10,
      searchTerm: params?.searchTerm,
      status: params?.status,
      sortBy: params?.sortBy || 'createdAt',
      sortOrder: params?.sortOrder || 'desc',
    };

    const response = await api.get('/surveys', { params: queryParams });
    return response.data;
  }

  /**
   * Get a specific survey by ID
   */
  static async getSurvey(id: string): Promise<SurveyResponse> {
    const response = await api.get(`/surveys/${id}`);
    return response.data;
  }

  /**
   * Create a new survey
   */
  static async createSurvey(request: CreateSurveyRequest): Promise<SurveyResponse> {
    const response = await api.post('/surveys', request);
    return response.data;
  }

  /**
   * Update an existing survey
   */
  static async updateSurvey(id: string, request: UpdateSurveyRequest): Promise<SurveyResponse> {
    const response = await api.put(`/surveys/${id}`, request);
    return response.data;
  }

  /**
   * Delete a survey
   */
  static async deleteSurvey(id: string): Promise<{ success: boolean; message: string }> {
    const response = await api.delete(`/surveys/${id}`);
    return response.data;
  }

  /**
   * Get survey results with detailed statistics
   */
  static async getSurveyResults(id: string): Promise<SurveyResultsResponse> {
    const response = await api.get(`/surveys/${id}/results?includeIndividualVotes=true`);
    return response.data;
  }


  /**
   * Get survey participants
   */
  static async getSurveyParticipants(id: string): Promise<SurveyParticipantsResponse> {
    const response = await api.get(`/surveys/${id}/participants`);
    return response.data;
  }

  /**
   * Get share URL for a survey
   */
  static async getShareUrl(id: string): Promise<{ success: boolean; data: { surveyId: string; shareUrl: string; shareToken: string } }> {
    const response = await api.get(`/surveys/${id}/share-url`);
    return response.data;
  }

  /**
   * Register a participant for a survey
   */
  static async registerParticipant(shareToken: string, participantData: { firstName: string; lastName: string; childrenCount: number }): Promise<{ success: boolean; data: any; message?: string }> {
    const response = await api.post(`/surveys/share/${shareToken}/register`, participantData);
    return response.data;
  }

  /**
   * Validate survey form data
   */
  static validateSurveyForm(formData: SurveyFormData): SurveyFormErrors {
    const errors: SurveyFormErrors = {};

    // Title validation
    if (!formData.title.trim()) {
      errors.title = 'Survey title is required';
    } else if (formData.title.length > 200) {
      errors.title = 'Survey title cannot exceed 200 characters';
    }

    // Description validation
    if (formData.description && formData.description.length > 1000) {
      errors.description = 'Survey description cannot exceed 1000 characters';
    }

    // Start date validation
    if (!formData.startDate) {
      errors.startDate = 'Survey start date is required';
    } else {
      const startDate = new Date(formData.startDate);
      const now = new Date();

      if (startDate <= now) {
        errors.startDate = 'Start date must be in the future';
      }
    }

    // End date validation
    if (formData.endDate) {
      const startDate = new Date(formData.startDate);
      const endDate = new Date(formData.endDate);

      if (endDate <= startDate) {
        errors.endDate = 'End date must be after start date';
      }
    }

    // Max participants validation
    if (formData.maxParticipants !== undefined) {
      if (formData.maxParticipants < 1) {
        errors.maxParticipants = 'Maximum participants must be greater than 0';
      } else if (formData.maxParticipants > 10000) {
        errors.maxParticipants = 'Maximum participants cannot exceed 10,000';
      }
    }

    // Activity IDs validation
    if (!formData.activityIds || formData.activityIds.length === 0) {
      errors.activityIds = 'At least one activity is required';
    }

    return errors;
  }

  /**
   * Format survey data for display
   */
  static formatSurveyForDisplay(survey: Survey): Survey {
    return {
      ...survey,
      startDate: new Date(survey.startDate).toLocaleDateString(),
      endDate: survey.endDate ? new Date(survey.endDate).toLocaleDateString() : undefined,
      createdAt: new Date(survey.createdAt).toLocaleString(),
      updatedAt: new Date(survey.updatedAt).toLocaleString(),
      activities: survey.activities.map(activity => ({
        ...activity,
        averageVote: Math.round(activity.averageVote * 10) / 10, // Round to 1 decimal place
      })),
    };
  }

  /**
   * Calculate survey completion percentage
   */
  static calculateCompletionPercentage(survey: Survey): number {
    if (survey.maxParticipants && survey.maxParticipants > 0) {
      return Math.round((survey.participantCount / survey.maxParticipants) * 100);
    }
    return 0;
  }

  /**
   * Check if survey is currently active
   */
  static isSurveyActive(survey: Survey): boolean {
    const now = new Date();
    const startDate = new Date(survey.startDate);
    const endDate = survey.endDate ? new Date(survey.endDate) : null;

    const hasStarted = now >= startDate;
    const hasEnded = endDate ? now > endDate : false;
    const hasReachedMax = survey.maxParticipants ?
      survey.participantCount >= survey.maxParticipants : false;

    return survey.isActive && hasStarted && !hasEnded && !hasReachedMax;
  }

  /**
   * Get survey status text
   */
  static getSurveyStatusText(survey: Survey): string {
    if (!survey.isActive) {
      return 'Inactive';
    }

    const now = new Date();
    const startDate = new Date(survey.startDate);
    const endDate = survey.endDate ? new Date(survey.endDate) : null;

    if (now < startDate) {
      return 'Scheduled';
    }

    if (endDate && now > endDate) {
      return 'Completed';
    }

    if (survey.maxParticipants && survey.participantCount >= survey.maxParticipants) {
      return 'Full';
    }

    return 'Active';
  }

  /**
   * Get survey status color for UI
   */
  static getSurveyStatusColor(survey: Survey): string {
    const status = this.getSurveyStatusText(survey);

    switch (status) {
      case 'Active':
        return 'green';
      case 'Scheduled':
        return 'blue';
      case 'Completed':
        return 'gray';
      case 'Full':
        return 'orange';
      case 'Inactive':
        return 'red';
      default:
        return 'gray';
    }
  }

  /**
   * Export survey data as JSON
   */
  static exportSurveyData(survey: Survey): string {
    const exportData = {
      ...survey,
      exportedAt: new Date().toISOString(),
      exportedBy: 'Admin Panel',
    };

    return JSON.stringify(exportData, null, 2);
  }

  /**
   * Import survey data from JSON
   */
  static importSurveyData(jsonData: string): Partial<CreateSurveyRequest> {
    try {
      const parsed = JSON.parse(jsonData);

      return {
        title: parsed.title,
        description: parsed.description,
        startDate: parsed.startDate,
        endDate: parsed.endDate,
        maxParticipants: parsed.maxParticipants,
        activityIds: parsed.activities?.map((a: any) => a.activityId) || [],
      };
    } catch (error) {
      throw new Error('Invalid JSON format');
    }
  }
}

// Export default instance
export default SurveyService;