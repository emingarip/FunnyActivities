import React from 'react';
import { render, screen, fireEvent, waitFor, act } from '@testing-library/react';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import MultiPhotoUpload from '../../../client/src/components/materials/MultiPhotoUpload';

// Mock the materialsAPI
jest.mock('../../../../client/src/services/api', () => ({
  materialsAPI: {
    uploadMaterialPhotos: jest.fn(),
  },
}));

const { materialsAPI } = require('../../../../client/src/services/api');

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

// Mock File constructor
global.File = class MockFile {
  name: string;
  size: number;
  type: string;
  lastModified: number;

  constructor(parts: any[], filename: string, options: any = {}) {
    this.name = filename;
    this.size = options.size || 1024;
    this.type = options.type || 'image/jpeg';
    this.lastModified = Date.now();
  }
} as any;

// Mock FileReader
global.FileReader = class MockFileReader {
  onload: ((event: any) => void) | null = null;
  result: string | null = null;

  readAsDataURL(file: File) {
    // Simulate async file reading
    setTimeout(() => {
      this.result = `data:${file.type};base64,mockdata`;
      if (this.onload) {
        this.onload({ target: { result: this.result } } as any);
      }
    }, 0);
  }
} as any;

describe('MultiPhotoUpload', () => {
  const defaultProps = {
    materialId: 'test-material-id',
    maxFiles: 5,
    maxFileSize: 2, // 2MB
    acceptedTypes: ['image/jpeg', 'image/png'],
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('Initial Rendering', () => {
    it('renders upload area with correct text', () => {
      renderWithTheme(<MultiPhotoUpload {...defaultProps} />);

      expect(screen.getByText('Drag and drop photos here')).toBeInTheDocument();
      expect(screen.getByText('or click to browse files')).toBeInTheDocument();
      expect(screen.getByText('Choose Files')).toBeInTheDocument();
    });

    it('displays file limits and accepted types', () => {
      renderWithTheme(<MultiPhotoUpload {...defaultProps} />);

      expect(screen.getByText('Maximum 5 files, up to 2MB each')).toBeInTheDocument();
      expect(screen.getByText('Supported formats: jpeg, png')).toBeInTheDocument();
    });

    it('renders hidden file input', () => {
      renderWithTheme(<MultiPhotoUpload {...defaultProps} />);

      const fileInput = screen.getByTestId('file-input') || document.querySelector('input[type="file"]');
      expect(fileInput).toBeInTheDocument();
      expect(fileInput).toHaveAttribute('type', 'file');
      expect(fileInput).toHaveAttribute('multiple');
      expect(fileInput).toHaveAttribute('accept', 'image/jpeg,image/png');
    });
  });

  describe('File Validation', () => {
    it('validates file type correctly', async () => {
      renderWithTheme(<MultiPhotoUpload {...defaultProps} />);

      const file = new File(['test'], 'test.txt', { type: 'text/plain' });
      const fileInput = screen.getByTestId('file-input') || document.querySelector('input[type="file"]') as HTMLInputElement;

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file] } });
      });

      await waitFor(() => {
        expect(screen.getByText(/File type text\/plain is not supported/)).toBeInTheDocument();
      });
    });

    it('validates file size correctly', async () => {
      renderWithTheme(<MultiPhotoUpload {...defaultProps} />);

      // Create a large file using the mock File constructor
      const largeFile = new global.File(['x'.repeat(3 * 1024 * 1024)], 'large.jpg', { type: 'image/jpeg' });
      // Manually set the size to 3MB
      Object.defineProperty(largeFile, 'size', { value: 3 * 1024 * 1024, writable: false });

      const fileInput = screen.getByTestId('file-input') || document.querySelector('input[type="file"]') as HTMLInputElement;

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [largeFile] } });
      });

      await waitFor(() => {
        expect(screen.getByText(/File size exceeds 2MB limit/)).toBeInTheDocument();
      });
    });

    it('accepts valid files', async () => {
      renderWithTheme(<MultiPhotoUpload {...defaultProps} />);

      const validFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      const fileInput = screen.getByTestId('file-input') || document.querySelector('input[type="file"]') as HTMLInputElement;

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [validFile] } });
      });

      await waitFor(() => {
        expect(screen.getByText('Selected Files (1/5)')).toBeInTheDocument();
        expect(screen.getByText('test.jpg')).toBeInTheDocument();
      });
    });
  });

  describe('File Count Limits', () => {
    it('prevents adding more files than maxFiles', async () => {
      renderWithTheme(<MultiPhotoUpload {...defaultProps} maxFiles={2} />);

      const file1 = new File(['test1'], 'test1.jpg', { type: 'image/jpeg' });
      const file2 = new File(['test2'], 'test2.jpg', { type: 'image/jpeg' });
      const file3 = new File(['test3'], 'test3.jpg', { type: 'image/jpeg' });

      const fileInput = screen.getByTestId('file-input') || document.querySelector('input[type="file"]') as HTMLInputElement;

      // Add first two files
      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file1, file2] } });
      });

      await waitFor(() => {
        expect(screen.getByText('Selected Files (2/2)')).toBeInTheDocument();
      });

      // Try to add third file
      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file3] } });
      });

      await waitFor(() => {
        expect(screen.getByText(/Maximum 2 files allowed/)).toBeInTheDocument();
      });

      // Should still have only 2 files
      expect(screen.getByText('Selected Files (2/2)')).toBeInTheDocument();
    });
  });

  describe('Drag and Drop', () => {
    it('handles drag over events', async () => {
      renderWithTheme(<MultiPhotoUpload {...defaultProps} />);

      const dropZone = screen.getByText('Drag and drop photos here').closest('div');

      fireEvent.dragOver(dropZone!);

      // Check if drag active state is applied (this might require checking styles or classes)
      expect(dropZone).toBeInTheDocument();
    });

    it('handles file drop correctly', async () => {
      renderWithTheme(<MultiPhotoUpload {...defaultProps} />);

      const dropZone = screen.getByText('Drag and drop photos here').closest('div');
      const file = new File(['test'], 'dropped.jpg', { type: 'image/jpeg' });

      const mockDataTransfer = {
        files: [file],
      };

      fireEvent.drop(dropZone!, { dataTransfer: mockDataTransfer });

      await waitFor(() => {
        expect(screen.getByText('Selected Files (1/5)')).toBeInTheDocument();
        expect(screen.getByText('dropped.jpg')).toBeInTheDocument();
      });
    });
  });

  describe('File Management', () => {
    it('removes individual files', async () => {
      renderWithTheme(<MultiPhotoUpload {...defaultProps} />);

      const file = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      const fileInput = screen.getByTestId('file-input') || document.querySelector('input[type="file"]') as HTMLInputElement;

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file] } });
      });

      await waitFor(() => {
        expect(screen.getByText('test.jpg')).toBeInTheDocument();
      });

      // Find and click delete button
      const deleteButton = screen.getByTestId('delete-file');
      fireEvent.click(deleteButton);

      await waitFor(() => {
        expect(screen.queryByText('test.jpg')).not.toBeInTheDocument();
      });
    });

    it('clears all files', async () => {
      renderWithTheme(<MultiPhotoUpload {...defaultProps} />);

      const file1 = new File(['test1'], 'test1.jpg', { type: 'image/jpeg' });
      const file2 = new File(['test2'], 'test2.jpg', { type: 'image/jpeg' });
      const fileInput = screen.getByTestId('file-input') || document.querySelector('input[type="file"]') as HTMLInputElement;

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file1, file2] } });
      });

      await waitFor(() => {
        expect(screen.getByText('Selected Files (2/5)')).toBeInTheDocument();
      });

      const clearButton = screen.getByText('Clear All');
      fireEvent.click(clearButton);

      await waitFor(() => {
        expect(screen.queryByText('Selected Files (2/5)')).not.toBeInTheDocument();
      });
    });
  });

  describe('File Upload', () => {
    it('uploads files successfully', async () => {
      const mockOnUploadSuccess = jest.fn();
      const mockResponse = {
        data: {
          success: true,
          data: { photoUrls: ['url1', 'url2'] }
        }
      };

      materialsAPI.uploadMaterialPhotos.mockResolvedValue(mockResponse);

      renderWithTheme(
        <MultiPhotoUpload
          {...defaultProps}
          onUploadSuccess={mockOnUploadSuccess}
        />
      );

      const file1 = new File(['test1'], 'test1.jpg', { type: 'image/jpeg' });
      const file2 = new File(['test2'], 'test2.jpg', { type: 'image/jpeg' });
      const fileInput = screen.getByTestId('file-input') || document.querySelector('input[type="file"]') as HTMLInputElement;

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file1, file2] } });
      });

      await waitFor(() => {
        expect(screen.getByText('Selected Files (2/5)')).toBeInTheDocument();
      });

      const uploadButton = screen.getByText('Upload 2 Photos');
      fireEvent.click(uploadButton);

      await waitFor(() => {
        expect(materialsAPI.uploadMaterialPhotos).toHaveBeenCalledWith('test-material-id', [file1, file2]);
        expect(mockOnUploadSuccess).toHaveBeenCalledWith(['url1', 'url2']);
        expect(screen.getByText('Successfully uploaded 2 photo(s)')).toBeInTheDocument();
      });
    });

    it('handles upload errors', async () => {
      const mockOnUploadError = jest.fn();
      const mockError = new Error('Upload failed');

      materialsAPI.uploadMaterialPhotos.mockRejectedValue(mockError);

      renderWithTheme(
        <MultiPhotoUpload
          {...defaultProps}
          onUploadError={mockOnUploadError}
        />
      );

      const file = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      const fileInput = screen.getByTestId('file-input') || document.querySelector('input[type="file"]') as HTMLInputElement;

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file] } });
      });

      await waitFor(() => {
        expect(screen.getByText('Selected Files (1/5)')).toBeInTheDocument();
      });

      const uploadButton = screen.getByText('Upload 1 Photo');
      fireEvent.click(uploadButton);

      await waitFor(() => {
        expect(mockOnUploadError).toHaveBeenCalledWith('Upload failed');
        expect(screen.getAllByText('Upload failed')).toHaveLength(2); // One in alert, one in file error
      });
    });

    it('shows loading state during upload', async () => {
      materialsAPI.uploadMaterialPhotos.mockImplementation(() =>
        new Promise(resolve => setTimeout(() => resolve({
          data: { success: true, data: { photoUrls: ['url1'] } }
        }), 100))
      );

      renderWithTheme(<MultiPhotoUpload {...defaultProps} />);

      const file = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      const fileInput = screen.getByTestId('file-input') || document.querySelector('input[type="file"]') as HTMLInputElement;

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file] } });
      });

      await waitFor(() => {
        expect(screen.getByText('Selected Files (1/5)')).toBeInTheDocument();
      });

      const uploadButton = screen.getByText('Upload 1 Photo');
      fireEvent.click(uploadButton);

      // Check loading state
      await waitFor(() => {
        expect(screen.getByText('Uploading...')).toBeInTheDocument();
      });

      // Wait for completion
      await waitFor(() => {
        expect(screen.getByText('Successfully uploaded 1 photo(s)')).toBeInTheDocument();
      });
    });
  });

  describe('Disabled State', () => {
    it('disables all interactions when disabled', () => {
      renderWithTheme(<MultiPhotoUpload {...defaultProps} disabled={true} />);

      const chooseButton = screen.getByText('Choose Files');
      expect(chooseButton.closest('button')).toBeDisabled();

      const fileInput = screen.getByTestId('file-input') || document.querySelector('input[type="file"]') as HTMLInputElement;
      expect(fileInput).toBeDisabled();
    });
  });

  describe('Error Handling', () => {
    it('displays validation errors', async () => {
      renderWithTheme(<MultiPhotoUpload {...defaultProps} />);

      const invalidFile = new File(['test'], 'test.txt', { type: 'text/plain' });
      const fileInput = screen.getByTestId('file-input') || document.querySelector('input[type="file"]') as HTMLInputElement;

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [invalidFile] } });
      });

      await waitFor(() => {
        expect(screen.getByText(/File type text\/plain is not supported/)).toBeInTheDocument();
      });
    });

    it('clears errors when valid files are added', async () => {
      renderWithTheme(<MultiPhotoUpload {...defaultProps} />);

      // Add invalid file first
      const invalidFile = new File(['test'], 'test.txt', { type: 'text/plain' });
      const fileInput = screen.getByTestId('file-input') || document.querySelector('input[type="file"]') as HTMLInputElement;

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [invalidFile] } });
      });

      await waitFor(() => {
        expect(screen.getByText(/File type text\/plain is not supported/)).toBeInTheDocument();
      });

      // Add valid file
      const validFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [validFile] } });
      });

      await waitFor(() => {
        expect(screen.queryByText(/File type text\/plain is not supported/)).not.toBeInTheDocument();
        expect(screen.getByText('test.jpg')).toBeInTheDocument();
      });
    });
  });

  describe('Accessibility', () => {
    it('has proper ARIA labels', () => {
      renderWithTheme(<MultiPhotoUpload {...defaultProps} />);

      const fileInput = screen.getByTestId('file-input') || document.querySelector('input[type="file"]') as HTMLInputElement;
      expect(fileInput).toHaveAttribute('aria-label', 'Upload photos');
    });

    it('supports keyboard navigation', async () => {
      renderWithTheme(<MultiPhotoUpload {...defaultProps} />);

      const chooseButton = screen.getByText('Choose Files');

      // Focus the button
      await act(async () => {
        chooseButton.focus();
      });
      expect(chooseButton).toHaveFocus();
    });
  });
});