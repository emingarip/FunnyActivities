import React from 'react';
import {
  Box,
  Button,
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

interface ActivityStepsNavigatorProps {
  steps: ActivityStep[];
  currentStepIndex: number;
  onStepClick: (stepIndex: number) => void;
}

const ActivityStepsNavigator: React.FC<ActivityStepsNavigatorProps> = ({
  steps,
  currentStepIndex,
  onStepClick,
}) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  if (steps.length === 0) {
    return null;
  }

  return (
    <Box sx={{ mb: 2 }} data-cy="step-navigator">
      <Typography
        variant="h6"
        gutterBottom
        sx={{ fontSize: isMobile ? '1.1rem' : '1.25rem' }}
      >
        Activity Steps
      </Typography>
      <Box
        sx={{
          display: 'flex',
          gap: 1,
          flexWrap: 'wrap',
          alignItems: 'center',
        }}
      >
        {steps.map((step, index) => (
          <Button
            key={step.id}
            data-cy="step-button"
            variant={index === currentStepIndex ? 'contained' : 'outlined'}
            size={isMobile ? 'small' : 'medium'}
            onClick={() => onStepClick(index)}
            sx={{
              minWidth: 44,
              minHeight: 44,
              borderRadius: '50%',
              fontSize: isMobile ? '1rem' : '1rem',
              fontWeight: index === currentStepIndex ? 600 : 400,
            }}
          >
            {step.order}
          </Button>
        ))}
      </Box>
    </Box>
  );
};

export default ActivityStepsNavigator;