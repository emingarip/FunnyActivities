import React, { useState } from 'react';
import {
  Box,
  Typography,
  Switch,
  FormControlLabel,
  Button,
  Alert,
  CircularProgress,
  Card,
  CardContent,
  TextField,
} from '@mui/material';
import { activitiesAPI } from '../../services/api';

interface ActivityPublicToggleProps {
  activityId?: string;
  onUpdate?: () => void;
}

const ActivityPublicToggle: React.FC<ActivityPublicToggleProps> = ({
  activityId: initialActivityId,
  onUpdate
}) => {
  const [activityId, setActivityId] = useState(initialActivityId || '');
  const [isPublic, setIsPublic] = useState(false);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const handleTogglePublic = async () => {
    if (!activityId.trim()) {
      setMessage({ type: 'error', text: 'Please enter an activity ID' });
      return;
    }

    setLoading(true);
    setMessage(null);

    try {
      // Get current activity data (admin component, so use authenticated endpoint)
      const response = await activitiesAPI.getActivity(activityId);

      if (response.data.success) {
        const activity = response.data.data;

        // Update activity with new public status
        const updateResponse = await activitiesAPI.updateActivity(activityId, {
          name: activity.name,
          description: activity.description,
          videoUrl: activity.videoUrl,
          durationHours: activity.duration ? parseInt(activity.duration.split(':')[0]) : 0,
          durationMinutes: activity.duration ? parseInt(activity.duration.split(':')[1]) : 0,
          durationSeconds: activity.duration ? parseInt(activity.duration.split(':')[2]) : 0,
          isPublic: !isPublic
        });

        if (updateResponse.data.success) {
          setIsPublic(!isPublic);
          setMessage({
            type: 'success',
            text: `Activity ${!isPublic ? 'marked as public' : 'made private'} successfully`
          });

          if (onUpdate) {
            onUpdate();
          }
        }
      }
    } catch (error: any) {
      console.error('Error updating activity:', error);
      setMessage({
        type: 'error',
        text: error.message || 'Failed to update activity'
      });
    } finally {
      setLoading(false);
    }
  };

  const handleActivityIdChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setActivityId(event.target.value);
  };

  return (
    <Card sx={{ maxWidth: 600, mx: 'auto', mt: 4 }}>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Toggle Activity Public Status
        </Typography>

        <Box sx={{ mb: 3 }}>
          <TextField
            fullWidth
            label="Activity ID"
            value={activityId}
            onChange={handleActivityIdChange}
            placeholder="Enter activity ID (e.g., ec5d4067-18d9-4564-89c7-f2d17604239e)"
            variant="outlined"
          />
        </Box>

        <Box sx={{ mb: 3 }}>
          <FormControlLabel
            control={
              <Switch
                checked={isPublic}
                onChange={(e) => setIsPublic(e.target.checked)}
                color="primary"
              />
            }
            label={`Activity is ${isPublic ? 'Public' : 'Private'}`}
          />
        </Box>

        {message && (
          <Alert severity={message.type} sx={{ mb: 2 }}>
            {message.text}
          </Alert>
        )}

        <Button
          variant="contained"
          onClick={handleTogglePublic}
          disabled={loading || !activityId.trim()}
          startIcon={loading ? <CircularProgress size={20} /> : null}
          fullWidth
        >
          {loading ? 'Updating...' : `Make Activity ${isPublic ? 'Private' : 'Public'}`}
        </Button>

        <Box sx={{ mt: 2 }}>
          <Typography variant="body2" color="text.secondary">
            <strong>Note:</strong> Public activities can be accessed without authentication via{' '}
            <code>/api/activities/public/{'{id}'}</code>
          </Typography>
        </Box>
      </CardContent>
    </Card>
  );
};

export default ActivityPublicToggle;