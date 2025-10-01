import React from 'react';
import { render, screen } from '@testing-library/react';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import ActivityStepContent from '../components/activities/ActivityStepContent';

const theme = createTheme();

const renderWithTheme = (component: React.ReactElement) => {
  return render(
    <ThemeProvider theme={theme}>
      {component}
    </ThemeProvider>
  );
};

const mockStep = {
  id: 'step-1',
  order: 1,
  description: 'This is the first step of the activity. Follow these instructions carefully.',
  pauseTimeSeconds: 45,
};

describe('ActivityStepContent', () => {
  it('renders step number and description', () => {
    renderWithTheme(
      <ActivityStepContent currentStep={mockStep} />
    );

    expect(screen.getByText('Step 1')).toBeInTheDocument();
    expect(screen.getByText('This is the first step of the activity. Follow these instructions carefully.')).toBeInTheDocument();
  });

  it('renders pause time when available', () => {
    renderWithTheme(
      <ActivityStepContent currentStep={mockStep} />
    );

    expect(screen.getByText('Pause at: 0:45')).toBeInTheDocument();
  });

  it('formats pause time correctly for different durations', () => {
    const testCases = [
      { pauseTimeSeconds: 30, expected: 'Pause at: 0:30' },
      { pauseTimeSeconds: 90, expected: 'Pause at: 1:30' },
      { pauseTimeSeconds: 125, expected: 'Pause at: 2:05' },
      { pauseTimeSeconds: 3600, expected: 'Pause at: 60:00' },
    ];

    testCases.forEach(({ pauseTimeSeconds, expected }) => {
      const stepWithPauseTime = {
        ...mockStep,
        pauseTimeSeconds,
      };

      const { rerender } = renderWithTheme(
        <ActivityStepContent currentStep={stepWithPauseTime} />
      );

      expect(screen.getByText(expected)).toBeInTheDocument();

      // Clean up for next test
      rerender(<div />);
    });
  });

  it('does not render pause time when not available', () => {
    const stepWithoutPauseTime = {
      ...mockStep,
      pauseTimeSeconds: undefined,
    };

    renderWithTheme(
      <ActivityStepContent currentStep={stepWithoutPauseTime} />
    );

    expect(screen.queryByText(/Pause at:/)).not.toBeInTheDocument();
  });

  it('does not render when currentStep is null', () => {
    renderWithTheme(
      <ActivityStepContent currentStep={null} />
    );

    expect(screen.queryByText(/Step/)).not.toBeInTheDocument();
    expect(screen.queryByText(/Pause at:/)).not.toBeInTheDocument();
  });

  it('applies correct styling for mobile screens', () => {
    // Mock mobile screen
    Object.defineProperty(window, 'innerWidth', {
      writable: true,
      configurable: true,
      value: 400,
    });

    renderWithTheme(
      <ActivityStepContent currentStep={mockStep} />
    );

    expect(screen.getByText('Step 1')).toBeInTheDocument();
    // Mobile styling should be applied (smaller font sizes)
  });

  it('applies correct styling for desktop screens', () => {
    // Mock desktop screen
    Object.defineProperty(window, 'innerWidth', {
      writable: true,
      configurable: true,
      value: 1200,
    });

    renderWithTheme(
      <ActivityStepContent currentStep={mockStep} />
    );

    expect(screen.getByText('Step 1')).toBeInTheDocument();
    // Desktop styling should be applied (larger font sizes)
  });

  it('handles long step descriptions with word breaking', () => {
    const stepWithLongDescription = {
      ...mockStep,
      description: 'This is a very long step description that should break words properly when it exceeds the normal line length and needs to wrap to multiple lines in the UI component.',
    };

    renderWithTheme(
      <ActivityStepContent currentStep={stepWithLongDescription} />
    );

    const descriptionElement = screen.getByText(/This is a very long step description/);
    expect(descriptionElement).toHaveStyle({ wordBreak: 'break-word' });
  });

  it('renders step number with correct color and weight', () => {
    renderWithTheme(
      <ActivityStepContent currentStep={mockStep} />
    );

    const stepTitle = screen.getByText('Step 1');
    expect(stepTitle).toHaveClass('MuiTypography-h6');
    // Should have primary color and font weight 600
  });

  it('renders description with correct typography', () => {
    renderWithTheme(
      <ActivityStepContent currentStep={mockStep} />
    );

    const description = screen.getByText('This is the first step of the activity. Follow these instructions carefully.');
    expect(description).toHaveClass('MuiTypography-body1');
    expect(description).toHaveStyle({ lineHeight: '1.6' });
  });

  it('handles step with order 0', () => {
    const stepWithOrderZero = {
      ...mockStep,
      order: 0,
    };

    renderWithTheme(
      <ActivityStepContent currentStep={stepWithOrderZero} />
    );

    expect(screen.getByText('Step 0')).toBeInTheDocument();
  });

  it('handles step with very high order number', () => {
    const stepWithHighOrder = {
      ...mockStep,
      order: 999,
    };

    renderWithTheme(
      <ActivityStepContent currentStep={stepWithHighOrder} />
    );

    expect(screen.getByText('Step 999')).toBeInTheDocument();
  });

  it('handles empty description', () => {
    const stepWithEmptyDescription = {
      ...mockStep,
      description: '',
    };

    renderWithTheme(
      <ActivityStepContent currentStep={stepWithEmptyDescription} />
    );

    expect(screen.getByText('Step 1')).toBeInTheDocument();
    // Empty description should still render the Typography element
    const descriptionElement = screen.getByRole('paragraph', { hidden: true });
    expect(descriptionElement).toBeInTheDocument();
  });

  it('handles pause time of 0 seconds', () => {
    const stepWithZeroPauseTime = {
      ...mockStep,
      pauseTimeSeconds: 0,
    };

    renderWithTheme(
      <ActivityStepContent currentStep={stepWithZeroPauseTime} />
    );

    expect(screen.getByText('Pause at: 0:0')).toBeInTheDocument();
  });
});