import React from 'react';
import { Survey } from '../../../types/survey.types';
import VoteService from '../../../services/voteService';
import SurveyService from '../../../services/surveyService';
import './SurveyCard.css';

export interface SurveyCardProps {
  survey: Survey;
  onVoteClick?: (surveyId: string) => void;
  onViewResults?: (surveyId: string) => void;
  showActions?: boolean;
  className?: string;
  compact?: boolean;
}

const SurveyCard: React.FC<SurveyCardProps> = ({
  survey,
  onVoteClick,
  onViewResults,
  showActions = true,
  className = '',
  compact = false,
}) => {
  const surveyStatus = SurveyService.getSurveyStatusText(survey);
  const completionPercentage = VoteService.getParticipationRate(survey);
  const timeUntilEnd = VoteService.getTimeUntilEnd(survey);
  const isActive = VoteService.canUserVote(survey).canVote;

  const handleVoteClick = () => {
    if (onVoteClick && isActive) {
      onVoteClick(survey.id);
    }
  };

  const handleViewResults = () => {
    if (onViewResults) {
      onViewResults(survey.id);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent, action: () => void) => {
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      action();
    }
  };

  return (
    <article
      className={`survey-card ${compact ? 'survey-card--compact' : ''} ${className}`}
      role="article"
      aria-labelledby={`survey-title-${survey.id}`}
    >
      <div className="survey-card__header">
        <div className="survey-card__title-section">
          <h3
            id={`survey-title-${survey.id}`}
            className="survey-card__title"
          >
            {survey.title}
          </h3>
          <div className="survey-card__status">
            <span
              className={`survey-card__status-badge survey-card__status-badge--${surveyStatus.toLowerCase()}`}
              aria-label={`Survey status: ${surveyStatus}`}
            >
              {surveyStatus}
            </span>
          </div>
        </div>

        {!compact && survey.description && (
          <p className="survey-card__description">{survey.description}</p>
        )}
      </div>

      <div className="survey-card__content">
        <div className="survey-card__stats">
          <div className="survey-card__stat">
            <span className="survey-card__stat-label">Participants</span>
            <span className="survey-card__stat-value">
              {survey.participantCount}
              {survey.maxParticipants && ` / ${survey.maxParticipants}`}
            </span>
          </div>

          {completionPercentage > 0 && (
            <div className="survey-card__stat">
              <span className="survey-card__stat-label">Completion</span>
              <span className="survey-card__stat-value">{completionPercentage}%</span>
            </div>
          )}

          <div className="survey-card__stat">
            <span className="survey-card__stat-label">Activities</span>
            <span className="survey-card__stat-value">{survey.activities.length}</span>
          </div>
        </div>

        {!compact && timeUntilEnd && (
          <div className="survey-card__countdown">
            <span className="survey-card__countdown-label">Time remaining:</span>
            <span className="survey-card__countdown-value">
              {VoteService.formatCountdown(timeUntilEnd)}
            </span>
          </div>
        )}

        {showActions && (
          <div className="survey-card__actions">
            {isActive && onVoteClick && (
              <button
                className="survey-card__action-button survey-card__action-button--primary"
                onClick={handleVoteClick}
                onKeyDown={(e) => handleKeyDown(e, handleVoteClick)}
                aria-label={`Vote in survey: ${survey.title}`}
              >
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
                </svg>
                Vote Now
              </button>
            )}

            {onViewResults && (
              <button
                className="survey-card__action-button survey-card__action-button--secondary"
                onClick={handleViewResults}
                onKeyDown={(e) => handleKeyDown(e, handleViewResults)}
                aria-label={`View results for survey: ${survey.title}`}
              >
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
                </svg>
                View Results
              </button>
            )}
          </div>
        )}
      </div>
    </article>
  );
};

export default SurveyCard;