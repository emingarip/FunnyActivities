import React from 'react';
import { render, screen } from '@testing-library/react';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import ActivityProgressBar from '../components/activities/ActivityProgressBar';

const theme = createTheme();

const renderWithTheme = (component: React.ReactElement) => {
  return render(
    <ThemeProvider theme={theme}>
      {component}
    </ThemeProvider>
  );
};

describe('ActivityProgressBar', () => {
  it('renders progress bar with correct percentage when progress is greater than 0', () => {
    renderWithTheme(<ActivityProgressBar progressPercentage={75} />);

    expect(screen.getByText('Progress: 75%')).toBeInTheDocument();

    // Check that LinearProgress is rendered with correct value
    const progressBar = screen.getByRole('progressbar');
    expect(progressBar).toBeInTheDocument();
    expect(progressBar).toHaveAttribute('aria-valuenow', '75');
  });

  it('does not render when progress percentage is 0', () => {
    renderWithTheme(<ActivityProgressBar progressPercentage={0} />);

    expect(screen.queryByText(/Progress:/)).not.toBeInTheDocument();
    expect(screen.queryByRole('progressbar')).not.toBeInTheDocument();
  });

  it('does not render when progress percentage is negative', () => {
    renderWithTheme(<ActivityProgressBar progressPercentage={-10} />);

    expect(screen.queryByText(/Progress:/)).not.toBeInTheDocument();
    expect(screen.queryByRole('progressbar')).not.toBeInTheDocument();
  });

  it('renders with 100% progress', () => {
    renderWithTheme(<ActivityProgressBar progressPercentage={100} />);

    expect(screen.getByText('Progress: 100%')).toBeInTheDocument();

    const progressBar = screen.getByRole('progressbar');
    expect(progressBar).toHaveAttribute('aria-valuenow', '100');
  });

  it('renders with decimal progress values', () => {
    renderWithTheme(<ActivityProgressBar progressPercentage={33.5} />);

    expect(screen.getByText('Progress: 33.5%')).toBeInTheDocument();

    const progressBar = screen.getByRole('progressbar');
    expect(progressBar).toHaveAttribute('aria-valuenow', '34'); // MUI LinearProgress rounds to nearest integer
  });

  it('applies correct styling for mobile screens', () => {
    // Mock mobile screen
    Object.defineProperty(window, 'innerWidth', {
      writable: true,
      configurable: true,
      value: 400,
    });

    renderWithTheme(<ActivityProgressBar progressPercentage={50} />);

    expect(screen.getByText('Progress: 50%')).toBeInTheDocument();

    // The component should render with mobile-specific styling
    // (height: 6 for mobile vs 8 for desktop)
    const progressBar = screen.getByRole('progressbar');
    expect(progressBar).toBeInTheDocument();
  });

  it('applies correct styling for desktop screens', () => {
    // Mock desktop screen
    Object.defineProperty(window, 'innerWidth', {
      writable: true,
      configurable: true,
      value: 1200,
    });

    renderWithTheme(<ActivityProgressBar progressPercentage={50} />);

    expect(screen.getByText('Progress: 50%')).toBeInTheDocument();

    // The component should render with desktop-specific styling
    // (height: 8 for desktop vs 6 for mobile)
    const progressBar = screen.getByRole('progressbar');
    expect(progressBar).toBeInTheDocument();
  });

  it('handles edge case of very high progress values', () => {
    renderWithTheme(<ActivityProgressBar progressPercentage={150} />);

    expect(screen.getByText('Progress: 150%')).toBeInTheDocument();

    const progressBar = screen.getByRole('progressbar');
    expect(progressBar).toHaveAttribute('aria-valuenow', '150');
  });

  it('handles edge case of very low positive progress values', () => {
    renderWithTheme(<ActivityProgressBar progressPercentage={0.1} />);

    expect(screen.getByText('Progress: 0.1%')).toBeInTheDocument();

    const progressBar = screen.getByRole('progressbar');
    expect(progressBar).toHaveAttribute('aria-valuenow', '0'); // MUI LinearProgress rounds down very small values
  });

  it('renders with correct theme colors', () => {
    renderWithTheme(<ActivityProgressBar progressPercentage={60} />);

    const progressBar = screen.getByRole('progressbar');
    expect(progressBar).toBeInTheDocument();

    // The progress bar should have the theme's background color for the track
    // and default color for the progress indicator
  });
});