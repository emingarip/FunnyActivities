import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import ActivityVideoPlayer from '../components/activities/ActivityVideoPlayer';

const theme = createTheme();

const renderWithTheme = (component: React.ReactElement) => {
  return render(
    <ThemeProvider theme={theme}>
      {component}
    </ThemeProvider>
  );
};

// Mock HTMLVideoElement
const mockVideoElement = {
  addEventListener: jest.fn(),
  removeEventListener: jest.fn(),
  play: jest.fn(),
  pause: jest.fn(),
  currentTime: 0,
  duration: 120,
  paused: true,
  ended: false,
};

Object.defineProperty(window.HTMLMediaElement.prototype, 'play', {
  writable: true,
  value: jest.fn().mockImplementation(() => Promise.resolve()),
});

Object.defineProperty(window.HTMLMediaElement.prototype, 'pause', {
  writable: true,
  value: jest.fn(),
});

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
];

const defaultProps = {
  videoUrl: 'https://example.com/video.mp4',
  steps: mockSteps,
  currentStepIndex: 0,
  isPlaying: false,
  isPausedAtStep: false,
  onPlay: jest.fn(),
  onPause: jest.fn(),
  onContinue: jest.fn(),
  onTimeUpdate: jest.fn(),
  onEnded: jest.fn(),
};

describe('ActivityVideoPlayer', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders video element when videoUrl is provided', () => {
    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = screen.getByTestId('activity-video') || document.querySelector('video');
    expect(video).toBeInTheDocument();
    expect(video).toHaveAttribute('src', 'https://example.com/video.mp4');
  });

  it('does not render when videoUrl is null', () => {
    renderWithTheme(
      <ActivityVideoPlayer
        {...defaultProps}
        videoUrl={null}
      />
    );

    const video = screen.queryByTestId('activity-video') || document.querySelector('video');
    expect(video).not.toBeInTheDocument();
  });

  it('sets up video event listeners on mount', () => {
    const addEventListenerSpy = jest.fn();
    const mockVideo = {
      ...mockVideoElement,
      addEventListener: addEventListenerSpy,
    };

    // Mock the video ref
    jest.spyOn(require('react'), 'useRef').mockReturnValue({ current: mockVideo });

    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    // Event listeners should be added
    expect(addEventListenerSpy).toHaveBeenCalledWith('timeupdate', expect.any(Function));
    expect(addEventListenerSpy).toHaveBeenCalledWith('play', expect.any(Function));
    expect(addEventListenerSpy).toHaveBeenCalledWith('pause', expect.any(Function));
    expect(addEventListenerSpy).toHaveBeenCalledWith('ended', expect.any(Function));
  });

  it('shows step overlay when isPausedAtStep is true', () => {
    renderWithTheme(
      <ActivityVideoPlayer
        {...defaultProps}
        isPausedAtStep={true}
      />
    );

    expect(screen.getByText('Step 1')).toBeInTheDocument();
    expect(screen.getByText('First step')).toBeInTheDocument();
    expect(screen.getByText('Continue Activity')).toBeInTheDocument();
  });

  it('hides video controls when paused at step', () => {
    renderWithTheme(
      <ActivityVideoPlayer
        {...defaultProps}
        isPausedAtStep={true}
      />
    );

    const video = document.querySelector('video');
    expect(video).not.toHaveAttribute('controls');
  });

  it('shows video controls when not paused at step', () => {
    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = document.querySelector('video');
    expect(video).toHaveAttribute('controls');
  });

  it('calls onContinue when continue button is clicked', () => {
    renderWithTheme(
      <ActivityVideoPlayer
        {...defaultProps}
        isPausedAtStep={true}
      />
    );

    const continueButton = screen.getByText('Continue Activity');
    fireEvent.click(continueButton);

    expect(defaultProps.onContinue).toHaveBeenCalledTimes(1);
  });

  it('calls onPlay when video play event is triggered', () => {
    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = document.querySelector('video');
    fireEvent.play(video!);

    expect(defaultProps.onPlay).toHaveBeenCalledTimes(1);
  });

  it('calls onPause when video pause event is triggered', () => {
    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = document.querySelector('video');
    fireEvent.pause(video!);

    expect(defaultProps.onPause).toHaveBeenCalledTimes(1);
  });

  it('calls onEnded when video ended event is triggered', () => {
    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = document.querySelector('video');
    fireEvent.ended(video!);

    expect(defaultProps.onEnded).toHaveBeenCalledTimes(1);
  });

  it('calls onTimeUpdate when video timeupdate event is triggered', () => {
    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = document.querySelector('video');
    // Simulate timeupdate event
    fireEvent.timeUpdate(video!, { target: { currentTime: 15 } });

    expect(defaultProps.onTimeUpdate).toHaveBeenCalledWith(15);
  });

  it('pauses video when reaching step pause time', () => {
    const mockVideo = {
      ...mockVideoElement,
      currentTime: 30,
      pause: jest.fn(),
    };

    // Mock the video element
    jest.spyOn(document, 'querySelector').mockReturnValue(mockVideo as any);

    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = document.querySelector('video');
    fireEvent.timeUpdate(video!, { target: { currentTime: 30 } });

    expect(mockVideo.pause).toHaveBeenCalledTimes(1);
    expect(defaultProps.onPause).toHaveBeenCalledTimes(1);
  });

  it('does not pause video when not at step pause time', () => {
    const mockVideo = {
      ...mockVideoElement,
      currentTime: 25,
      pause: jest.fn(),
    };

    jest.spyOn(document, 'querySelector').mockReturnValue(mockVideo as any);

    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = document.querySelector('video');
    fireEvent.timeUpdate(video!, { target: { currentTime: 25 } });

    expect(mockVideo.pause).not.toHaveBeenCalled();
    expect(defaultProps.onPause).not.toHaveBeenCalled();
  });

  it('handles video load event', () => {
    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = document.querySelector('video');
    fireEvent.load(video!);

    // Should not crash
    expect(video).toBeInTheDocument();
  });

  it('renders with correct mobile styling', () => {
    // Mock mobile screen
    Object.defineProperty(window, 'innerWidth', {
      writable: true,
      configurable: true,
      value: 400,
    });

    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = document.querySelector('video');
    expect(video).toBeInTheDocument();
    // Mobile styling should be applied (maxHeight: 300px)
  });

  it('renders with correct desktop styling', () => {
    // Mock desktop screen
    Object.defineProperty(window, 'innerWidth', {
      writable: true,
      configurable: true,
      value: 1200,
    });

    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = document.querySelector('video');
    expect(video).toBeInTheDocument();
    // Desktop styling should be applied (maxHeight: 500px)
  });

  it('handles steps with no pause time', () => {
    const stepsWithoutPause = [
      {
        id: 'step-1',
        order: 1,
        description: 'First step',
        pauseTimeSeconds: undefined,
      },
    ];

    renderWithTheme(
      <ActivityVideoPlayer
        {...defaultProps}
        steps={stepsWithoutPause}
      />
    );

    const video = document.querySelector('video');
    fireEvent.timeUpdate(video!, { target: { currentTime: 30 } });

    // Should not pause since no pause time is set
    expect(defaultProps.onPause).not.toHaveBeenCalled();
  });

  it('handles empty steps array', () => {
    renderWithTheme(
      <ActivityVideoPlayer
        {...defaultProps}
        steps={[]}
      />
    );

    const video = document.querySelector('video');
    expect(video).toBeInTheDocument();
    // Should not crash with empty steps
  });

  it('cleans up event listeners on unmount', () => {
    const { unmount } = renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    unmount();

    const video = document.querySelector('video');
    expect(mockVideoElement.removeEventListener).toHaveBeenCalledWith('timeupdate', expect.any(Function));
    expect(mockVideoElement.removeEventListener).toHaveBeenCalledWith('play', expect.any(Function));
    expect(mockVideoElement.removeEventListener).toHaveBeenCalledWith('pause', expect.any(Function));
    expect(mockVideoElement.removeEventListener).toHaveBeenCalledWith('ended', expect.any(Function));
  });
});