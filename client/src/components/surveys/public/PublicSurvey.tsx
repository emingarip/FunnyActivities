import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Survey, SurveyActivity } from '../../../types/survey.types';
import VoteService from '../../../services/voteService';
import SurveyService from '../../../services/surveyService';
import SurveyActivities from './SurveyActivities';
import ParticipantRegistration from './ParticipantRegistration';
import VoteButton from '../shared/VoteButton';
import './PublicSurvey.css';

interface PublicSurveyProps {
  surveyId?: string;
  onVoteClick?: (surveyId: string, activityId: string) => void;
  onBack?: () => void;
}

const PublicSurvey: React.FC<PublicSurveyProps> = ({
  surveyId: propSurveyId,
  onVoteClick,
  onBack,
}) => {
  const { surveyId: urlSurveyId } = useParams<{ surveyId: string }>();
  const navigate = useNavigate();
  const surveyId = propSurveyId || urlSurveyId;

  const [survey, setSurvey] = useState<Survey | null>(null);
  const [activities, setActivities] = useState<SurveyActivity[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedActivity, setSelectedActivity] = useState<SurveyActivity | null>(null);
  const [participantId, setParticipantId] = useState<string | null>(null);
  const [showRegistration, setShowRegistration] = useState(false);

  useEffect(() => {
    if (surveyId) {
      checkParticipantRegistration();
    }
  }, [surveyId]);

  const checkParticipantRegistration = async () => {
    if (!surveyId) return;

    // Check if participant is already registered in session storage
    const storedParticipantId = sessionStorage.getItem(`survey_${surveyId}_participant`);
    if (storedParticipantId) {
      setParticipantId(storedParticipantId);
      await loadSurveyData();
      return;
    }

    // If not registered, show registration form
    setShowRegistration(true);
    setLoading(false);
  };

  const loadSurveyData = async () => {
    if (!surveyId) return;

    try {
      setLoading(true);
      setError(null);

      // Load survey data
      const surveyResponse = await VoteService.getPublicSurvey(surveyId);
      if (surveyResponse.success && surveyResponse.data) {
        setSurvey(surveyResponse.data);
      } else {
        throw new Error(surveyResponse.message || 'Failed to load survey');
      }

      // Load activities using the survey ID
      const survey = surveyResponse.data;
      const activitiesResponse = await VoteService.getSurveyActivities(survey.id);
      if (activitiesResponse.success) {
        setActivities(activitiesResponse.data);
      }
    } catch (err) {
      console.error('Error loading survey data:', err);
      setError(err instanceof Error ? err.message : 'Failed to load survey');
    } finally {
      setLoading(false);
    }
  };

  const handleRegistrationSuccess = async (participantId: string) => {
    setParticipantId(participantId);
    setShowRegistration(false);
    await loadSurveyData();
  };

  const handleVoteClick = (activity: SurveyActivity, voteValue: number) => {
    if (onVoteClick) {
      onVoteClick(surveyId!, activity.id);
    }
    // Voting is now handled inline in SurveyActivities component
  };

  const handleBack = () => {
    if (onBack) {
      onBack();
    } else {
      navigate(-1);
    }
  };

  const handleRefresh = () => {
    loadSurveyData();
  };

  if (loading) {
    return (
      <div className="public-survey public-survey--loading">
        <div className="public-survey__loading">
          <div className="public-survey__spinner" aria-hidden="true">
            <svg width="40" height="40" viewBox="0 0 24 24" fill="none">
              <circle
                cx="12"
                cy="12"
                r="10"
                stroke="currentColor"
                strokeWidth="2"
                strokeDasharray="32"
                strokeDashoffset="32"
              >
                <animate
                  attributeName="stroke-dasharray"
                  dur="2s"
                  values="0 32;16 16;0 32;0 32"
                  repeatCount="indefinite"
                />
                <animate
                  attributeName="stroke-dashoffset"
                  dur="2s"
                  values="0;-16;-32;-32"
                  repeatCount="indefinite"
                />
              </circle>
            </svg>
          </div>
          <p className="public-survey__loading-text">Loading survey...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="public-survey public-survey--error">
        <div className="public-survey__error">
          <div className="public-survey__error-icon" aria-hidden="true">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <circle cx="12" cy="12" r="10"/>
              <line x1="15" y1="9" x2="9" y2="15"/>
              <line x1="9" y1="9" x2="15" y2="15"/>
            </svg>
          </div>
          <h2 className="public-survey__error-title">Survey Not Found</h2>
          <p className="public-survey__error-message">{error}</p>
          <div className="public-survey__error-actions">
            <VoteButton onClick={handleRefresh} variant="outline">
              Try Again
            </VoteButton>
            <VoteButton onClick={handleBack} variant="secondary">
              Go Back
            </VoteButton>
          </div>
        </div>
      </div>
    );
  }

  // Show registration form if participant is not registered
  if (showRegistration) {
    return (
      <ParticipantRegistration
        onRegistrationSuccess={handleRegistrationSuccess}
        onCancel={handleBack}
      />
    );
  }

  if (!survey) {
    return (
      <div className="public-survey public-survey--not-found">
        <div className="public-survey__error">
          <h2 className="public-survey__error-title">Survey Not Found</h2>
          <p className="public-survey__error-message">The survey you're looking for doesn't exist.</p>
          <VoteButton onClick={handleBack}>Go Back</VoteButton>
        </div>
      </div>
    );
  }

  const canVote = VoteService.canUserVote(survey);

  return (
    <div className="public-survey">
      <div className="public-survey__header">
        <button
          className="public-survey__back-button"
          onClick={handleBack}
          aria-label="Go back"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <polyline points="15,18 9,12 15,6"/>
          </svg>
          Back
        </button>
        <div className="public-survey__header-content">
          <h1 className="public-survey__title">{survey.title}</h1>
          <div className="public-survey__meta">
            <span className="public-survey__meta-item">
              Created by {survey.createdByUserName}
            </span>
            <span className="public-survey__meta-separator">•</span>
            <span className="public-survey__meta-item">
              {activities.length} {activities.length === 1 ? 'activity' : 'activities'}
            </span>
          </div>
        </div>
      </div>

      <div className="public-survey__content">
        <SurveyActivities
          activities={activities}
          survey={survey}
          onVoteClick={handleVoteClick}
          canVote={canVote.canVote}
          participantId={participantId}
        />
      </div>
    </div>
  );
};

export default PublicSurvey;
