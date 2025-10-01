import React, { useState, useEffect } from 'react';
import {
  Container,
  Paper,
  Typography,
  TextField,
  Button,
  Box,
  Stepper,
  Step,
  StepLabel,
  Alert,
  CircularProgress,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Chip,
  OutlinedInput,
  FormHelperText,
} from '@mui/material';
import {
  ArrowBack,
  ArrowForward,
  Save,
  Cancel,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQuery } from '@tanstack/react-query';
import AdminRoute from '../../../components/AdminRoute';
import SurveyService from '../../../services/surveyService';
import { activitiesAPI } from '../../../services/api';
import { CreateSurveyRequest, SurveyFormData, SurveyFormErrors } from '../../../types/survey.types';

const steps = ['Basic Information', 'Activities', 'Settings'];

interface Activity {
  id: string;
  name: string;
  description?: string;
  videoUrl?: string;
  duration?: string;
  activityCategoryId: string;
  activityCategoryName: string;
  createdAt: string;
  updatedAt: string;
  stepCount: number;
  productVariantCount: number;
}

const SurveyCreate: React.FC = () => {
  const navigate = useNavigate();
  const [activeStep, setActiveStep] = useState(0);
  const [formData, setFormData] = useState<SurveyFormData>({
    title: '',
    description: '',
    startDate: '',
    endDate: '',
    maxParticipants: undefined,
    activityIds: [],
  });
  const [errors, setErrors] = useState<SurveyFormErrors>({});

  // Fetch available activities
  const { data: activitiesResponse, isLoading: activitiesLoading, error: activitiesError } = useQuery({
    queryKey: ['activities'],
    queryFn: async () => {
      const response = await activitiesAPI.getPublicActivities();
      return response.data;
    },
  });

  const activities: Activity[] = activitiesResponse?.data?.items || [];
  console.log('✅ Activities loaded:', activities.length, 'items');

  // Debug: Monitor activities changes
  useEffect(() => {
    console.log('🔄 ACTIVITIES CHANGED:', activities);
    console.log('🔄 ACTIVITIES LENGTH:', activities.length);
    if (activities.length > 0) {
      console.log('🔄 FIRST ACTIVITY:', activities[0]);
      console.log('🔄 FIRST ACTIVITY KEYS:', Object.keys(activities[0]));
    }
  }, [activities]);

  // Create survey mutation
  const createMutation = useMutation({
    mutationFn: (data: CreateSurveyRequest) => SurveyService.createSurvey(data),
    onSuccess: () => {
      navigate('/admin/surveys');
    },
    onError: (error: any) => {
      setErrors({ general: error.message || 'Failed to create survey' });
    },
  });

  const handleInputChange = (field: keyof SurveyFormData, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // Clear error when user starts typing
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: undefined }));
    }
  };

  const validateStep = (step: number): boolean => {
    const newErrors: SurveyFormErrors = {};

    switch (step) {
      case 0: // Basic Information
        if (!formData.title.trim()) {
          newErrors.title = 'Survey title is required';
        } else if (formData.title.length > 200) {
          newErrors.title = 'Title cannot exceed 200 characters';
        }

        if (formData.description && formData.description.length > 1000) {
          newErrors.description = 'Description cannot exceed 1000 characters';
        }
        break;

      case 1: // Activities
        if (!formData.activityIds || formData.activityIds.length === 0) {
          newErrors.activityIds = 'At least one activity is required';
        }
        break;

      case 2: // Settings
        if (formData.maxParticipants !== undefined) {
          if (formData.maxParticipants < 1) {
            newErrors.maxParticipants = 'Maximum participants must be greater than 0';
          } else if (formData.maxParticipants > 10000) {
            newErrors.maxParticipants = 'Maximum participants cannot exceed 10,000';
          }
        }
        break;
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleNext = () => {
    if (validateStep(activeStep)) {
      setActiveStep((prev) => prev + 1);
    }
  };

  const handleBack = () => {
    setActiveStep((prev) => prev - 1);
  };

  const handleSave = () => {
    if (validateStep(activeStep)) {
      const surveyData: CreateSurveyRequest = {
        title: formData.title.trim(),
        description: formData.description.trim() || undefined,
        startDate: new Date().toISOString().split('T')[0],
        endDate: undefined,
        maxParticipants: formData.maxParticipants,
        activityIds: formData.activityIds,
      };

      createMutation.mutate(surveyData);
    }
  };

  const handleCancel = () => {
    navigate('/admin/surveys');
  };

  const renderStepContent = (step: number) => {
    switch (step) {
      case 0:
        return (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
            <TextField
              label="Survey Title"
              value={formData.title}
              onChange={(e) => handleInputChange('title', e.target.value)}
              error={!!errors.title}
              helperText={errors.title}
              fullWidth
              required
            />

            <TextField
              label="Description (Optional)"
              value={formData.description}
              onChange={(e) => handleInputChange('description', e.target.value)}
              error={!!errors.description}
              helperText={errors.description || `${formData.description.length}/1000 characters`}
              fullWidth
              multiline
              rows={4}
            />
          </Box>
        );

      case 1:
        return (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
            <Typography variant="h6" gutterBottom>
              Select Activities
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Choose the activities that participants will vote on in this survey.
            </Typography>

            {activitiesLoading ? (
              <Box display="flex" justifyContent="center" py={4}>
                <CircularProgress />
              </Box>
            ) : activitiesError ? (
              <Alert severity="error" sx={{ mb: 2 }}>
                Failed to load activities. Please try refreshing the page or contact support if the problem persists.
              </Alert>
            ) : (
              <FormControl fullWidth error={!!errors.activityIds}>
                <InputLabel>Select Activities</InputLabel>
                <Select
                  multiple
                  value={formData.activityIds}
                  onChange={(e) => handleInputChange('activityIds', e.target.value)}
                  input={<OutlinedInput label="Select Activities" />}
                  renderValue={(selected) => (
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                      {selected.map((value) => {
                        const activity = activities.find(a => a.id === value);
                        return (
                          <Chip
                            key={value}
                            label={activity?.name || value}
                            size="small"
                          />
                        );
                      })}
                    </Box>
                  )}
                >
                  {(() => {
                    console.log('🎯 RENDERING ACTIVITIES:', activities);
                    console.log('🎯 ACTIVITIES LENGTH:', activities.length);
                    if (activities.length === 0) {
                      console.log('🎯 NO ACTIVITIES TO RENDER');
                    }
                    return activities.map((activity, index) => {
                      console.log(`🎯 RENDERING ACTIVITY ${index}:`, activity);
                      return (
                        <MenuItem key={activity.id} value={activity.id}>
                          <Box>
                            <Typography variant="subtitle2">{activity.name}</Typography>
                            <Typography variant="caption" color="text.secondary">
                              {activity.description}
                            </Typography>
                          </Box>
                        </MenuItem>
                      );
                    });
                  })()}
                </Select>
                {errors.activityIds && (
                  <FormHelperText>{errors.activityIds}</FormHelperText>
                )}
              </FormControl>
            )}

            {formData.activityIds.length > 0 && (
              <Box>
                <Typography variant="subtitle2" gutterBottom>
                  Selected Activities ({formData.activityIds.length})
                </Typography>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                  {formData.activityIds.map((activityId) => {
                    const activity = activities.find(a => a.id === activityId);
                    return (
                      <Chip
                        key={activityId}
                        label={activity?.name || activityId}
                        onDelete={() => {
                          const newIds = formData.activityIds.filter(id => id !== activityId);
                          handleInputChange('activityIds', newIds);
                        }}
                        color="primary"
                        variant="outlined"
                      />
                    );
                  })}
                </Box>
              </Box>
            )}
          </Box>
        );

      case 2:
        return (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
            <Typography variant="h6" gutterBottom>
              Survey Settings
            </Typography>

            <TextField
              label="Maximum Participants (Optional)"
              type="number"
              value={formData.maxParticipants || ''}
              onChange={(e) => handleInputChange('maxParticipants', e.target.value ? parseInt(e.target.value) : undefined)}
              error={!!errors.maxParticipants}
              helperText={errors.maxParticipants || 'Leave empty for unlimited participants'}
              fullWidth
              InputProps={{
                inputProps: { min: 1, max: 10000 }
              }}
            />

            <Alert severity="info">
              <Typography variant="body2">
                <strong>Summary:</strong><br />
                • Title: {formData.title}<br />
                • Activities: {formData.activityIds.length} selected<br />
                {formData.maxParticipants && `• Max Participants: ${formData.maxParticipants}<br />`}
              </Typography>
            </Alert>
          </Box>
        );

      default:
        return null;
    }
  };

  return (
    <AdminRoute>
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Box display="flex" alignItems="center" mb={3}>
          <Button
            startIcon={<ArrowBack />}
            onClick={handleCancel}
            sx={{ mr: 2 }}
          >
            Back to Surveys
          </Button>
          <Typography variant="h4" component="h1" fontWeight="bold">
            Create New Survey
          </Typography>
        </Box>

      <Paper sx={{ p: 4 }}>
        <Stepper activeStep={activeStep} sx={{ mb: 4 }}>
          {steps.map((label) => (
            <Step key={label}>
              <StepLabel>{label}</StepLabel>
            </Step>
          ))}
        </Stepper>

        {errors.general && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {errors.general}
          </Alert>
        )}

        {renderStepContent(activeStep)}

        <Box display="flex" justifyContent="space-between" mt={4}>
          <Button
            disabled={activeStep === 0}
            onClick={handleBack}
            startIcon={<ArrowBack />}
          >
            Back
          </Button>

          <Box display="flex" gap={2}>
            <Button
              variant="outlined"
              onClick={handleCancel}
              startIcon={<Cancel />}
            >
              Cancel
            </Button>

            {activeStep === steps.length - 1 ? (
              <Button
                variant="contained"
                onClick={handleSave}
                disabled={createMutation.isPending}
                startIcon={createMutation.isPending ? <CircularProgress size={20} /> : <Save />}
              >
                {createMutation.isPending ? 'Creating...' : 'Create Survey'}
              </Button>
            ) : (
              <Button
                variant="contained"
                onClick={handleNext}
                endIcon={<ArrowForward />}
              >
                Next
              </Button>
            )}
          </Box>
        </Box>
      </Paper>
    </Container>
    </AdminRoute>
  );
};

export default SurveyCreate;