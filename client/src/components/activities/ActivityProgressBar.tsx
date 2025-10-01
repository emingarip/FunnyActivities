import React from 'react';
import {
  Box,
  Typography,
  LinearProgress,
  useTheme,
  useMediaQuery,
} from '@mui/material';

interface ActivityProgressBarProps {
  progressPercentage: number;
}

const ActivityProgressBar: React.FC<ActivityProgressBarProps> = ({
  progressPercentage,
}) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  if (progressPercentage <= 0) {
    return null;
  }

  return (
    <Box sx={{ mb: 3 }} data-cy="progress-bar">
      <Typography
        variant="body2"
        color="text.secondary"
        sx={{ mb: 1, fontSize: isMobile ? '1rem' : '1rem' }}
      >
        Progress: {progressPercentage}%
      </Typography>
      <LinearProgress
        variant="determinate"
        value={progressPercentage}
        sx={{
          height: isMobile ? 6 : 8,
          borderRadius: 4,
          backgroundColor: theme.palette.grey[200],
          '& .MuiLinearProgress-bar': {
            borderRadius: 4,
          },
        }}
      />
    </Box>
  );
};

export default ActivityProgressBar;