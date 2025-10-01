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
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Snackbar,
  IconButton,
} from '@mui/material';
import {
  ArrowBack,
  Save,
  Cancel,
  Warning,
  Refresh,
  Share,
  ContentCopy,
} from '@mui/icons-material';
import { useNavigate, useParams } from 'react-router-dom';
import { useMutation, useQuery } from '@tanstack/react-query';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import AdminRoute from '../../../components/AdminRoute';
import SurveyService from '../../../services/surveyService';
import { Survey, UpdateSurveyRequest, SurveyFormData, SurveyFormErrors } from '../../../types/survey.types';

const steps = ['Basic Information', 'Activities', 'Settings'];

interface Activity {
  id: string;
  name: string;
  description: string;
}

const SurveyEdit: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [activeStep, setActiveStep] = useState(0);
  const [formData, setFormData] = useState<SurveyFormData>({
    title: '',
    description: '',
    startDate: '',
    endDate: '',
    maxParticipants: undefined,
    activityIds: [],
  });
  const [originalData, setOriginalData] = useState<SurveyFormData | null>(null);
  const [errors, setErrors] = useState<SurveyFormErrors>({});
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);
  const [showUnsavedDialog, setShowUnsavedDialog] = useState(false);
  const [pendingNavigation, setPendingNavigation] = useState<string | null>(null);
  const [shareDialogOpen, setShareDialogOpen] = useState(false);
  const [shareUrl, setShareUrl] = useState('');
  const [copySuccess, setCopySuccess] = useState(false);

  // Fetch survey data
  const {
    data: surveyResponse,
    isLoading: surveyLoading,
    error: surveyError,
  } = useQuery({
    queryKey: ['survey', id],
    queryFn: () => SurveyService.getSurvey(id!),
    enabled: !!id,
  });

  const survey: Survey | undefined = surveyResponse?.data;

  // Fetch available activities
  const { data: activitiesResponse, isLoading: activitiesLoading } = useQuery({
    queryKey: ['activities'],
    queryFn: () => fetch('/api/activities').then(res => res.json()),
  });

  const activities: Activity[] = activitiesResponse?.data || [];

  // Update survey mutation
  const updateMutation = useMutation({
    mutationFn: (data: UpdateSurveyRequest) => SurveyService.updateSurvey(id!, data),
    onSuccess: () => {
      setHasUnsavedChanges(false);
      navigate('/admin/surveys');
    },
    onError: (error: any) => {
      setErrors({ general: error.message || 'Failed to update survey' });
    },
  });

  // Initialize form data when survey loads
  useEffect(() => {
    if (survey) {
      const initialData: SurveyFormData = {
        title: survey.title,
        description: survey.description,
        startDate: survey.startDate.split('T')[0], // Convert to date string
        endDate: survey.endDate ? survey.endDate.split('T')[0] : '',
        maxParticipants: survey.maxParticipants,
        activityIds: survey.activities.map(a => a.activityId),
      };
      setFormData(initialData);
      setOriginalData(initialData);
    }
  }, [survey]);

  // Track changes
  useEffect(() => {
    if (originalData) {
      const hasChanges = JSON.stringify(formData) !== JSON.stringify(originalData);
      setHasUnsavedChanges(hasChanges);
    }
  }, [formData, originalData]);

  // Handle browser back/forward
  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (hasUnsavedChanges) {
        e.preventDefault();
        e.returnValue = '';
      }
    };

    window.addEventListener('beforeunload', handleBeforeUnload);
    return () => window.removeEventListener('beforeunload', handleBeforeUnload);
  }, [hasUnsavedChanges]);

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

        if (!formData.startDate) {
          newErrors.startDate = 'Start date is required';
        } else {
          const startDate = new Date(formData.startDate);
          const now = new Date();
          if (startDate <= now) {
            newErrors.startDate = 'Start date must be in the future';
          }
        }

        if (formData.endDate) {
          const startDate = new Date(formData.startDate);
          const endDate = new Date(formData.endDate);
          if (endDate <= startDate) {
            newErrors.endDate = 'End date must be after start date';
          }
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
      const updateData: UpdateSurveyRequest = {
        title: formData.title.trim(),
        description: formData.description.trim() || undefined,
        startDate: formData.startDate,
        endDate: formData.endDate || undefined,
        maxParticipants: formData.maxParticipants,
        activityIds: formData.activityIds,
      };

      updateMutation.mutate(updateData);
    }
  };

  const handleCancel = () => {
    if (hasUnsavedChanges) {
      setShowUnsavedDialog(true);
      setPendingNavigation('/admin/surveys');
    } else {
      navigate('/admin/surveys');
    }
  };

  const handleConfirmNavigation = () => {
    setShowUnsavedDialog(false);
    setHasUnsavedChanges(false);
    navigate(pendingNavigation || '/admin/surveys');
  };

  const handleDiscardChanges = () => {
    setShowUnsavedDialog(false);
    if (originalData) {
      setFormData(originalData);
    }
    setHasUnsavedChanges(false);
  };

  const handleShareSurvey = async () => {
    if (!id) return;

    try {
      const response = await SurveyService.getShareUrl(id);
      const shareUrl = response.data.shareUrl;
      setShareUrl(shareUrl);
      setShareDialogOpen(true);
    } catch (error) {
      console.error('Error getting share URL:', error);
    }
  };

  const handleCopyToClipboard = async () => {
    try {
      await navigator.clipboard.writeText(shareUrl);
      setCopySuccess(true);
      setTimeout(() => setCopySuccess(false), 2000);
    } catch (error) {
      console.error('Error copying to clipboard:', error);
    }
  };

  const handleCloseShareDialog = () => {
    setShareDialogOpen(false);
    setShareUrl('');
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

            <LocalizationProvider dateAdapter={AdapterDateFns}>
              <DatePicker
                label="Start Date"
                value={new Date(formData.startDate)}
                onChange={(date) => handleInputChange('startDate', date?.toISOString().split('T')[0])}
                slotProps={{
                  textField: {
                    fullWidth: true,
                    error: !!errors.startDate,
                    helperText: errors.startDate,
                    required: true,
                  },
                }}
                disablePast
              />

              <DatePicker
                label="End Date (Optional)"
                value={formData.endDate ? new Date(formData.endDate) : null}
                onChange={(date) => handleInputChange('endDate', date?.toISOString().split('T')[0] || '')}
                slotProps={{
                  textField: {
                    fullWidth: true,
                    error: !!errors.endDate,
                    helperText: errors.endDate,
                  },
                }}
                minDate={new Date(formData.startDate)}
              />
            </LocalizationProvider>
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
                  {activities.map((activity) => (
                    <MenuItem key={activity.id} value={activity.id}>
                      <Box>
                        <Typography variant="subtitle2">{activity.name}</Typography>
                        <Typography variant="caption" color="text.secondary">
                          {activity.description}
                        </Typography>
                      </Box>
                    </MenuItem>
                  ))}
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
                • Start Date: {new Date(formData.startDate).toLocaleDateString()}<br />
                {formData.endDate && `• End Date: ${new Date(formData.endDate).toLocaleDateString()}<br />`}
                {formData.maxParticipants && `• Max Participants: ${formData.maxParticipants}<br />`}
              </Typography>
            </Alert>
          </Box>
        );

      default:
        return null;
    }
  };

  if (surveyLoading) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Box display="flex" justifyContent="center" py={8}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (surveyError || !survey) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="error">
          Error loading survey. Please try again.
        </Alert>
      </Container>
    );
  }

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
            Edit Survey: {survey.title}
          </Typography>
          <Button
            variant="outlined"
            startIcon={<Share />}
            onClick={handleShareSurvey}
            sx={{ ml: 'auto', mr: 2 }}
            color="success"
          >
            Share Survey
          </Button>
        {hasUnsavedChanges && (
          <Chip
            label="Unsaved Changes"
            color="warning"
            size="small"
            icon={<Warning />}
            sx={{ ml: 2 }}
          />
        )}
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
                disabled={updateMutation.isPending}
                startIcon={updateMutation.isPending ? <CircularProgress size={20} /> : <Save />}
              >
                {updateMutation.isPending ? 'Saving...' : 'Save Changes'}
              </Button>
            ) : (
              <Button
                variant="contained"
                onClick={handleNext}
                endIcon={<ArrowBack sx={{ transform: 'rotate(180deg)' }} />}
              >
                Next
              </Button>
            )}
          </Box>
        </Box>
      </Paper>

      {/* Unsaved Changes Dialog */}
      <Dialog open={showUnsavedDialog} onClose={() => setShowUnsavedDialog(false)}>
        <DialogTitle>Unsaved Changes</DialogTitle>
        <DialogContent>
          <Typography>
            You have unsaved changes. What would you like to do?
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleDiscardChanges}>Discard Changes</Button>
          <Button onClick={() => setShowUnsavedDialog(false)} autoFocus>
            Keep Editing
          </Button>
          <Button onClick={handleConfirmNavigation} variant="contained">
            Leave Anyway
          </Button>
        </DialogActions>
      </Dialog>

      {/* Share Dialog */}
      <Dialog open={shareDialogOpen} onClose={handleCloseShareDialog} maxWidth="sm" fullWidth>
        <DialogTitle>
          Share Survey: {survey?.title}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Share this link with participants to allow them to vote on the survey:
            </Typography>
            <TextField
              fullWidth
              value={shareUrl}
              InputProps={{
                readOnly: true,
                endAdornment: (
                  <IconButton onClick={handleCopyToClipboard} edge="end">
                    <ContentCopy />
                  </IconButton>
                ),
              }}
              variant="outlined"
              sx={{ mt: 1 }}
            />
            <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
              Share Token: {survey?.shareToken}
            </Typography>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseShareDialog}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Copy Success Snackbar */}
      <Snackbar
        open={copySuccess}
        autoHideDuration={2000}
        onClose={() => setCopySuccess(false)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert severity="success" sx={{ width: '100%' }}>
          Link copied to clipboard!
        </Alert>
      </Snackbar>
    </Container>
    </AdminRoute>
  );
};

export default SurveyEdit;