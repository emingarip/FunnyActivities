import React, { useEffect, useState, useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Container,
  Box,
  Typography,
  CircularProgress,
  Alert,
  Button,
} from '@mui/material';
import { ArrowBack as ArrowBackIcon } from '@mui/icons-material';
import ActivityForm from '../components/activities/ActivityForm';
import { activitiesAPI, activityCategoriesAPI } from '../services/api';

interface ActivityCategory {
  id: string;
  name: string;
  description?: string;
}

interface Activity {
  id: string;
  name: string;
  description?: string;
  activityCategoryId?: string;
  videoUrl?: string;
  durationHours?: number;
  durationMinutes?: number;
  durationSeconds?: number;
}

const ActivityEditPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [activity, setActivity] = useState<Activity | null>(null);
  const [categories, setCategories] = useState<ActivityCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [categoryWarning, setCategoryWarning] = useState<string | null>(null);

  const handleNavigateBack = useCallback(() => {
    navigate('/admin/activities');
  }, [navigate]);

  useEffect(() => {
    const loadData = async () => {
      if (!id) {
        setError('Activity identifier is missing.');
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);
        setCategoryWarning(null);

        const [activityResponse, categoriesResponse] = await Promise.all([
          activitiesAPI.getActivityWithDetails(id),
          activityCategoriesAPI.getActivityCategories({ pageSize: 100 }),
        ]);

        if (activityResponse.data.success && activityResponse.data.data) {
          setActivity(activityResponse.data.data);
        } else {
          setActivity(null);
          setError('Activity could not be loaded. Please try again.');
        }

        if (categoriesResponse.data.success) {
          setCategories(categoriesResponse.data.data?.items || []);
        } else {
          setCategories([]);
          setCategoryWarning('Activity categories could not be loaded.');
        }
      } catch (err) {
        console.error('[ActivityEditPage] Failed to load activity data:', err);
        setActivity(null);
        setCategories([]);
        setError('We couldn\'t load this activity. Please refresh and try again.');
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [id]);

  if (!id) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={handleNavigateBack}
          sx={{ mb: 2 }}
        >
          Back to Activities
        </Button>
        <Alert severity="error">No activity identifier was provided.</Alert>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Button
        startIcon={<ArrowBackIcon />}
        onClick={handleNavigateBack}
        sx={{ mb: 2 }}
      >
        Back to Activities
      </Button>

      <Typography variant="h4" component="h1" gutterBottom>
        Edit Activity
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {categoryWarning && (
        <Alert severity="warning" sx={{ mb: 3 }}>
          {categoryWarning}
        </Alert>
      )}

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
          <CircularProgress />
        </Box>
      ) : activity ? (
        <ActivityForm
          activity={activity}
          categories={categories}
          onSuccess={handleNavigateBack}
          onCancel={handleNavigateBack}
        />
      ) : (
        <Alert severity="info">
          The requested activity could not be found.
        </Alert>
      )}
    </Container>
  );
};

export default ActivityEditPage;
