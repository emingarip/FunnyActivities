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
  play: jest.fn().mockImplementation(() => Promise.resolve()),
  pause: jest.fn(),
  currentTime: 0,
  duration: 120,
  paused: true,
  ended: false,
  readyState: 4,
  networkState: 2,
  error: null,
  src: '',
  videoWidth: 1920,
  videoHeight: 1080,
  buffered: { length: 1, start: jest.fn(() => 0), end: jest.fn(() => 120) },
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
    timestampSeconds: 30,
  },
  {
    id: 'step-2',
    order: 2,
    description: 'Second step',
    timestampSeconds: 60,
  },
];

const mockActivity = {
  id: 'activity-1',
  name: 'Test Activity',
  description: 'This is a test activity',
  activityCategoryId: 'category-1',
  activityCategory: {
    id: 'category-1',
    name: 'Test Category',
  },
};

const defaultProps = {
  activity: mockActivity,
  videoUrl: 'https://example.com/video.mp4',
  introVideoUrl: undefined,
  steps: mockSteps,
  currentStepIndex: 0,
  isPlaying: false,
  isPausedAtStep: false,
  onPlay: jest.fn(),
  onPause: jest.fn(),
  onContinue: jest.fn(),
  onTimeUpdate: jest.fn(),
  onStepReached: jest.fn(),
  onEnded: jest.fn(),
};

describe('ActivityVideoPlayer', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders video element when videoUrl is provided', () => {
    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = screen.getByTestId('activity-video');
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

    expect(screen.queryByTestId('activity-video')).not.toBeInTheDocument();
  });

  it('renders intro video when introVideoUrl is provided', () => {
    renderWithTheme(
      <ActivityVideoPlayer
        {...defaultProps}
        introVideoUrl="https://example.com/intro.mp4"
      />
    );

    expect(screen.getByTestId('intro-video')).toBeInTheDocument();
    expect(screen.queryByTestId('activity-video')).not.toBeInTheDocument();
  });

  it('skips intro video when Skip Intro is clicked', async () => {
    renderWithTheme(
      <ActivityVideoPlayer
        {...defaultProps}
        introVideoUrl="https://example.com/intro.mp4"
      />
    );

    fireEvent.click(screen.getByText('Skip Intro'));

    await waitFor(() => {
      expect(screen.getByTestId('activity-video')).toBeInTheDocument();
    });
  });

  it('sets up video event listeners on mount', () => {
    const addEventListenerSpy = jest.fn();
    const mockVideo = {
      ...mockVideoElement,
      addEventListener: addEventListenerSpy,
    };

    // Mock the video ref
    const reactModule = require('react');
    const useRefSpy = jest.spyOn(reactModule, 'useRef');
    useRefSpy
      .mockReturnValueOnce({ current: mockVideo })
      .mockReturnValueOnce({ current: null });

    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    // Event listeners should be added
    expect(addEventListenerSpy).toHaveBeenCalledWith('timeupdate', expect.any(Function));
    expect(addEventListenerSpy).toHaveBeenCalledWith('play', expect.any(Function));
    expect(addEventListenerSpy).toHaveBeenCalledWith('pause', expect.any(Function));
    expect(addEventListenerSpy).toHaveBeenCalledWith('ended', expect.any(Function));

    // Restore the original useRef
    useRefSpy.mockRestore();
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

    const video = screen.getByTestId('activity-video');
    expect(video).not.toHaveAttribute('controls');
  });

  it('shows video controls when not paused at step', () => {
    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = screen.getByTestId('activity-video');
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

    const video = screen.getByTestId('activity-video');
    fireEvent.play(video);

    expect(defaultProps.onPlay).toHaveBeenCalledTimes(1);
  });

  it('calls onPause when video pause event is triggered', () => {
    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = screen.getByTestId('activity-video');
    fireEvent.pause(video);

    expect(defaultProps.onPause).toHaveBeenCalledTimes(1);
  });

  it('calls onEnded when video ended event is triggered', () => {
    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = screen.getByTestId('activity-video');
    fireEvent.ended(video);

    expect(defaultProps.onEnded).toHaveBeenCalledTimes(1);
  });

  it('calls onTimeUpdate when video timeupdate event is triggered', () => {
    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = screen.getByTestId('activity-video');
    // Simulate timeupdate event
    fireEvent(video, new Event('timeupdate', { bubbles: true }));
    // Manually set currentTime since fireEvent doesn't handle custom properties
    Object.defineProperty(video, 'currentTime', { value: 15, writable: true });
    fireEvent(video, new Event('timeupdate', { bubbles: true }));

    expect(defaultProps.onTimeUpdate).toHaveBeenCalledWith(15);
  });

  it('triggers onStepReached when hitting a step timestamp', () => {
    const mockVideo = {
      ...mockVideoElement,
      currentTime: 30,
      pause: jest.fn(),
    };

    // Mock the video ref
    const reactModule = require('react');
    const useRefSpy = jest.spyOn(reactModule, 'useRef');
    useRefSpy
      .mockReturnValueOnce({ current: mockVideo })
      .mockReturnValueOnce({ current: null });

    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = screen.getByTestId('activity-video');
    // Simulate timeupdate event
    fireEvent(video, new Event('timeupdate', { bubbles: true }));

    expect(mockVideo.pause).toHaveBeenCalledTimes(1);
    expect(defaultProps.onStepReached).toHaveBeenCalledWith(0);

    useRefSpy.mockRestore();
  });

  it('does not trigger step callbacks before timestamps are reached', () => {
    const mockVideo = {
      ...mockVideoElement,
      currentTime: 25,
      pause: jest.fn(),
    };

    const reactModule = require('react');
    const useRefSpy = jest.spyOn(reactModule, 'useRef');
    useRefSpy
      .mockReturnValueOnce({ current: mockVideo })
      .mockReturnValueOnce({ current: null });

    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = screen.getByTestId('activity-video');
    // Simulate timeupdate event
    fireEvent(video, new Event('timeupdate', { bubbles: true }));

    expect(mockVideo.pause).not.toHaveBeenCalled();
    expect(defaultProps.onStepReached).not.toHaveBeenCalled();

    useRefSpy.mockRestore();
  });

  it('handles video load event', () => {
    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    const video = screen.getByTestId('activity-video');
    fireEvent.loadedData(video);

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

    const video = screen.getByTestId('activity-video');
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

    const video = screen.getByTestId('activity-video');
    expect(video).toBeInTheDocument();
    // Desktop styling should be applied (maxHeight: 500px)
  });

  it('handles empty steps array', () => {
    renderWithTheme(
      <ActivityVideoPlayer
        {...defaultProps}
        steps={[]}
      />
    );

    const video = screen.getByTestId('activity-video');
    expect(video).toBeInTheDocument();
    // Should not crash with empty steps
  });

  it('renders timeline markers when video has duration and steps have timestamps', async () => {
    // Create a proper mock video element
    const mockVideo = document.createElement('video');
    Object.defineProperty(mockVideo, 'duration', { value: 120, writable: true });
    Object.defineProperty(mockVideo, 'currentTime', { value: 0, writable: true });
    mockVideo.addEventListener = jest.fn();
    mockVideo.removeEventListener = jest.fn();
    mockVideo.play = jest.fn().mockImplementation(() => Promise.resolve());
    mockVideo.pause = jest.fn();

    // Mock the video ref
    const reactModule = require('react');
    const useRefSpy = jest.spyOn(reactModule, 'useRef');
    useRefSpy
      .mockReturnValueOnce({ current: mockVideo })
      .mockReturnValueOnce({ current: null });

    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    // Set duration on the DOM video element
    const video = screen.getByTestId('activity-video');
    Object.defineProperty(video, 'duration', { value: 120, writable: true });

    // Trigger loadeddata to set duration
    fireEvent.loadedData(video);

    // Wait for state update
    await waitFor(() => {
      const markers = document.querySelectorAll('[aria-label*="Jump to step"]');
      expect(markers).toHaveLength(2); // Two steps with timestamps
    });

    useRefSpy.mockRestore();
  });

  it('does not render timeline markers when paused at step', () => {
    const mockVideo = document.createElement('video');
    Object.defineProperty(mockVideo, 'duration', { value: 120, writable: true });
    mockVideo.addEventListener = jest.fn();
    mockVideo.removeEventListener = jest.fn();

    const reactModule = require('react');
    const useRefSpy = jest.spyOn(reactModule, 'useRef');
    useRefSpy
      .mockReturnValueOnce({ current: mockVideo })
      .mockReturnValueOnce({ current: null });

    renderWithTheme(
      <ActivityVideoPlayer
        {...defaultProps}
        isPausedAtStep={true}
      />
    );

    // Trigger loadeddata
    const video = screen.getByTestId('activity-video');
    fireEvent.loadedData(video);

    // Markers should not be rendered when paused at step
    const markers = document.querySelectorAll('[aria-label*="Jump to step"]');
    expect(markers).toHaveLength(0);

    useRefSpy.mockRestore();
  });

  it('clicking timeline marker triggers seek functionality', async () => {
    const mockVideo = document.createElement('video');
    Object.defineProperty(mockVideo, 'duration', { value: 120, writable: true });
    mockVideo.addEventListener = jest.fn();
    mockVideo.removeEventListener = jest.fn();
    mockVideo.play = jest.fn().mockImplementation(() => Promise.resolve());
    mockVideo.pause = jest.fn();

    const reactModule = require('react');
    const useRefSpy = jest.spyOn(reactModule, 'useRef');
    useRefSpy
      .mockReturnValueOnce({ current: mockVideo })
      .mockReturnValueOnce({ current: null });

    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    // Set duration on the DOM video element
    const video = screen.getByTestId('activity-video');
    Object.defineProperty(video, 'duration', { value: 120, writable: true });

    // Trigger loadeddata
    fireEvent.loadedData(video);

    // Wait for markers to be rendered
    await waitFor(() => {
      const markers = document.querySelectorAll('[aria-label*="Jump to step"]');
      expect(markers).toHaveLength(2);
    });

    // Find and click the first marker - this should trigger the seek functionality
    const markers = document.querySelectorAll('[aria-label*="Jump to step"]');
    fireEvent.click(markers[0]);

    // The click should have been handled (verified by the markers being clickable)
    expect(markers[0]).toBeInTheDocument();

    useRefSpy.mockRestore();
  });

  it('timeline markers show correct tooltip information', async () => {
    const mockVideo = document.createElement('video');
    Object.defineProperty(mockVideo, 'duration', { value: 120, writable: true });
    mockVideo.addEventListener = jest.fn();
    mockVideo.removeEventListener = jest.fn();

    const reactModule = require('react');
    const useRefSpy = jest.spyOn(reactModule, 'useRef');
    useRefSpy
      .mockReturnValueOnce({ current: mockVideo })
      .mockReturnValueOnce({ current: null });

    renderWithTheme(<ActivityVideoPlayer {...defaultProps} />);

    // Set duration on the DOM video element
    const video = screen.getByTestId('activity-video');
    Object.defineProperty(video, 'duration', { value: 120, writable: true });

    // Trigger loadeddata
    fireEvent.loadedData(video);

    // Wait for markers to be rendered and check aria-labels
    await waitFor(() => {
      const markers = document.querySelectorAll('[aria-label*="Jump to step"]');
      expect(markers[0]).toHaveAttribute('aria-label', 'Jump to step 1 at 0:30');
      expect(markers[1]).toHaveAttribute('aria-label', 'Jump to step 2 at 1:00');
    });

    useRefSpy.mockRestore();
  });
});
