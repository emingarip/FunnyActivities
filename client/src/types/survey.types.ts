// Survey System TypeScript Interfaces
// Following the existing API patterns and backend DTOs

export {}; // Make this a module

// Base interfaces
export interface Survey {
  id: string;
  title: string;
  description: string;
  createdByUserId: string;
  createdByUserName: string;
  isActive: boolean;
  startDate: string;
  endDate?: string;
  maxParticipants?: number;
  participantCount: number;
  createdAt: string;
  updatedAt: string;
  activities: SurveyActivity[];
  isCurrentlyActive: boolean;
  hasReachedMaxParticipants: boolean;
  shareToken?: string;
}

export interface SurveyActivity {
  id: string;
  surveyId: string;
  activityId: string;
  activityName: string;
  activityDescription: string;
  durationMinutes?: number;
  averageVote: number;
  voteCount: number;
  order: number;
  videoUrl?: string;
}

export interface Vote {
  id: string;
  surveyId: string;
  surveyActivityId: string;
  surveyParticipantId: string;
  participantName: string;
  voteValue: number;
  createdAt: string;
  updatedAt: string;
}

export interface SurveyParticipant {
  id: string;
  surveyId: string;
  firstName: string;
  lastName: string;
  childrenCount: number;
  participatedAt: string;
  completedAt?: string;
  isCompleted: boolean;
}

export interface SurveyResults {
  surveyId: string;
  surveyTitle: string;
  totalParticipants: number;
  completedCount: number;
  completionRate: number;
  activityResults: ActivityResult[];
  votes: Vote[];
}

export interface ActivityResult {
  surveyActivityId: string;
  activityId: string;
  activityName: string;
  activityDescription: string;
  averageVote: number;
  voteCount: number;
  voteDistribution: Record<number, number>;
}


// Request interfaces
export interface CreateSurveyRequest {
  title: string;
  description?: string;
  startDate: string;
  endDate?: string;
  maxParticipants?: number;
  activityIds: string[];
}

export interface UpdateSurveyRequest {
  title?: string;
  description?: string;
  startDate?: string;
  endDate?: string;
  maxParticipants?: number;
  activityIds?: string[];
}

export interface VoteRequest {
  surveyActivityId: string;
  voteValue: number;
  surveyParticipantId: string;
}

// List and pagination interfaces
export interface SurveyListItem {
  id: string;
  title: string;
  description?: string;
  createdByUserName: string;
  isActive: boolean;
  startDate: string;
  endDate?: string;
  participantCount: number;
  maxParticipants?: number;
  createdAt: string;
  updatedAt: string;
  isCurrentlyActive: boolean;
  hasReachedMaxParticipants: boolean;
  shareToken?: string;
}

export interface SurveyListResponse {
  success: boolean;
  data: {
    surveys: SurveyListItem[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
  };
}

// API Response wrappers
export interface SurveyResponse {
  success: boolean;
  message?: string;
  data?: Survey;
}

export interface SurveyListApiResponse {
  success: boolean;
  message?: string;
  data?: {
    surveys: SurveyListItem[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
  };
}

export interface SurveyResultsResponse {
  success: boolean;
  message?: string;
  data?: SurveyResults;
}


export interface SurveyParticipantsResponse {
  success: boolean;
  message?: string;
  data?: SurveyParticipant[];
}

export interface VoteResponse {
  success: boolean;
  message?: string;
  data?: Vote;
}

// Query parameters
export interface GetSurveysParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  status?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

// Survey status interface
export interface SurveyStatus {
  surveyId: string;
  title: string;
  isActive: boolean;
  isCurrentlyActive: boolean;
  startDate: string;
  endDate?: string;
  totalActivities: number;
  canVote: boolean;
}

// Error interfaces
export interface SurveyError {
  success: boolean;
  message: string;
  error: string;
  status?: number;
}

// Form validation interfaces
export interface SurveyFormData {
  title: string;
  description: string;
  startDate: string;
  endDate?: string;
  maxParticipants?: number;
  activityIds: string[];
}

export interface SurveyFormErrors {
  title?: string;
  description?: string;
  startDate?: string;
  endDate?: string;
  maxParticipants?: string;
  activityIds?: string;
  general?: string;
}

// Chart and visualization interfaces
export interface SurveyChartData {
  labels: string[];
  datasets: {
    label: string;
    data: number[];
    backgroundColor?: string[];
    borderColor?: string[];
    borderWidth?: number;
  }[];
}

export interface VoteDistributionChart {
  voteValue: number;
  count: number;
  percentage: number;
  color: string;
}

// Real-time update interfaces
export interface SurveyUpdateEvent {
  type: 'vote_submitted' | 'survey_updated' | 'participant_joined';
  surveyId: string;
  data: any;
  timestamp: string;
}

export interface SurveyNotification {
  id: string;
  type: 'info' | 'success' | 'warning' | 'error';
  title: string;
  message: string;
  timestamp: string;
  isRead: boolean;
}