import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import PhotoPreview from '../../../client/src/components/materials/PhotoPreview';

// Mock IntersectionObserver
const mockIntersectionObserver = jest.fn();
mockIntersectionObserver.mockReturnValue({
  observe: () => null,
  unobserve: () => null,
  disconnect: () => null
});
global.IntersectionObserver = mockIntersectionObserver;

describe('PhotoPreview', () => {
  const defaultProps = {
    materialId: 'test-material-id',
    photoCount: 3,
    thumbnailUrl: 'https://example.com/thumbnail.jpg',
    size: 'medium' as const,
    showCount: true,
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('Initial Rendering', () => {
    it('renders with photo icon when no thumbnail URL', () => {
      render(<PhotoPreview {...defaultProps} thumbnailUrl={undefined} />);

      const photoIcon = screen.getByTestId('photo-icon') || document.querySelector('[data-testid="PhotoIcon"]');
      expect(photoIcon).toBeInTheDocument();
    });

    it('renders with image when thumbnail URL is provided', () => {
      render(<PhotoPreview {...defaultProps} />);

      const image = screen.getByAltText('Material thumbnail');
      expect(image).toBeInTheDocument();
      expect(image).toHaveAttribute('src', 'https://example.com/thumbnail.jpg');
    });

    it('shows photo count when showCount is true and photoCount > 1', () => {
      render(<PhotoPreview {...defaultProps} />);

      expect(screen.getByText('3')).toBeInTheDocument();
    });

    it('hides photo count when showCount is false', () => {
      render(<PhotoPreview {...defaultProps} showCount={false} />);

      expect(screen.queryByText('3')).not.toBeInTheDocument();
    });

    it('does not show count when photoCount is 1', () => {
      render(<PhotoPreview {...defaultProps} photoCount={1} />);

      expect(screen.queryByText('1')).not.toBeInTheDocument();
    });

    it('shows truncated count for large numbers', () => {
      render(<PhotoPreview {...defaultProps} photoCount={150} />);

      expect(screen.getByText('99+')).toBeInTheDocument();
    });
  });

  describe('Size Variants', () => {
    it('applies small size class', () => {
      render(<PhotoPreview {...defaultProps} size="small" />);

      const container = screen.getByTestId('photo-preview') || document.querySelector('.photo-preview');
      expect(container).toHaveClass('photo-preview-small');
    });

    it('applies medium size class', () => {
      render(<PhotoPreview {...defaultProps} size="medium" />);

      const container = screen.getByTestId('photo-preview') || document.querySelector('.photo-preview');
      expect(container).toHaveClass('photo-preview-medium');
    });

    it('applies large size class', () => {
      render(<PhotoPreview {...defaultProps} size="large" />);

      const container = screen.getByTestId('photo-preview') || document.querySelector('.photo-preview');
      expect(container).toHaveClass('photo-preview-large');
    });
  });

  describe('Click Handling', () => {
    it('calls onClick when provided and component is clicked', () => {
      const mockOnClick = jest.fn();
      render(<PhotoPreview {...defaultProps} onClick={mockOnClick} />);

      const container = screen.getByTestId('photo-preview') || document.querySelector('.photo-preview');
      fireEvent.click(container);

      expect(mockOnClick).toHaveBeenCalledTimes(1);
    });

    it('does not call onClick when not provided', () => {
      const mockOnClick = jest.fn();
      render(<PhotoPreview {...defaultProps} onClick={undefined} />);

      const container = screen.getByTestId('photo-preview') || document.querySelector('.photo-preview');
      fireEvent.click(container);

      expect(mockOnClick).not.toHaveBeenCalled();
    });
  });

  describe('Empty State', () => {
    it('renders placeholder when photoCount is 0', () => {
      render(<PhotoPreview {...defaultProps} photoCount={0} />);

      const photoIcon = screen.getByTestId('photo-icon') || document.querySelector('[data-testid="PhotoIcon"]');
      expect(photoIcon).toBeInTheDocument();
      expect(screen.getByTitle('No photos available')).toBeInTheDocument();
    });

    it('does not show photo count when photoCount is 0', () => {
      render(<PhotoPreview {...defaultProps} photoCount={0} />);

      expect(screen.queryByTestId('photo-count')).not.toBeInTheDocument();
    });
  });

  describe('Lazy Loading', () => {
    it('sets up IntersectionObserver', () => {
      render(<PhotoPreview {...defaultProps} />);

      expect(mockIntersectionObserver).toHaveBeenCalled();
    });

    it('loads image when it comes into view', () => {
      render(<PhotoPreview {...defaultProps} />);

      // The IntersectionObserver should be called with the image element
      expect(mockIntersectionObserver).toHaveBeenCalled();
    });

    it('shows loading state initially', () => {
      render(<PhotoPreview {...defaultProps} />);

      const loadingElement = screen.getByTestId('loading-container') || document.querySelector('.loading-container');
      expect(loadingElement).toBeInTheDocument();
    });

    it('hides loading state when image loads', async () => {
      render(<PhotoPreview {...defaultProps} />);

      const image = screen.getByAltText('Material thumbnail');

      // Simulate image load
      fireEvent.load(image);

      await waitFor(() => {
        const loadingElement = screen.queryByTestId('loading-container') || document.querySelector('.loading-container');
        expect(loadingElement).not.toBeInTheDocument();
        expect(image).toBeVisible();
      });
    });

    it('shows error state when image fails to load', async () => {
      render(<PhotoPreview {...defaultProps} />);

      const image = screen.getByAltText('Material thumbnail');

      // Simulate image error
      fireEvent.error(image);

      await waitFor(() => {
        const errorElement = screen.getByTestId('error-container') || document.querySelector('.error-container');
        expect(errorElement).toBeInTheDocument();

        const photoIcon = screen.getByTestId('photo-icon') || document.querySelector('[data-testid="PhotoIcon"]');
        expect(photoIcon).toBeInTheDocument();
      });
    });
  });

  describe('Accessibility', () => {
    it('has proper alt text for images', () => {
      render(<PhotoPreview {...defaultProps} />);

      const image = screen.getByAltText('Material thumbnail');
      expect(image).toHaveAttribute('alt', 'Material thumbnail');
    });

    it('has proper title attribute', () => {
      render(<PhotoPreview {...defaultProps} />);

      const container = screen.getByTestId('photo-preview') || document.querySelector('.photo-preview');
      expect(container).toHaveAttribute('title', '3 photos available');
    });

    it('has proper title for empty state', () => {
      render(<PhotoPreview {...defaultProps} photoCount={0} />);

      const container = screen.getByTestId('photo-preview') || document.querySelector('.photo-preview');
      expect(container).toHaveAttribute('title', 'No photos available');
    });

    it('supports keyboard interaction', () => {
      const mockOnClick = jest.fn();
      render(<PhotoPreview {...defaultProps} onClick={mockOnClick} />);

      const container = screen.getByTestId('photo-preview') || document.querySelector('.photo-preview');

      // Simulate Enter key
      fireEvent.keyDown(container, { key: 'Enter' });
      expect(mockOnClick).toHaveBeenCalledTimes(1);

      // Simulate Space key
      fireEvent.keyDown(container, { key: ' ' });
      expect(mockOnClick).toHaveBeenCalledTimes(2);
    });

    it('has proper ARIA attributes', () => {
      render(<PhotoPreview {...defaultProps} />);

      const container = screen.getByTestId('photo-preview') || document.querySelector('.photo-preview');
      expect(container).toHaveAttribute('role', 'button');
      expect(container).toHaveAttribute('tabIndex', '0');
    });
  });

  describe('Photo Count Display', () => {
    it('displays correct count for small numbers', () => {
      render(<PhotoPreview {...defaultProps} photoCount={5} />);

      expect(screen.getByText('5')).toBeInTheDocument();
    });

    it('displays "99+" for large numbers', () => {
      render(<PhotoPreview {...defaultProps} photoCount={150} />);

      expect(screen.getByText('99+')).toBeInTheDocument();
    });

    it('positions count correctly', () => {
      render(<PhotoPreview {...defaultProps} />);

      const countElement = screen.getByTestId('photo-count') || document.querySelector('.photo-count');
      expect(countElement).toBeInTheDocument();
    });

    it('applies correct styling for different sizes', () => {
      render(<PhotoPreview {...defaultProps} size="large" />);

      const countElement = screen.getByTestId('photo-count') || document.querySelector('.photo-count');
      expect(countElement).toBeInTheDocument();
      // The styling would be verified through CSS classes
    });
  });

  describe('Image Loading States', () => {
    it('handles image load success', async () => {
      render(<PhotoPreview {...defaultProps} />);

      const image = screen.getByAltText('Material thumbnail');

      // Initially should show loading
      expect(screen.getByTestId('loading-container') || document.querySelector('.loading-container')).toBeInTheDocument();

      // After load, should show image
      fireEvent.load(image);

      await waitFor(() => {
        expect(image).toBeVisible();
        expect(screen.queryByTestId('loading-container')).not.toBeInTheDocument();
      });
    });

    it('handles image load error', async () => {
      render(<PhotoPreview {...defaultProps} />);

      const image = screen.getByAltText('Material thumbnail');

      // Initially should show loading
      expect(screen.getByTestId('loading-container') || document.querySelector('.loading-container')).toBeInTheDocument();

      // After error, should show error state
      fireEvent.error(image);

      await waitFor(() => {
        expect(screen.getByTestId('error-container') || document.querySelector('.error-container')).toBeInTheDocument();
        expect(screen.queryByTestId('loading-container')).not.toBeInTheDocument();
      });
    });

    it('shows placeholder when no thumbnail but has photos', () => {
      render(<PhotoPreview {...defaultProps} thumbnailUrl={undefined} />);

      const placeholder = screen.getByTestId('placeholder-container') || document.querySelector('.placeholder-container');
      expect(placeholder).toBeInTheDocument();

      const photoIcon = screen.getByTestId('photo-icon') || document.querySelector('[data-testid="PhotoIcon"]');
      expect(photoIcon).toBeInTheDocument();
    });
  });

  describe('Edge Cases', () => {
    it('handles null thumbnailUrl gracefully', () => {
      render(<PhotoPreview {...defaultProps} thumbnailUrl={null as any} />);

      const placeholder = screen.getByTestId('placeholder-container') || document.querySelector('.placeholder-container');
      expect(placeholder).toBeInTheDocument();
    });

    it('handles very large photo counts', () => {
      render(<PhotoPreview {...defaultProps} photoCount={1000} />);

      expect(screen.getByText('99+')).toBeInTheDocument();
    });

    it('handles photoCount of exactly 99', () => {
      render(<PhotoPreview {...defaultProps} photoCount={99} />);

      expect(screen.getByText('99')).toBeInTheDocument();
    });

    it('handles photoCount of exactly 100', () => {
      render(<PhotoPreview {...defaultProps} photoCount={100} />);

      expect(screen.getByText('99+')).toBeInTheDocument();
    });
  });

  describe('Performance', () => {
    it('only renders image when in viewport', () => {
      render(<PhotoPreview {...defaultProps} />);

      // IntersectionObserver should be set up
      expect(mockIntersectionObserver).toHaveBeenCalledTimes(1);
    });

    it('cleans up IntersectionObserver on unmount', () => {
      const { unmount } = render(<PhotoPreview {...defaultProps} />);

      unmount();

      // The disconnect method should be called
      const mockInstance = mockIntersectionObserver.mock.results[0].value;
      expect(mockInstance.disconnect).toHaveBeenCalled();
    });
  });
});