import React from 'react';
import {
  Box,
  Typography,
  useTheme,
  useMediaQuery,
} from '@mui/material';

interface ActivityStep {
  id: string;
  order: number;
  description: string;
  pauseTimeSeconds?: number;
}

interface ActivityStepContentProps {
  currentStep: ActivityStep | null;
}

const ActivityStepContent: React.FC<ActivityStepContentProps> = ({
  currentStep,
}) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  if (!currentStep) {
    return null;
  }

  return (
    <Box sx={{ mb: 2 }}>
      <Typography
        variant="h6"
        color="primary"
        gutterBottom
        sx={{
          fontSize: isMobile ? '1.1rem' : '1.25rem',
          fontWeight: 600,
        }}
      >
        Step {currentStep.order}
      </Typography>
      <Typography
        variant="body1"
        sx={{
          fontSize: isMobile ? '1rem' : '1rem',
          lineHeight: 1.6,
          wordBreak: 'break-word',
        }}
      >
        {currentStep.description}
      </Typography>
      {currentStep.pauseTimeSeconds && (
        <Typography
          variant="body2"
          color="text.secondary"
          sx={{
            mt: 1,
            fontSize: isMobile ? '0.875rem' : '0.875rem',
          }}
        >
          Pause at: {Math.floor(currentStep.pauseTimeSeconds / 60)}:
          {(currentStep.pauseTimeSeconds % 60).toString().padStart(2, '0')}
        </Typography>
      )}
    </Box>
  );
};

export default ActivityStepContent;