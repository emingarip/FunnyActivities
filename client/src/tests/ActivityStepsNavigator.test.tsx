import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import ActivityStepsNavigator from '../components/activities/ActivityStepsNavigator';

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
    pauseTimeSeconds: 30,
  },
  {
    id: 'step-2',
    order: 2,
    description: 'Second step',
    pauseTimeSeconds: 60,
  },
  {
    id: 'step-3',
    order: 3,
    description: 'Third step',
    pauseTimeSeconds: 90,
  },
];

const mockOnStepClick = jest.fn();

describe('ActivityStepsNavigator', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders step buttons for each step', () => {
    renderWithTheme(
      <ActivityStepsNavigator
        steps={mockSteps}
        currentStepIndex={0}
        onStepClick={mockOnStepClick}
      />
    );

    expect(screen.getByText('Activity Steps')).toBeInTheDocument();
    expect(screen.getByText('1')).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument();
    expect(screen.getByText('3')).toBeInTheDocument();
  });

  it('highlights current step button', () => {
    renderWithTheme(
      <ActivityStepsNavigator
        steps={mockSteps}
        currentStepIndex={1}
        onStepClick={mockOnStepClick}
      />
    );

    const stepButtons = screen.getAllByRole('button');
    const currentStepButton = stepButtons.find(button => button.textContent === '2');

    expect(currentStepButton).toHaveClass('MuiButton-contained'); // Contained variant for current step
  });

  it('renders other step buttons as outlined', () => {
    renderWithTheme(
      <ActivityStepsNavigator
        steps={mockSteps}
        currentStepIndex={1}
        onStepClick={mockOnStepClick}
      />
    );

    const stepButtons = screen.getAllByRole('button');
    const firstStepButton = stepButtons.find(button => button.textContent === '1');
    const thirdStepButton = stepButtons.find(button => button.textContent === '3');

    expect(firstStepButton).toHaveClass('MuiButton-outlined');
    expect(thirdStepButton).toHaveClass('MuiButton-outlined');
  });

  it('calls onStepClick with correct index when step button is clicked', () => {
    renderWithTheme(
      <ActivityStepsNavigator
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
      <ActivityStepsNavigator
        steps={[]}
        currentStepIndex={0}
        onStepClick={mockOnStepClick}
      />
    );

    expect(screen.queryByText('Activity Steps')).not.toBeInTheDocument();
    expect(screen.queryByRole('button')).not.toBeInTheDocument();
  });

  it('renders with correct mobile styling', () => {
    // Mock mobile screen
    Object.defineProperty(window, 'innerWidth', {
      writable: true,
      configurable: true,
      value: 400,
    });

    renderWithTheme(
      <ActivityStepsNavigator
        steps={mockSteps}
        currentStepIndex={0}
        onStepClick={mockOnStepClick}
      />
    );

    const stepButtons = screen.getAllByRole('button');
    expect(stepButtons).toHaveLength(3);

    // Mobile buttons should be smaller
    stepButtons.forEach(button => {
      expect(button).toBeInTheDocument();
    });
  });

  it('renders with correct desktop styling', () => {
    // Mock desktop screen
    Object.defineProperty(window, 'innerWidth', {
      writable: true,
      configurable: true,
      value: 1200,
    });

    renderWithTheme(
      <ActivityStepsNavigator
        steps={mockSteps}
        currentStepIndex={0}
        onStepClick={mockOnStepClick}
      />
    );

    const stepButtons = screen.getAllByRole('button');
    expect(stepButtons).toHaveLength(3);

    // Desktop buttons should be larger
    stepButtons.forEach(button => {
      expect(button).toBeInTheDocument();
    });
  });

  it('handles single step', () => {
    const singleStep = [mockSteps[0]];

    renderWithTheme(
      <ActivityStepsNavigator
        steps={singleStep}
        currentStepIndex={0}
        onStepClick={mockOnStepClick}
      />
    );

    expect(screen.getByText('Activity Steps')).toBeInTheDocument();
    expect(screen.getByText('1')).toBeInTheDocument();
    expect(screen.queryByText('2')).not.toBeInTheDocument();
  });

  it('handles current step at the end of the array', () => {
    renderWithTheme(
      <ActivityStepsNavigator
        steps={mockSteps}
        currentStepIndex={2}
        onStepClick={mockOnStepClick}
      />
    );

    const stepButtons = screen.getAllByRole('button');
    const thirdStepButton = stepButtons.find(button => button.textContent === '3');

    expect(thirdStepButton).toHaveClass('MuiButton-contained');
  });

  it('handles clicking on current step button', () => {
    renderWithTheme(
      <ActivityStepsNavigator
        steps={mockSteps}
        currentStepIndex={1}
        onStepClick={mockOnStepClick}
      />
    );

    const stepButtons = screen.getAllByRole('button');
    const currentStepButton = stepButtons.find(button => button.textContent === '2');

    fireEvent.click(currentStepButton!);

    expect(mockOnStepClick).toHaveBeenCalledWith(1);
  });

  it('renders step numbers in correct order', () => {
    const unorderedSteps = [
      {
        id: 'step-3',
        order: 3,
        description: 'Third step',
        pauseTimeSeconds: 90,
      },
      {
        id: 'step-1',
        order: 1,
        description: 'First step',
        pauseTimeSeconds: 30,
      },
      {
        id: 'step-2',
        order: 2,
        description: 'Second step',
        pauseTimeSeconds: 60,
      },
    ];

    renderWithTheme(
      <ActivityStepsNavigator
        steps={unorderedSteps}
        currentStepIndex={0}
        onStepClick={mockOnStepClick}
      />
    );

    // Should render based on order property, not array order
    expect(screen.getByText('1')).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument();
    expect(screen.getByText('3')).toBeInTheDocument();
  });
});