import React from 'react';
import {
  Card,
  CardContent,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import ActivityStepsNavigator from './ActivityStepsNavigator';
import ActivityStepContent from './ActivityStepContent';

interface ActivityStep {
  id: string;
  order: number;
  description: string;
  timestampSeconds: number;
}

interface ActivityLayoutProps {
  steps: ActivityStep[];
  currentStepIndex: number;
  onStepClick: (stepIndex: number) => void;
}

const ActivityLayout: React.FC<ActivityLayoutProps> = ({
  steps,
  currentStepIndex,
  onStepClick,
}) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  if (steps.length === 0) {
    return null;
  }

  const safeIndex = Math.min(Math.max(currentStepIndex, 0), steps.length - 1);
  const currentStep = steps[safeIndex];

  return (
    <Card data-cy="activity-layout" sx={{ mb: 3 }}>
      <CardContent sx={{ p: isMobile ? 2 : 3 }}>
        <ActivityStepsNavigator
          steps={steps}
          currentStepIndex={safeIndex}
          onStepClick={onStepClick}
        />
        <ActivityStepContent currentStep={currentStep} />
      </CardContent>
    </Card>
  );
};

export default ActivityLayout;
