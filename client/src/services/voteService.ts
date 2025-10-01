import axios from 'axios';
import {
  Survey,
  SurveyActivity,
  VoteRequest,
  Vote,
  SurveyStatus,
  VoteResponse,
  SurveyResponse,
} from '../types/survey.types';

// Use direct axios for public endpoints to avoid authentication
const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:8080/api';

/**
 * Vote Service for public voting operations
 * Handles anonymous/public survey voting operations
 */
export class VoteService {
  /**
   * Get public survey by share token (no authentication required)
   */
  static async getPublicSurvey(shareToken: string): Promise<SurveyResponse> {
    const response = await axios.get(`${API_BASE_URL}/surveys/share/${shareToken}`);
    return response.data;
  }

  /**
   * Get survey activities for public voting (no authentication required)
   */
  static async getSurveyActivities(surveyId: string): Promise<{ success: boolean; data: SurveyActivity[] }> {
    console.log('📡 VoteService: Fetching survey activities:', {
      surveyId,
      surveyIdType: typeof surveyId,
      surveyIdLength: surveyId?.length,
      apiUrl: `${API_BASE_URL}/surveys/${surveyId}/activities`
    });

    const response = await axios.get(`${API_BASE_URL}/surveys/${surveyId}/activities`);

    console.log('📡 VoteService: Survey activities API response:', {
      responseStatus: response?.status,
      responseData: response?.data,
      hasData: !!response?.data,
      dataType: typeof response?.data,
      success: response?.data?.success,
      data: response?.data?.data,
      dataTypeInner: typeof response?.data?.data,
      dataIsArray: Array.isArray(response?.data?.data),
      dataLength: response?.data?.data?.length,
      firstActivity: response?.data?.data?.[0],
      firstActivityVideoUrl: response?.data?.data?.[0]?.videoUrl,
      firstActivityVideoUrlType: typeof response?.data?.data?.[0]?.videoUrl,
      firstActivityVideoUrlLength: response?.data?.data?.[0]?.videoUrl?.length
    });

    return response.data;
  }

  /**
   * Get survey status (no authentication required)
   */
  static async getSurveyStatus(surveyId: string): Promise<{ success: boolean; data: SurveyStatus }> {
    const response = await axios.get(`${API_BASE_URL}/surveys/${surveyId}/status`);
    return response.data;
  }

  /**
   * Get votes for a participant (no authentication required)
   */
  static async getParticipantVotes(participantId: string): Promise<{ success: boolean; data: Vote[] }> {
    const response = await axios.get(`${API_BASE_URL}/surveys/participant/${participantId}/votes`);
    return response.data;
  }

  /**
   * Submit a vote for a survey activity (no authentication required)
   */
  static async submitVote(request: VoteRequest): Promise<VoteResponse> {
    const response = await axios.post(`${API_BASE_URL}/surveys/vote`, request);
    return response.data;
  }

  /**
   * Check if survey is active and accepting votes
   */
  static async checkSurveyAvailability(surveyId: string): Promise<boolean> {
    try {
      const statusResponse = await this.getSurveyStatus(surveyId);
      return statusResponse.data.canVote;
    } catch (error) {
      console.error('Error checking survey availability:', error);
      return false;
    }
  }

  /**
   * Validate vote request
   */
  static validateVoteRequest(request: VoteRequest): { isValid: boolean; errors: string[] } {
    const errors: string[] = [];

    if (!request.surveyActivityId) {
      errors.push('Survey activity ID is required');
    }

    if (!request.voteValue || request.voteValue < 1 || request.voteValue > 5) {
      errors.push('Vote value must be between 1 and 5');
    }

    return {
      isValid: errors.length === 0,
      errors,
    };
  }

  /**
   * Format vote value for display
   */
  static formatVoteValue(voteValue: number): string {
    const stars = '★'.repeat(voteValue) + '☆'.repeat(5 - voteValue);
    return `${voteValue}/5 ${stars}`;
  }

  /**
   * Get vote color based on value
   */
  static getVoteColor(voteValue: number): string {
    if (voteValue >= 4) return 'green';
    if (voteValue >= 3) return 'orange';
    return 'red';
  }

  /**
   * Calculate average vote display
   */
  static formatAverageVote(averageVote: number): string {
    return (Math.round(averageVote * 10) / 10).toFixed(1);
  }

  /**
   * Get vote distribution for visualization
   */
  static getVoteDistribution(votes: Vote[]): Record<number, number> {
    const distribution: Record<number, number> = { 1: 0, 2: 0, 3: 0, 4: 0, 5: 0 };

    votes.forEach(vote => {
      distribution[vote.voteValue]++;
    });

    return distribution;
  }

  /**
   * Get vote statistics
   */
  static getVoteStatistics(votes: Vote[]): {
    totalVotes: number;
    averageVote: number;
    distribution: Record<number, number>;
    highestVote: number;
    lowestVote: number;
  } {
    if (votes.length === 0) {
      return {
        totalVotes: 0,
        averageVote: 0,
        distribution: { 1: 0, 2: 0, 3: 0, 4: 0, 5: 0 },
        highestVote: 0,
        lowestVote: 0,
      };
    }

    const voteValues = votes.map(v => v.voteValue);
    const totalVotes = votes.length;
    const averageVote = voteValues.reduce((sum, vote) => sum + vote, 0) / totalVotes;
    const distribution = this.getVoteDistribution(votes);
    const highestVote = Math.max(...voteValues);
    const lowestVote = Math.min(...voteValues);

    return {
      totalVotes,
      averageVote,
      distribution,
      highestVote,
      lowestVote,
    };
  }

  /**
   * Check if user can vote (client-side validation)
   */
  static canUserVote(survey: Survey): { canVote: boolean; reason?: string } {
    const now = new Date();
    const startDate = new Date(survey.startDate);
    const endDate = survey.endDate ? new Date(survey.endDate) : null;

    if (!survey.isActive) {
      return { canVote: false, reason: 'Survey is not active' };
    }

    if (now < startDate) {
      return { canVote: false, reason: 'Survey has not started yet' };
    }

    if (endDate && now > endDate) {
      return { canVote: false, reason: 'Survey has ended' };
    }

    if (survey.maxParticipants && survey.participantCount >= survey.maxParticipants) {
      return { canVote: false, reason: 'Survey has reached maximum participants' };
    }

    return { canVote: true };
  }

  /**
   * Get time until survey starts
   */
  static getTimeUntilStart(survey: Survey): { days: number; hours: number; minutes: number } | null {
    const now = new Date();
    const startDate = new Date(survey.startDate);

    if (now >= startDate) {
      return null;
    }

    const diffMs = startDate.getTime() - now.getTime();
    const days = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    const hours = Math.floor((diffMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    const minutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));

    return { days, hours, minutes };
  }

  /**
   * Get time until survey ends
   */
  static getTimeUntilEnd(survey: Survey): { days: number; hours: number; minutes: number } | null {
    if (!survey.endDate) {
      return null;
    }

    const now = new Date();
    const endDate = new Date(survey.endDate);

    if (now >= endDate) {
      return null;
    }

    const diffMs = endDate.getTime() - now.getTime();
    const days = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    const hours = Math.floor((diffMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    const minutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));

    return { days, hours, minutes };
  }

  /**
   * Format countdown time
   */
  static formatCountdown(time: { days: number; hours: number; minutes: number }): string {
    const parts: string[] = [];

    if (time.days > 0) {
      parts.push(`${time.days}d`);
    }
    if (time.hours > 0) {
      parts.push(`${time.hours}h`);
    }
    if (time.minutes > 0) {
      parts.push(`${time.minutes}m`);
    }

    return parts.join(' ') || 'Less than a minute';
  }

  /**
   * Get survey progress percentage
   */
  static getSurveyProgress(survey: Survey): number {
    if (!survey.endDate) {
      return 0;
    }

    const now = new Date();
    const startDate = new Date(survey.startDate);
    const endDate = new Date(survey.endDate);

    if (now <= startDate) {
      return 0;
    }
    if (now >= endDate) {
      return 100;
    }

    const totalDuration = endDate.getTime() - startDate.getTime();
    const elapsed = now.getTime() - startDate.getTime();

    return Math.round((elapsed / totalDuration) * 100);
  }

  /**
   * Get participation rate
   */
  static getParticipationRate(survey: Survey): number {
    if (!survey.maxParticipants || survey.maxParticipants === 0) {
      return 0;
    }

    return Math.round((survey.participantCount / survey.maxParticipants) * 100);
  }
}

// Export default instance
export default VoteService;