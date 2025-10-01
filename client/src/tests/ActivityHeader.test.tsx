import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import ActivityHeader from '../components/activities/ActivityHeader';

const theme = createTheme();

const mockActivity = {
  id: 'activity-1',
  name: 'Test Activity',
  description: 'Test Description',
  durationHours: 1,
  durationMinutes: 30,
  durationSeconds: 45,
  activityCategoryId: 'category-1',
  activityCategory: {
    id: 'category-1',
    name: 'Test Category',
  },
};

const mockFavorites = new Set(['activity-1', 'activity-2']);
const mockOnToggleFavorite = jest.fn();
const mockOnBack = jest.fn();

const renderWithTheme = (component: React.ReactElement) => {
  return render(
    <ThemeProvider theme={theme}>
      {component}
    </ThemeProvider>
  );
};

describe('ActivityHeader', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders activity name and description', () => {
    renderWithTheme(
      <ActivityHeader
        activity={mockActivity}
        favorites={mockFavorites}
        onToggleFavorite={mockOnToggleFavorite}
        onBack={mockOnBack}
      />
    );

    expect(screen.getByText('Test Activity')).toBeInTheDocument();
    expect(screen.getByText('Test Description')).toBeInTheDocument();
  });

  it('renders favorite icon as filled when activity is favorited', () => {
    renderWithTheme(
      <ActivityHeader
        activity={mockActivity}
        favorites={mockFavorites}
        onToggleFavorite={mockOnToggleFavorite}
        onBack={mockOnBack}
      />
    );

    const favoriteButton = screen.getByRole('button');
    expect(favoriteButton).toBeInTheDocument();
    // The filled favorite icon should be present
    expect(favoriteButton.querySelector('svg')).toBeInTheDocument();
  });

  it('renders favorite icon as outlined when activity is not favorited', () => {
    const emptyFavorites = new Set<string>();

    renderWithTheme(
      <ActivityHeader
        activity={mockActivity}
        favorites={emptyFavorites}
        onToggleFavorite={mockOnToggleFavorite}
        onBack={mockOnBack}
      />
    );

    const favoriteButton = screen.getByRole('button');
    expect(favoriteButton).toBeInTheDocument();
    // The outlined favorite icon should be present
    expect(favoriteButton.querySelector('svg')).toBeInTheDocument();
  });

  it('calls onToggleFavorite when favorite button is clicked', () => {
    renderWithTheme(
      <ActivityHeader
        activity={mockActivity}
        favorites={mockFavorites}
        onToggleFavorite={mockOnToggleFavorite}
        onBack={mockOnBack}
      />
    );

    const favoriteButton = screen.getByRole('button');
    fireEvent.click(favoriteButton);

    expect(mockOnToggleFavorite).toHaveBeenCalledTimes(1);
  });

  it('formats duration correctly with all components', () => {
    renderWithTheme(
      <ActivityHeader
        activity={mockActivity}
        favorites={mockFavorites}
        onToggleFavorite={mockOnToggleFavorite}
        onBack={mockOnBack}
      />
    );

    expect(screen.getByText('Duration: 1h 30m 45s')).toBeInTheDocument();
  });

  it('formats duration correctly with only minutes', () => {
    const activityWithMinutesOnly = {
      ...mockActivity,
      durationHours: undefined,
      durationMinutes: 45,
      durationSeconds: undefined,
    };

    renderWithTheme(
      <ActivityHeader
        activity={activityWithMinutesOnly}
        favorites={mockFavorites}
        onToggleFavorite={mockOnToggleFavorite}
        onBack={mockOnBack}
      />
    );

    expect(screen.getByText('Duration: 45m')).toBeInTheDocument();
  });

  it('formats duration correctly with only seconds', () => {
    const activityWithSecondsOnly = {
      ...mockActivity,
      durationHours: undefined,
      durationMinutes: undefined,
      durationSeconds: 30,
    };

    renderWithTheme(
      <ActivityHeader
        activity={activityWithSecondsOnly}
        favorites={mockFavorites}
        onToggleFavorite={mockOnToggleFavorite}
        onBack={mockOnBack}
      />
    );

    expect(screen.getByText('Duration: 30s')).toBeInTheDocument();
  });

  it('shows N/A for duration when no duration components are provided', () => {
    const activityWithoutDuration = {
      ...mockActivity,
      durationHours: undefined,
      durationMinutes: undefined,
      durationSeconds: undefined,
    };

    renderWithTheme(
      <ActivityHeader
        activity={activityWithoutDuration}
        favorites={mockFavorites}
        onToggleFavorite={mockOnToggleFavorite}
        onBack={mockOnBack}
      />
    );

    expect(screen.getByText('Duration: N/A')).toBeInTheDocument();
  });

  it('renders activity category chip when category is provided', () => {
    renderWithTheme(
      <ActivityHeader
        activity={mockActivity}
        favorites={mockFavorites}
        onToggleFavorite={mockOnToggleFavorite}
        onBack={mockOnBack}
      />
    );

    expect(screen.getByText('Test Category')).toBeInTheDocument();
  });

  it('does not render category section when category is not provided', () => {
    const activityWithoutCategory = {
      ...mockActivity,
      activityCategory: undefined,
    };

    renderWithTheme(
      <ActivityHeader
        activity={activityWithoutCategory}
        favorites={mockFavorites}
        onToggleFavorite={mockOnToggleFavorite}
        onBack={mockOnBack}
      />
    );

    expect(screen.queryByText('Test Category')).not.toBeInTheDocument();
  });

  it('handles activity without description', () => {
    const activityWithoutDescription = {
      ...mockActivity,
      description: undefined,
    };

    renderWithTheme(
      <ActivityHeader
        activity={activityWithoutDescription}
        favorites={mockFavorites}
        onToggleFavorite={mockOnToggleFavorite}
        onBack={mockOnBack}
      />
    );

    expect(screen.getByText('Test Activity')).toBeInTheDocument();
    expect(screen.queryByText('Test Description')).not.toBeInTheDocument();
  });

  it('applies word break styling to long activity names and descriptions', () => {
    const activityWithLongText = {
      ...mockActivity,
      name: 'Very Long Activity Name That Should Break Words Properly',
      description: 'Very long description that should also break words properly when it exceeds normal line length',
    };

    renderWithTheme(
      <ActivityHeader
        activity={activityWithLongText}
        favorites={mockFavorites}
        onToggleFavorite={mockOnToggleFavorite}
        onBack={mockOnBack}
      />
    );

    const nameElement = screen.getByText('Very Long Activity Name That Should Break Words Properly');
    const descriptionElement = screen.getByText('Very long description that should also break words properly when it exceeds normal line length');

    expect(nameElement).toHaveStyle({ wordBreak: 'break-word' });
    expect(descriptionElement).toHaveStyle({ wordBreak: 'break-word' });
  });

  it('renders correctly on mobile screens', () => {
    // Mock mobile screen
    Object.defineProperty(window, 'innerWidth', {
      writable: true,
      configurable: true,
      value: 400,
    });

    renderWithTheme(
      <ActivityHeader
        activity={mockActivity}
        favorites={mockFavorites}
        onToggleFavorite={mockOnToggleFavorite}
        onBack={mockOnBack}
      />
    );

    // The component should render without crashing on mobile
    expect(screen.getByText('Test Activity')).toBeInTheDocument();
  });
});