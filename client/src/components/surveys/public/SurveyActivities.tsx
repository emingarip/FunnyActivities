import React, { useState, useEffect } from 'react';
import { useSwipeable } from 'react-swipeable';
import { Survey, SurveyActivity, Vote } from '../../../types/survey.types';
import VoteService from '../../../services/voteService';
import VoteButton from '../shared/VoteButton';
import VideoPreview from '../../activities/VideoPreview';
import './SurveyActivities.css';

export interface SurveyActivitiesProps {
  activities: SurveyActivity[];
  survey: Survey;
  onVoteClick: (activity: SurveyActivity, voteValue: number) => void;
  canVote: boolean;
  participantId?: string | null;
  className?: string;
}

const SurveyActivities: React.FC<SurveyActivitiesProps> = ({
  activities,
  survey,
  onVoteClick,
  canVote,
  participantId,
  className = '',
}) => {
  const [currentActivityIndex, setCurrentActivityIndex] = useState(0);
  const [votedActivities, setVotedActivities] = useState<Set<string>>(new Set());
  const [participantVotes, setParticipantVotes] = useState<Vote[]>([]);
  const [loadingVotes, setLoadingVotes] = useState(false);

  // Debug logging for activities data structure
  console.log('📊 SurveyActivities: Received activities:', {
    activitiesCount: activities?.length,
    activitiesType: typeof activities,
    activitiesIsArray: Array.isArray(activities),
    firstActivity: activities?.[0],
    firstActivityVideoUrl: activities?.[0]?.videoUrl,
    firstActivityVideoUrlType: typeof activities?.[0]?.videoUrl,
    firstActivityVideoUrlLength: activities?.[0]?.videoUrl?.length,
    firstActivityActivityId: activities?.[0]?.activityId,
    firstActivityActivityIdType: typeof activities?.[0]?.activityId,
    surveyId: survey?.id,
    surveyTitle: survey?.title
  });

  // Filter activities to only show unvoted ones
  const unvotedActivities = activities.filter(activity => !votedActivities.has(activity.id));
  const currentActivity = unvotedActivities[currentActivityIndex];
  const hasVoted = votedActivities.has(currentActivity?.id || '');

  console.log('📊 SurveyActivities: Current activity data:', {
    currentActivityIndex,
    currentActivity,
    currentActivityVideoUrl: currentActivity?.videoUrl,
    currentActivityVideoUrlType: typeof currentActivity?.videoUrl,
    currentActivityVideoUrlLength: currentActivity?.videoUrl?.length,
    currentActivityActivityId: currentActivity?.activityId,
    currentActivityActivityIdType: typeof currentActivity?.activityId,
    hasVoted
  });

  useEffect(() => {
    // Reset to first activity when activities change
    setCurrentActivityIndex(0);
    setVotedActivities(new Set());
  }, [activities]);

  useEffect(() => {
    // Load participant's existing votes when participantId is available
    const loadParticipantVotes = async () => {
      if (participantId) {
        setLoadingVotes(true);
        try {
          const response = await VoteService.getParticipantVotes(participantId);
          if (response.success && response.data) {
            setParticipantVotes(response.data);
            // Mark activities as voted based on existing votes
            const votedActivityIds = new Set(response.data.map(vote => vote.surveyActivityId));
            setVotedActivities(votedActivityIds);
          }
        } catch (error) {
          console.error('Error loading participant votes:', error);
        } finally {
          setLoadingVotes(false);
        }
      }
    };

    loadParticipantVotes();
  }, [participantId]);

  const handleVoteClick = async (activity: SurveyActivity, voteValue: number) => {
    if (canVote && !hasVoted && participantId) {
      try {
        // Submit the vote
        await VoteService.submitVote({
          surveyActivityId: activity.id,
          voteValue: voteValue,
          surveyParticipantId: participantId,
        });

        // Mark as voted
        const newVotedActivities = new Set(votedActivities).add(activity.id);
        setVotedActivities(newVotedActivities);

        // Adjust index if out of bounds after removing voted activity
        const newUnvotedLength = activities.length - newVotedActivities.size;
        if (currentActivityIndex >= newUnvotedLength) {
          setCurrentActivityIndex(Math.max(0, newUnvotedLength - 1));
        }

        // Call the parent callback
        onVoteClick(activity, voteValue);
      } catch (error) {
        console.error('Error submitting vote:', error);
        // Could show an error message here
      }
    }
  };

  const handleNext = () => {
    if (currentActivityIndex < unvotedActivities.length - 1) {
      setCurrentActivityIndex(prev => prev + 1);
    }
  };

  const handlePrevious = () => {
    if (currentActivityIndex > 0) {
      setCurrentActivityIndex(prev => prev - 1);
    }
  };

  // Swipe handlers for mobile navigation
  const swipeHandlers = useSwipeable({
    onSwipedLeft: () => {
      // Swipe left to go to next activity
      handleNext();
    },
    onSwipedRight: () => {
      // Swipe right to go to previous activity
      handlePrevious();
    },
    preventScrollOnSwipe: true,
    trackMouse: false, // Only track touch events on mobile
  });


  if (activities.length === 0) {
    return (
      <div className={`survey-activities survey-activities--empty ${className}`}>
        <div className="survey-activities__empty">
          <div className="survey-activities__empty-icon" aria-hidden="true">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <circle cx="12" cy="12" r="3"/>
              <path d="M12 1v6m0 6v6m11-7h-6m-6 0H1"/>
            </svg>
          </div>
          <h3 className="survey-activities__empty-title">No Activities Available</h3>
          <p className="survey-activities__empty-message">
            This survey doesn't have any activities to vote on yet.
          </p>
        </div>
      </div>
    );
  }

  if (loadingVotes) {
    return (
      <div className={`survey-activities survey-activities--loading ${className}`}>
        <div className="survey-activities__loading">
          <div className="survey-activities__spinner" aria-hidden="true">
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
          <p className="survey-activities__loading-text">Loading your voting progress...</p>
        </div>
      </div>
    );
  }

  if (unvotedActivities.length === 0) {
    return (
      <div className={`survey-activities survey-activities--completed ${className}`}>
        <div className="survey-activities__completed">
          <div className="survey-activities__completed-icon" aria-hidden="true">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <polyline points="20,6 9,17 4,12"/>
            </svg>
          </div>
          <h3 className="survey-activities__completed-title">All Activities Completed!</h3>
          <p className="survey-activities__completed-message">
            You've voted on all available activities in this survey. Thank you for your participation!
          </p>
        </div>
      </div>
    );
  }

  return (
    <div
      className={`survey-activities survey-activities--single ${className}`}
      role="region"
      aria-label="Survey activity"
      {...swipeHandlers}
    >
      <div className="survey-activities__header">
        <h2 className="survey-activities__title">Activity {currentActivityIndex + 1} of {unvotedActivities.length}</h2>
        <div className="survey-activities__progress">
          <div className="survey-activities__progress-bar">
            <div
              className="survey-activities__progress-fill"
              style={{ width: `${((currentActivityIndex + 1) / unvotedActivities.length) * 100}%` }}
            />
          </div>
        </div>
      </div>

      <div className="survey-activities__content">
        <article
          className="survey-activity survey-activity--single"
          role="article"
          aria-labelledby={`activity-title-${currentActivity.id}`}
        >
          <div className="survey-activity__header">
            <div className="survey-activity__title-section">
              <h3
                id={`activity-title-${currentActivity.id}`}
                className="survey-activity__title"
              >
                {currentActivity.activityName}
              </h3>
            </div>
          </div>

          {currentActivity.videoUrl && (
            <div className="survey-activity__video">
              <VideoPreview
                src={currentActivity.videoUrl}
                activityId={currentActivity.activityId}
                autoPlay={true}
                muted={true}
                loop={true}
                aspectRatio="16/9"
                onPlay={() => console.log(`Playing video for survey activity: ${currentActivity.activityName}`)}
                onPause={() => console.log(`Paused video for survey activity: ${currentActivity.activityName}`)}
                onError={(error) => {
                  console.error(`Video error for survey activity ${currentActivity.activityName}:`, error);
                }}
              />
            </div>
          )}

          {currentActivity.activityDescription && (
            <div className="survey-activity__description">
              <p>{currentActivity.activityDescription}</p>
            </div>
          )}

          {currentActivity.durationMinutes && (
            <div className="survey-activity__duration">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <circle cx="12" cy="12" r="10"/>
                <polyline points="12,6 12,12 16,14"/>
              </svg>
              {currentActivity.durationMinutes} minutes
            </div>
          )}

          <div className="survey-activity__voting">
            <h4 className="survey-activity__voting-title">Rate this activity:</h4>
            <div className="survey-activity__rating-buttons">
              {[1, 2, 3, 4, 5].map((rating) => (
                <button
                  key={rating}
                  className={`survey-activity__rating-button ${hasVoted ? 'survey-activity__rating-button--voted' : ''}`}
                  onClick={() => handleVoteClick(currentActivity, rating)}
                  disabled={!canVote || hasVoted}
                  aria-label={`Rate ${rating} star${rating !== 1 ? 's' : ''}`}
                >
                  <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor" stroke="currentColor" strokeWidth="1">
                    <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
                  </svg>
                  {rating}
                </button>
              ))}
            </div>
            {hasVoted && (
              <div className="survey-activity__voted-message">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                  <polyline points="20,6 9,17 4,12"/>
                </svg>
                Vote recorded!
              </div>
            )}
          </div>
        </article>
      </div>

      <div className="survey-activities__navigation">
        <button
          className="survey-activities__nav-button survey-activities__nav-button--previous"
          onClick={handlePrevious}
          disabled={currentActivityIndex === 0}
          aria-label="Previous activity"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <polyline points="15,18 9,12 15,6"/>
          </svg>
          Previous
        </button>

        <div className="survey-activities__nav-info">
          {currentActivityIndex + 1} / {unvotedActivities.length}
        </div>

        <button
          className="survey-activities__nav-button survey-activities__nav-button--next"
          onClick={handleNext}
          disabled={currentActivityIndex === unvotedActivities.length - 1}
          aria-label="Next activity"
        >
          Next
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <polyline points="9,18 15,12 9,6"/>
          </svg>
        </button>
      </div>
    </div>
  );
};

export default SurveyActivities;
