import React, { useState } from 'react';
import { Box, Typography, useTheme, useMediaQuery } from '@mui/material';
import ActivityList from '../components/activities/ActivityList';
import ActivityDetail from '../components/activities/ActivityDetail';

interface Activity {
  id: string;
  name: string;
  description?: string;
  durationHours?: number;
  durationMinutes?: number;
  durationSeconds?: number;
  activityCategoryId?: string;
  activityCategory?: {
    id: string;
    name: string;
  };
  videoUrl?: string;
}

const Activities: React.FC = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  const [selectedActivity, setSelectedActivity] = useState<Activity | null>(null);
  const [view, setView] = useState<'list' | 'detail'>('list');

  const handleActivitySelect = (activity: Activity) => {
    setSelectedActivity(activity);
    setView('detail');
  };

  const handleBackToList = () => {
    setSelectedActivity(null);
    setView('list');
  };

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
      {/* Header */}
      <Box sx={{ bgcolor: 'primary.main', color: 'primary.contrastText', py: 3, px: 3 }}>
        <Typography variant="h3" component="h1" gutterBottom>
          Activities
        </Typography>
        <Typography variant="body1">
          Discover and participate in fun activities with step-by-step guidance
        </Typography>
      </Box>

      {/* Content */}
      <Box sx={{ py: 4 }}>
        {view === 'list' ? (
          <ActivityList onActivitySelect={handleActivitySelect} />
        ) : (
          selectedActivity && (
            <ActivityDetail
              activityId={selectedActivity.id}
              onBack={handleBackToList}
            />
          )
        )}
      </Box>
    </Box>
  );
};

export default Activities;