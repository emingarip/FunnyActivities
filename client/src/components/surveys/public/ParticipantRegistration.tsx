import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import SurveyService from '../../../services/surveyService';
import VoteButton from '../shared/VoteButton';
import './ParticipantRegistration.css';

interface ParticipantRegistrationProps {
  onRegistrationSuccess?: (participantId: string) => void;
  onCancel?: () => void;
}

interface FormData {
  firstName: string;
  lastName: string;
  childrenCount: number;
}

interface FormErrors {
  firstName?: string;
  lastName?: string;
  childrenCount?: string;
  general?: string;
}

const ParticipantRegistration: React.FC<ParticipantRegistrationProps> = ({
  onRegistrationSuccess,
  onCancel,
}) => {
  const { surveyId } = useParams<{ surveyId: string }>();
  const navigate = useNavigate();

  const [formData, setFormData] = useState<FormData>({
    firstName: '',
    lastName: '',
    childrenCount: 0,
  });

  const [errors, setErrors] = useState<FormErrors>({});
  const [touched, setTouched] = useState<Record<string, boolean>>({});
  const [submitting, setSubmitting] = useState(false);

  const validateForm = (): boolean => {
    const newErrors: FormErrors = {};

    // First name validation
    if (!formData.firstName.trim()) {
      newErrors.firstName = 'First name is required';
    } else if (formData.firstName.trim().length < 2) {
      newErrors.firstName = 'First name must be at least 2 characters';
    } else if (formData.firstName.trim().length > 50) {
      newErrors.firstName = 'First name cannot exceed 50 characters';
    }

    // Last name validation
    if (!formData.lastName.trim()) {
      newErrors.lastName = 'Last name is required';
    } else if (formData.lastName.trim().length < 2) {
      newErrors.lastName = 'Last name must be at least 2 characters';
    } else if (formData.lastName.trim().length > 50) {
      newErrors.lastName = 'Last name cannot exceed 50 characters';
    }

    // Children count validation
    if (formData.childrenCount < 0) {
      newErrors.childrenCount = 'Children count cannot be negative';
    } else if (formData.childrenCount > 20) {
      newErrors.childrenCount = 'Children count cannot exceed 20';
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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      setTouched({ firstName: true, lastName: true, childrenCount: true });
      return;
    }

    if (!surveyId) {
      setErrors({ general: 'Survey ID is missing' });
      return;
    }

    setSubmitting(true);
    setErrors({});

    try {
      const response = await SurveyService.registerParticipant(surveyId, {
        firstName: formData.firstName.trim(),
        lastName: formData.lastName.trim(),
        childrenCount: formData.childrenCount,
      });

      if (response.success && response.data) {
        // Store participant ID in session storage for the voting flow
        sessionStorage.setItem(`survey_${surveyId}_participant`, response.data.id);

        if (onRegistrationSuccess) {
          onRegistrationSuccess(response.data.id);
        } else {
          // Navigate to survey voting
          navigate(`/surveys/${surveyId}`);
        }
      } else {
        throw new Error(response.message || 'Failed to register participant');
      }
    } catch (err) {
      console.error('Error registering participant:', err);
      setErrors({
        general: err instanceof Error ? err.message : 'Failed to register participant'
      });
    } finally {
      setSubmitting(false);
    }
  };

  const handleCancel = () => {
    if (onCancel) {
      onCancel();
    } else {
      navigate(-1);
    }
  };

  return (
    <div className="participant-registration">
      <div className="participant-registration__header">
        <button
          className="participant-registration__back-button"
          onClick={handleCancel}
          aria-label="Go back"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <polyline points="15,18 9,12 15,6"/>
          </svg>
          Back
        </button>
        <div className="participant-registration__header-content">
          <h1 className="participant-registration__title">Join the Survey</h1>
          <p className="participant-registration__subtitle">Please provide your information to participate</p>
        </div>
      </div>

      <form className="participant-registration__content" onSubmit={handleSubmit}>
        {errors.general && (
          <div className="participant-registration__alert participant-registration__alert--error" role="alert">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <circle cx="12" cy="12" r="10"/>
              <line x1="15" y1="9" x2="9" y2="15"/>
              <line x1="9" y1="9" x2="15" y2="15"/>
            </svg>
            {errors.general}
          </div>
        )}

        <div className="participant-registration__section">
          <label htmlFor="firstName" className="participant-registration__label">
            First Name *
          </label>
          <input
            id="firstName"
            type="text"
            className={`participant-registration__input ${errors.firstName && touched.firstName ? 'participant-registration__input--error' : ''}`}
            value={formData.firstName}
            onChange={(e) => handleInputChange('firstName', e.target.value)}
            placeholder="Enter your first name"
            aria-describedby={errors.firstName ? 'firstName-error' : undefined}
            aria-invalid={!!(errors.firstName && touched.firstName)}
          />
          {errors.firstName && touched.firstName && (
            <div id="firstName-error" className="participant-registration__error-message" role="alert">
              {errors.firstName}
            </div>
          )}
        </div>

        <div className="participant-registration__section">
          <label htmlFor="lastName" className="participant-registration__label">
            Last Name *
          </label>
          <input
            id="lastName"
            type="text"
            className={`participant-registration__input ${errors.lastName && touched.lastName ? 'participant-registration__input--error' : ''}`}
            value={formData.lastName}
            onChange={(e) => handleInputChange('lastName', e.target.value)}
            placeholder="Enter your last name"
            aria-describedby={errors.lastName ? 'lastName-error' : undefined}
            aria-invalid={!!(errors.lastName && touched.lastName)}
          />
          {errors.lastName && touched.lastName && (
            <div id="lastName-error" className="participant-registration__error-message" role="alert">
              {errors.lastName}
            </div>
          )}
        </div>

        <div className="participant-registration__section">
          <label htmlFor="childrenCount" className="participant-registration__label">
            Number of Children
          </label>
          <select
            id="childrenCount"
            className={`participant-registration__select ${errors.childrenCount && touched.childrenCount ? 'participant-registration__select--error' : ''}`}
            value={formData.childrenCount}
            onChange={(e) => handleInputChange('childrenCount', parseInt(e.target.value))}
            aria-describedby={errors.childrenCount ? 'childrenCount-error' : undefined}
            aria-invalid={!!(errors.childrenCount && touched.childrenCount)}
          >
            {Array.from({ length: 21 }, (_, i) => (
              <option key={i} value={i}>
                {i === 0 ? 'None' : i === 1 ? '1 child' : `${i} children`}
              </option>
            ))}
          </select>
          {errors.childrenCount && touched.childrenCount && (
            <div id="childrenCount-error" className="participant-registration__error-message" role="alert">
              {errors.childrenCount}
            </div>
          )}
        </div>

        <div className="participant-registration__actions">
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
            {submitting ? 'Registering...' : 'Join Survey'}
          </VoteButton>
        </div>
      </form>
    </div>
  );
};

export default ParticipantRegistration;
