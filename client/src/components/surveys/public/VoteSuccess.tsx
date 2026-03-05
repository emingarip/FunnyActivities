import React from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import VoteButton from '../shared/VoteButton';
import './VoteSuccess.css';

const VoteSuccess: React.FC = () => {
  const { surveyId } = useParams<{ surveyId: string }>();
  const navigate = useNavigate();
  const location = useLocation();

  const state = location.state as { activityName?: string; voteValue?: number } | null;
  const activityName = state?.activityName;
  const voteValue = state?.voteValue;

  const handleBackToSurvey = () => {
    navigate(`/survey/${surveyId}`);
  };

  const handleShare = () => {
    if (navigator.share && typeof navigator.share === 'function') {
      navigator.share({
        title: 'Survey Vote',
        text: `I just voted ${voteValue} stars for "${activityName}" in a survey!`,
        url: window.location.origin + `/survey/${surveyId}`,
      });
    }
  };

  return (
    <div className="vote-success">
      <div className="vote-success__content">
        <div className="vote-success__icon" aria-hidden="true">
          <svg width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
            <polyline points="22,4 12,14.01 9,11.01"/>
          </svg>
        </div>

        <h1 className="vote-success__title">Thank You for Your Vote!</h1>

        <div className="vote-success__message">
          <p>Your feedback has been recorded successfully.</p>
          {activityName && voteValue && (
            <p className="vote-success__details">
              You rated "{activityName}" with {voteValue} star{voteValue !== 1 ? 's' : ''}.
            </p>
          )}
        </div>

        <div className="vote-success__actions">
          <VoteButton onClick={handleBackToSurvey} variant="primary">
            Back to Survey
          </VoteButton>
          {typeof navigator.share === 'function' && (
            <VoteButton onClick={handleShare} variant="outline">
              Share Survey
            </VoteButton>
          )}
        </div>
      </div>
    </div>
  );
};

export default VoteSuccess;
