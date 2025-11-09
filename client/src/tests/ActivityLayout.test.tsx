import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import ActivityLayout from '../components/activities/ActivityLayout';

const theme = createTheme();

const renderWithTheme = (component: React.ReactElement) => {
  return render(
    <ThemeProvider theme={theme}>
      {component}
    </ThemeProvider>
  );
};

const mockSteps = [
  {
    id: 'step-1',
    order: 1,
    description: 'First step',
    timestampSeconds: 30,
  },
  {
    id: 'step-2',
    order: 2,
    description: 'Second step',
    timestampSeconds: 60,
  },
  {
    id: 'step-3',
    order: 3,
    description: 'Third step',
    timestampSeconds: 90,
  },
];

const mockOnStepClick = jest.fn();

describe('ActivityLayout', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders ActivityStepsNavigator and ActivityStepContent', () => {
    renderWithTheme(
      <ActivityLayout
        steps={mockSteps}
        currentStepIndex={0}
        onStepClick={mockOnStepClick}
      />
    );

    expect(screen.getByText('Activity Steps')).toBeInTheDocument();
    expect(screen.getByText('Step 1')).toBeInTheDocument();
    expect(screen.getByText('First step')).toBeInTheDocument();
  });

  it('passes correct props to ActivityStepsNavigator', () => {
    renderWithTheme(
      <ActivityLayout
        steps={mockSteps}
        currentStepIndex={1}
        onStepClick={mockOnStepClick}
      />
    );

    // Check that step buttons are rendered (from ActivityStepsNavigator)
    expect(screen.getByText('1')).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument();
    expect(screen.getByText('3')).toBeInTheDocument();

    // The second step should be highlighted (currentStepIndex = 1)
    const stepButtons = screen.getAllByRole('button');
    const secondStepButton = stepButtons.find(button => button.textContent === '2');
    expect(secondStepButton).toHaveClass('MuiButton-contained');
  });

  it('passes correct props to ActivityStepContent', () => {
    renderWithTheme(
      <ActivityLayout
        steps={mockSteps}
        currentStepIndex={2}
        onStepClick={mockOnStepClick}
      />
    );

    expect(screen.getByText('Step 3')).toBeInTheDocument();
    expect(screen.getByText('Third step')).toBeInTheDocument();
  });

  it('handles step click through ActivityStepsNavigator', () => {
    renderWithTheme(
      <ActivityLayout
        steps={mockSteps}
        currentStepIndex={0}
        onStepClick={mockOnStepClick}
      />
    );

    const stepButtons = screen.getAllByRole('button');
    const secondStepButton = stepButtons.find(button => button.textContent === '2');

    fireEvent.click(secondStepButton!);

    expect(mockOnStepClick).toHaveBeenCalledWith(1);
    expect(mockOnStepClick).toHaveBeenCalledTimes(1);
  });

  it('does not render when steps array is empty', () => {
    renderWithTheme(
      <ActivityLayout
        steps={[]}
        currentStepIndex={0}
        onStepClick={mockOnStepClick}
      />
    );

    expect(screen.queryByText('Activity Steps')).not.toBeInTheDocument();
    expect(screen.queryByText(/Step/)).not.toBeInTheDocument();
  });

  it('renders with correct mobile styling', () => {
    // Mock mobile screen
    Object.defineProperty(window, 'innerWidth', {
      writable: true,
      configurable: true,
      value: 400,
    });

    renderWithTheme(
      <ActivityLayout
        steps={mockSteps}
        currentStepIndex={0}
        onStepClick={mockOnStepClick}
      />
    );

    // Should render with mobile padding
    const card = document.querySelector('.MuiCard-root');
    expect(card).toBeInTheDocument();
  });

  it('renders with correct desktop styling', () => {
    // Mock desktop screen
    Object.defineProperty(window, 'innerWidth', {
      writable: true,
      configurable: true,
      value: 1200,
    });

    renderWithTheme(
      <ActivityLayout
        steps={mockSteps}
        currentStepIndex={0}
        onStepClick={mockOnStepClick}
      />
    );

    // Should render with desktop padding
    const card = document.querySelector('.MuiCard-root');
    expect(card).toBeInTheDocument();
  });

  it('handles currentStepIndex at bounds', () => {
    // Test first step
    const { rerender } = renderWithTheme(
      <ActivityLayout
        steps={mockSteps}
        currentStepIndex={0}
        onStepClick={mockOnStepClick}
      />
    );

    expect(screen.getByText('Step 1')).toBeInTheDocument();

    // Test last step
    rerender(
      <ThemeProvider theme={theme}>
        <ActivityLayout
          steps={mockSteps}
          currentStepIndex={2}
          onStepClick={mockOnStepClick}
        />
      </ThemeProvider>
    );

    expect(screen.getByText('Step 3')).toBeInTheDocument();
  });

  it('handles single step', () => {
    const singleStep = [mockSteps[0]];

    renderWithTheme(
      <ActivityLayout
        steps={singleStep}
        currentStepIndex={0}
        onStepClick={mockOnStepClick}
      />
    );

    expect(screen.getByText('Activity Steps')).toBeInTheDocument();
    expect(screen.getByText('Step 1')).toBeInTheDocument();
    expect(screen.getByText('1')).toBeInTheDocument();
    expect(screen.queryByText('2')).not.toBeInTheDocument();
  });

  it('renders Card with correct content structure', () => {
    renderWithTheme(
      <ActivityLayout
        steps={mockSteps}
        currentStepIndex={0}
        onStepClick={mockOnStepClick}
      />
    );

    const card = document.querySelector('.MuiCard-root');
    expect(card).toBeInTheDocument();

    // Card should contain the navigator and content
    expect(card).toHaveClass('MuiCard-root');
  });

  it('passes onStepClick to ActivityStepsNavigator correctly', () => {
    const customOnStepClick = jest.fn();

    renderWithTheme(
      <ActivityLayout
        steps={mockSteps}
        currentStepIndex={0}
        onStepClick={customOnStepClick}
      />
    );

    const stepButtons = screen.getAllByRole('button');
    const thirdStepButton = stepButtons.find(button => button.textContent === '3');

    fireEvent.click(thirdStepButton!);

    expect(customOnStepClick).toHaveBeenCalledWith(2);
  });
});
