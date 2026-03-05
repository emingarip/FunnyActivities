import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { SurveyActivity, VoteRequest } from '../../../types/survey.types';
import VoteService from '../../../services/voteService';
import VoteButton from '../shared/VoteButton';
import './VoteForm.css';

interface VoteFormProps {
  surveyId?: string;
  activityId?: string;
  onSubmit?: (voteData: VoteRequest) => Promise<void>;
  onCancel?: () => void;
}

interface FormData {
  voterName: string;
  voteValue: number;
}

interface FormErrors {
  voterName?: string;
  voteValue?: string;
  general?: string;
}

const VoteForm: React.FC<VoteFormProps> = ({
  surveyId: propSurveyId,
  activityId: propActivityId,
  onSubmit,
  onCancel,
}) => {
  const { surveyId: urlSurveyId, activityId: urlActivityId } = useParams<{
    surveyId: string;
    activityId: string;
  }>();
  const navigate = useNavigate();

  const surveyId = propSurveyId || urlSurveyId;
  const activityId = propActivityId || urlActivityId;

  const [activity, setActivity] = useState<SurveyActivity | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [formData, setFormData] = useState<FormData>({
    voterName: '',
    voteValue: 0,
  });

  const [errors, setErrors] = useState<FormErrors>({});
  const [touched, setTouched] = useState<Record<string, boolean>>({});

  useEffect(() => {
    if (surveyId && activityId) {
      loadActivity();
    }
  }, [surveyId, activityId]);

  const loadActivity = async () => {
    if (!surveyId || !activityId) return;

    try {
      setLoading(true);
      setError(null);

      // First get the survey by share token to get the survey ID
      const surveyResponse = await VoteService.getPublicSurvey(surveyId);
      if (!surveyResponse.success || !surveyResponse.data) {
        throw new Error('Failed to load survey');
      }

      const survey = surveyResponse.data;

      // Then get activities using the survey ID
      const activitiesResponse = await VoteService.getSurveyActivities(survey.id);
      if (activitiesResponse.success) {
        const foundActivity = activitiesResponse.data.find(a => a.id === activityId);
        if (foundActivity) {
          setActivity(foundActivity);
        } else {
          throw new Error('Activity not found');
        }
      } else {
        throw new Error('Failed to load activity');
      }
    } catch (err) {
      console.error('Error loading activity:', err);
      setError(err instanceof Error ? err.message : 'Failed to load activity');
    } finally {
      setLoading(false);
    }
  };

  const validateForm = (): boolean => {
    const newErrors: FormErrors = {};

    // Voter name validation
    if (!formData.voterName.trim()) {
      newErrors.voterName = 'Name is required';
    } else if (formData.voterName.trim().length < 2) {
      newErrors.voterName = 'Name must be at least 2 characters';
    } else if (formData.voterName.trim().length > 100) {
      newErrors.voterName = 'Name cannot exceed 100 characters';
    }

    // Vote value validation
    if (formData.voteValue === 0) {
      newErrors.voteValue = 'Please select a rating';
    } else if (formData.voteValue < 1 || formData.voteValue > 5) {
      newErrors.voteValue = 'Rating must be between 1 and 5';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleInputChange = (field: keyof FormData, value: string | number) => {
    setFormData(prev => ({ ...prev, [field]: value }));

    // Clear error when user starts typing
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: undefined }));
    }

    // Mark field as touched
    setTouched(prev => ({ ...prev, [field]: true }));
  };

  const handleRatingClick = (rating: number) => {
    setFormData(prev => ({ ...prev, voteValue: rating }));
    setTouched(prev => ({ ...prev, voteValue: true }));

    if (errors.voteValue) {
      setErrors(prev => ({ ...prev, voteValue: undefined }));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      setTouched({ voterName: true, voteValue: true });
      return;
    }

    if (!activityId) {
      setError('Activity ID is missing');
      return;
    }

    setSubmitting(true);
    setError(null);

    try {
      // Get participant ID from session storage
      const participantId = sessionStorage.getItem(`survey_${surveyId}_participant`);
      if (!participantId) {
        setError('Participant registration required');
        return;
      }

      const voteData: VoteRequest = {
        surveyActivityId: activityId,
        voteValue: formData.voteValue,
        surveyParticipantId: participantId,
      };

      if (onSubmit) {
        await onSubmit(voteData);
      } else {
        const response = await VoteService.submitVote(voteData);
        if (response.success) {
          console.log('Vote submitted successfully, navigating to success page');
          // Navigate to success page with vote details
          navigate(`/survey/${surveyId}/success`, {
            state: {
              activityName: activity?.activityName,
              voteValue: formData.voteValue
            }
          });
        } else {
          throw new Error(response.message || 'Failed to submit vote');
        }
      }
    } catch (err) {
      console.error('Error submitting vote:', err);
      setError(err instanceof Error ? err.message : 'Failed to submit vote');
    } finally {
      setSubmitting(false);
    }
  };

  const handleCancel = () => {
    if (onCancel) {
      onCancel();
    } else {
      navigate(`/survey/${surveyId}`);
    }
  };

  const getRatingLabel = (rating: number): string => {
    const labels = {
      1: 'Poor',
      2: 'Fair',
      3: 'Good',
      4: 'Very Good',
      5: 'Excellent'
    };
    return labels[rating as keyof typeof labels] || '';
  };

  const getRatingEmoji = (rating: number): string => {
    const emojis = {
      1: '😞',
      2: '😐',
      3: '😊',
      4: '😄',
      5: '🤩'
    };
    return emojis[rating as keyof typeof emojis] || '';
  };

  if (loading) {
    return (
      <div className="vote-form vote-form--loading">
        <div className="vote-form__loading">
          <div className="vote-form__spinner" aria-hidden="true">
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
          <p className="vote-form__loading-text">Loading voting form...</p>
        </div>
      </div>
    );
  }

  if (error && !activity) {
    return (
      <div className="vote-form vote-form--error">
        <div className="vote-form__error">
          <h2 className="vote-form__error-title">Unable to Load Form</h2>
          <p className="vote-form__error-message">{error}</p>
          <VoteButton onClick={handleCancel} variant="outline">
            Go Back
          </VoteButton>
        </div>
      </div>
    );
  }

  if (!activity) {
    return (
      <div className="vote-form vote-form--not-found">
        <div className="vote-form__error">
          <h2 className="vote-form__error-title">Activity Not Found</h2>
          <p className="vote-form__error-message">The activity you're trying to vote on doesn't exist.</p>
          <VoteButton onClick={handleCancel}>Go Back</VoteButton>
        </div>
      </div>
    );
  }

  return (
    <div className="vote-form">
      <div className="vote-form__header">
        <button
          className="vote-form__back-button"
          onClick={handleCancel}
          aria-label="Go back"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <polyline points="15,18 9,12 15,6"/>
          </svg>
          Back
        </button>
        <div className="vote-form__header-content">
          <h1 className="vote-form__title">Vote for Activity</h1>
          <p className="vote-form__subtitle">Share your feedback for "{activity.activityName}"</p>
        </div>
      </div>

      <form className="vote-form__content" onSubmit={handleSubmit}>
        {error && (
          <div className="vote-form__alert vote-form__alert--error" role="alert">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <circle cx="12" cy="12" r="10"/>
              <line x1="15" y1="9" x2="9" y2="15"/>
              <line x1="9" y1="9" x2="15" y2="15"/>
            </svg>
            {error}
          </div>
        )}

        <div className="vote-form__section">
          <label htmlFor="voterName" className="vote-form__label">
            Your Name *
          </label>
          <input
            id="voterName"
            type="text"
            className={`vote-form__input ${errors.voterName && touched.voterName ? 'vote-form__input--error' : ''}`}
            value={formData.voterName}
            onChange={(e) => handleInputChange('voterName', e.target.value)}
            placeholder="Enter your full name"
            aria-describedby={errors.voterName ? 'voterName-error' : undefined}
            aria-invalid={!!(errors.voterName && touched.voterName)}
          />
          {errors.voterName && touched.voterName && (
            <div id="voterName-error" className="vote-form__error-message" role="alert">
              {errors.voterName}
            </div>
          )}
        </div>

        <div className="vote-form__section">
          <label className="vote-form__label">Your Rating *</label>
          <div className="vote-form__rating">
            {[1, 2, 3, 4, 5].map((rating) => (
              <button
                key={rating}
                type="button"
                className={`vote-form__rating-button ${
                  formData.voteValue === rating ? 'vote-form__rating-button--selected' : ''
                }`}
                onClick={() => handleRatingClick(rating)}
                aria-label={`Rate ${rating} star${rating !== 1 ? 's' : ''}: ${getRatingLabel(rating)}`}
                aria-pressed={formData.voteValue === rating}
              >
                <span className="vote-form__rating-number">{rating}</span>
                <span className="vote-form__rating-emoji">{getRatingEmoji(rating)}</span>
                <span className="vote-form__rating-label">{getRatingLabel(rating)}</span>
              </button>
            ))}
          </div>
          {errors.voteValue && touched.voteValue && (
            <div className="vote-form__error-message" role="alert">
              {errors.voteValue}
            </div>
          )}
        </div>


        <div className="vote-form__actions">
          <VoteButton
            onClick={handleCancel}
            variant="outline"
            disabled={submitting}
          >
            Cancel
          </VoteButton>
          <VoteButton
            type="submit"
            variant="primary"
            loading={submitting}
            disabled={submitting}
          >
            {submitting ? 'Submitting...' : 'Submit Vote'}
          </VoteButton>
        </div>
      </form>
    </div>
  );
};

export default VoteForm;
