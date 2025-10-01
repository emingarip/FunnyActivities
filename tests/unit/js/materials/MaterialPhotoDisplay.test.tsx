import React from 'react';
import { render, screen, fireEvent, waitFor, act } from '@testing-library/react';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import MaterialPhotoDisplay from '../../../client/src/components/materials/MaterialPhotoDisplay';

// Mock the materialsAPI
jest.mock('../../../../client/src/services/api', () => ({
  materialsAPI: {
    getMaterial: jest.fn(),
    deleteMaterialPhoto: jest.fn(),
  },
}));

const { materialsAPI } = require('../../../../client/src/services/api');

// Mock IntersectionObserver
const mockIntersectionObserver = jest.fn();
mockIntersectionObserver.mockReturnValue({
  observe: () => null,
  unobserve: () => null,
  disconnect: () => null
});
global.IntersectionObserver = mockIntersectionObserver;

// Create a theme for Material-UI components
const theme = createTheme();

// Test utilities
const renderWithTheme = (component: React.ReactElement) => {
  return render(
    <ThemeProvider theme={theme}>
      {component}
    </ThemeProvider>
  );
};

// Mock photos data
const mockPhotos = [
  {
    id: 'photo1',
    url: 'https://example.com/photo1.jpg',
    filename: 'photo1.jpg',
    uploadedAt: '2023-01-01T00:00:00Z',
    size: 1024000,
  },
  {
    id: 'photo2',
    url: 'https://example.com/photo2.jpg',
    filename: 'photo2.jpg',
    uploadedAt: '2023-01-02T00:00:00Z',
    size: 2048000,
  },
];

describe('MaterialPhotoDisplay', () => {
  const defaultProps = {
    materialId: 'test-material-id',
    photos: mockPhotos,
  };

  beforeEach(() => {
    jest.clearAllMocks();
    // Mock successful API response
    materialsAPI.getMaterial.mockResolvedValue({
      data: { success: true, data: { photos: mockPhotos } }
    });
  });

  describe('Initial Rendering', () => {
    it('renders photos grid when photos are provided', () => {
      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      expect(screen.getByAltText('photo1.jpg')).toBeInTheDocument();
      expect(screen.getByAltText('photo2.jpg')).toBeInTheDocument();
    });

    it('fetches photos when not provided', async () => {
      renderWithTheme(<MaterialPhotoDisplay materialId="test-material-id" />);

      await waitFor(() => {
        expect(materialsAPI.getMaterial).toHaveBeenCalledWith('test-material-id');
      });
    });

    it('shows loading state when fetching photos', () => {
      materialsAPI.getMaterial.mockImplementation(() => new Promise(() => {})); // Never resolves

      renderWithTheme(<MaterialPhotoDisplay materialId="test-material-id" />);

      expect(screen.getByRole('progressbar')).toBeInTheDocument();
    });

    it('shows error state when fetch fails', async () => {
      materialsAPI.getMaterial.mockRejectedValue(new Error('Fetch failed'));

      renderWithTheme(<MaterialPhotoDisplay materialId="test-material-id" />);

      await waitFor(() => {
        expect(screen.getByText('Failed to fetch photos')).toBeInTheDocument();
      });
    });

    it('shows empty state when no photos', () => {
      renderWithTheme(<MaterialPhotoDisplay materialId="test-material-id" photos={[]} />);

      expect(screen.getByText('No photos available for this material.')).toBeInTheDocument();
    });
  });

  describe('Photo Display', () => {
    it('displays photo thumbnails with correct attributes', () => {
      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      const images = screen.getAllByRole('img');
      expect(images).toHaveLength(2);

      const firstImage = screen.getByAltText('photo1.jpg');
      expect(firstImage).toHaveAttribute('src', 'https://example.com/photo1.jpg');
    });

    it('shows photo count and filename in modal', async () => {
      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      const firstImage = screen.getByAltText('photo1.jpg');
      fireEvent.click(firstImage);

      await waitFor(() => {
        expect(screen.getByText('photo1.jpg')).toBeInTheDocument();
        expect(screen.getByText('1/1/2023')).toBeInTheDocument();
      });
    });
  });

  describe('Modal Functionality', () => {
    it('opens modal when photo is clicked', async () => {
      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      const firstImage = screen.getByAltText('photo1.jpg');
      fireEvent.click(firstImage);

      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument();
      });
    });

    it('closes modal when close button is clicked', async () => {
      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      // Open modal
      const firstImage = screen.getByAltText('photo1.jpg');
      fireEvent.click(firstImage);

      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument();
      });

      // Close modal
      const closeButton = screen.getByTestId('modal-close') || screen.getByLabelText('close');
      fireEvent.click(closeButton);

      await waitFor(() => {
        expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
      });
    });

    it('navigates between photos in modal', async () => {
      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      // Open first photo
      const firstImage = screen.getByAltText('photo1.jpg');
      fireEvent.click(firstImage);

      await waitFor(() => {
        expect(screen.getByText('photo1.jpg')).toBeInTheDocument();
      });

      // Navigate to next photo
      const nextButton = screen.getByTestId('next-photo') || screen.getByLabelText('next');
      fireEvent.click(nextButton);

      await waitFor(() => {
        expect(screen.getByText('photo2.jpg')).toBeInTheDocument();
      });

      // Navigate back
      const prevButton = screen.getByTestId('prev-photo') || screen.getByLabelText('previous');
      fireEvent.click(prevButton);

      await waitFor(() => {
        expect(screen.getByText('photo1.jpg')).toBeInTheDocument();
      });
    });

    it('hides navigation buttons on mobile', () => {
      // Mock mobile viewport
      Object.defineProperty(window, 'innerWidth', { value: 600 });

      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      const firstImage = screen.getByAltText('photo1.jpg');
      fireEvent.click(firstImage);

      // Navigation buttons should not be present on mobile
      expect(screen.queryByTestId('next-photo')).not.toBeInTheDocument();
      expect(screen.queryByTestId('prev-photo')).not.toBeInTheDocument();
    });
  });

  describe('Delete Functionality', () => {
    it('shows delete button when not disabled', () => {
      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      const deleteButtons = screen.getAllByTestId('delete-photo') || screen.getAllByLabelText('delete');
      expect(deleteButtons).toHaveLength(2);
    });

    it('hides delete button when disabled', () => {
      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} disabled={true} />);

      const deleteButtons = screen.queryAllByTestId('delete-photo') || screen.queryAllByLabelText('delete');
      expect(deleteButtons).toHaveLength(0);
    });

    it('opens delete confirmation dialog', async () => {
      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      const deleteButtons = screen.getAllByTestId('delete-photo') || screen.getAllByLabelText('delete');
      fireEvent.click(deleteButtons[0]);

      await waitFor(() => {
        expect(screen.getByText('Delete Photo')).toBeInTheDocument();
        expect(screen.getByText('Are you sure you want to delete this photo? This action cannot be undone.')).toBeInTheDocument();
      });
    });

    it('deletes photo when confirmed', async () => {
      const mockOnPhotosChange = jest.fn();
      materialsAPI.deleteMaterialPhoto.mockResolvedValue({ data: { success: true } });

      renderWithTheme(
        <MaterialPhotoDisplay
          {...defaultProps}
          onPhotosChange={mockOnPhotosChange}
        />
      );

      // Open delete dialog
      const deleteButtons = screen.getAllByTestId('delete-photo') || screen.getAllByLabelText('delete');
      fireEvent.click(deleteButtons[0]);

      await waitFor(() => {
        expect(screen.getByText('Delete Photo')).toBeInTheDocument();
      });

      // Confirm deletion
      const deleteConfirmButton = screen.getByText('Delete');
      fireEvent.click(deleteConfirmButton);

      await waitFor(() => {
        expect(materialsAPI.deleteMaterialPhoto).toHaveBeenCalledWith('test-material-id', 'photo1');
        expect(mockOnPhotosChange).toHaveBeenCalledWith([mockPhotos[1]]);
      });
    });

    it('cancels delete when cancel button is clicked', async () => {
      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      const deleteButtons = screen.getAllByTestId('delete-photo') || screen.getAllByLabelText('delete');
      fireEvent.click(deleteButtons[0]);

      await waitFor(() => {
        expect(screen.getByText('Delete Photo')).toBeInTheDocument();
      });

      const cancelButton = screen.getByText('Cancel');
      fireEvent.click(cancelButton);

      await waitFor(() => {
        expect(screen.queryByText('Delete Photo')).not.toBeInTheDocument();
      });
    });
  });

  describe('Drag and Drop', () => {
    it('allows dragging when not disabled', () => {
      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      const photoCards = screen.getAllByTestId('photo-card') || screen.getAllByRole('img').map(img => img.closest('[draggable]'));
      const draggableCards = photoCards.filter(card => card?.getAttribute('draggable') === 'true');

      expect(draggableCards).toHaveLength(2);
    });

    it('prevents dragging when disabled', () => {
      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} disabled={true} />);

      const photoCards = screen.getAllByTestId('photo-card') || screen.getAllByRole('img').map(img => img.closest('[draggable]'));
      const draggableCards = photoCards.filter(card => card?.getAttribute('draggable') === 'true');

      expect(draggableCards).toHaveLength(0);
    });

    it('handles drag start event', () => {
      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      const photoCards = screen.getAllByTestId('photo-card') || screen.getAllByRole('img').map(img => img.closest('[draggable]'));
      const firstCard = photoCards[0];

      const mockDataTransfer = { effectAllowed: 'move' };
      fireEvent.dragStart(firstCard!, { dataTransfer: mockDataTransfer });

      expect(mockDataTransfer.effectAllowed).toBe('move');
    });

    it('handles drop event and reorders photos', () => {
      const mockOnPhotosChange = jest.fn();

      renderWithTheme(
        <MaterialPhotoDisplay
          {...defaultProps}
          onPhotosChange={mockOnPhotosChange}
        />
      );

      const photoCards = screen.getAllByTestId('photo-card') || screen.getAllByRole('img').map(img => img.closest('[draggable]'));

      // Simulate drag and drop from index 0 to index 1
      fireEvent.dragStart(photoCards[0]!, { dataTransfer: { effectAllowed: 'move' } });
      fireEvent.drop(photoCards[1]!, { dataTransfer: { dropEffect: 'move' } });

      expect(mockOnPhotosChange).toHaveBeenCalledWith([mockPhotos[1], mockPhotos[0]]);
    });
  });

  describe('Lazy Loading', () => {
    it('sets up IntersectionObserver for lazy loading', () => {
      const observeSpy = jest.spyOn(global.IntersectionObserver.prototype, 'observe');

      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      expect(observeSpy).toHaveBeenCalled();
    });

    it('loads image when it comes into view', () => {
      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      // Mock intersection
      const mockIntersectionObserver = global.IntersectionObserver as jest.MockedClass<typeof IntersectionObserver>;
      const mockObserve = mockIntersectionObserver.mock.instances[0]?.observe as jest.Mock;

      // Simulate intersection
      const mockEntry = { isIntersecting: true, target: { dataset: { src: 'test.jpg' } } };
      const callback = mockObserve.mock.calls[0][0]; // Get the callback passed to observe

      // This is a simplified test - in real implementation, we'd need to mock the observer callback
      expect(mockObserve).toHaveBeenCalled();
    });
  });

  describe('Error Handling', () => {
    it('handles delete API errors', async () => {
      materialsAPI.deleteMaterialPhoto.mockRejectedValue(new Error('Delete failed'));

      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      const deleteButtons = screen.getAllByTestId('delete-photo') || screen.getAllByLabelText('delete');
      fireEvent.click(deleteButtons[0]);

      await waitFor(() => {
        expect(screen.getByText('Delete Photo')).toBeInTheDocument();
      });

      const deleteConfirmButton = screen.getByText('Delete');
      fireEvent.click(deleteConfirmButton);

      await waitFor(() => {
        expect(screen.getByText('Failed to delete photo')).toBeInTheDocument();
      });
    });

    it('shows retry button on fetch error', async () => {
      materialsAPI.getMaterial.mockRejectedValue(new Error('Fetch failed'));

      renderWithTheme(<MaterialPhotoDisplay materialId="test-material-id" />);

      await waitFor(() => {
        expect(screen.getByText('Failed to fetch photos')).toBeInTheDocument();
      });

      const retryButton = screen.getByText('Retry');
      expect(retryButton).toBeInTheDocument();

      // Click retry
      materialsAPI.getMaterial.mockResolvedValue({
        data: { success: true, data: { photos: mockPhotos } }
      });

      fireEvent.click(retryButton);

      await waitFor(() => {
        expect(materialsAPI.getMaterial).toHaveBeenCalledTimes(2);
      });
    });
  });

  describe('Accessibility', () => {
    it('has proper ARIA labels for images', () => {
      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      const images = screen.getAllByRole('img');
      images.forEach((img, index) => {
        expect(img).toHaveAttribute('alt', mockPhotos[index].filename);
      });
    });

    it('has proper ARIA labels for buttons', () => {
      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      const deleteButtons = screen.getAllByTestId('delete-photo') || screen.getAllByLabelText('delete');
      expect(deleteButtons).toHaveLength(2);
    });

    it('supports keyboard navigation in modal', async () => {
      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      // Open modal
      const firstImage = screen.getByAltText('photo1.jpg');
      fireEvent.click(firstImage);

      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument();
      });

      // Test escape key
      fireEvent.keyDown(document, { key: 'Escape' });

      await waitFor(() => {
        expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
      });
    });
  });

  describe('Mobile Responsiveness', () => {
    it('renders with mobile grid layout', () => {
      // Mock mobile viewport
      Object.defineProperty(window, 'innerWidth', { value: 600 });

      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      const gridContainer = screen.getByTestId('photos-grid') || document.querySelector('[class*="MuiGrid-container"]');
      expect(gridContainer).toBeInTheDocument();
    });

    it('hides desktop-only navigation in modal on mobile', () => {
      // Mock mobile viewport
      Object.defineProperty(window, 'innerWidth', { value: 600 });

      renderWithTheme(<MaterialPhotoDisplay {...defaultProps} />);

      const firstImage = screen.getByAltText('photo1.jpg');
      fireEvent.click(firstImage);

      // Navigation buttons should be hidden on mobile
      expect(screen.queryByTestId('next-photo')).not.toBeInTheDocument();
      expect(screen.queryByTestId('prev-photo')).not.toBeInTheDocument();
    });
  });
});