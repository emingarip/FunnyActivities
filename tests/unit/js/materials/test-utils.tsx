import React from 'react';
import { render, screen } from '@testing-library/react';
import { ThemeProvider, createTheme } from '@mui/material/styles';

// Create a theme for Material-UI components
export const theme = createTheme();

// Test utilities
export const renderWithTheme = (component: React.ReactElement) => {
  return render(
    <ThemeProvider theme={theme}>
      {component}
    </ThemeProvider>
  );
};

// Mock File constructor
export const createMockFile = (
  name: string = 'test.jpg',
  size: number = 1024,
  type: string = 'image/jpeg'
): File => {
  const file = new File(['test'], name, { type });
  // Manually set the size property
  Object.defineProperty(file, 'size', { value: size, writable: false });
  return file;
};

// Mock multiple files
export const createMockFiles = (count: number = 2): File[] => {
  return Array.from({ length: count }, (_, index) =>
    createMockFile(`test${index + 1}.jpg`, 1024 * (index + 1), 'image/jpeg')
  );
};

// Mock drag event
export const createMockDragEvent = (type: string, files: File[] = []): React.DragEvent => {
  return {
    type,
    preventDefault: jest.fn(),
    stopPropagation: jest.fn(),
    dataTransfer: {
      files,
      dropEffect: 'copy',
      effectAllowed: 'copy',
    },
  } as any;
};

// Mock intersection observer entry
export const createMockIntersectionObserverEntry = (
  isIntersecting: boolean = true,
  target: Element = document.createElement('div')
): IntersectionObserverEntry => {
  return {
    isIntersecting,
    target,
    boundingClientRect: target.getBoundingClientRect(),
    intersectionRatio: isIntersecting ? 1 : 0,
    intersectionRect: target.getBoundingClientRect(),
    rootBounds: null,
    time: Date.now(),
  };
};

// Mock photo data
export const mockPhoto = {
  id: 'photo1',
  url: 'https://example.com/photo1.jpg',
  filename: 'photo1.jpg',
  uploadedAt: '2023-01-01T00:00:00Z',
  size: 1024000,
};

export const mockPhotos = [
  mockPhoto,
  {
    id: 'photo2',
    url: 'https://example.com/photo2.jpg',
    filename: 'photo2.jpg',
    uploadedAt: '2023-01-02T00:00:00Z',
    size: 2048000,
  },
];

// Mock API responses
export const mockApiResponse = {
  success: true,
  data: mockPhotos,
  message: 'Success',
};

export const mockUploadResponse = {
  success: true,
  data: { photoUrls: ['url1.jpg', 'url2.jpg'] },
  message: 'Photos uploaded successfully',
};

export const mockErrorResponse = {
  success: false,
  message: 'An error occurred',
  error: 'TestError',
};

// Mock axios error
export const createMockAxiosError = (status: number = 400, message: string = 'Error') => ({
  response: {
    status,
    data: { success: false, message },
  },
  message,
});

// Utility to wait for async operations
export const waitForAsync = () => new Promise(resolve => setTimeout(resolve, 0));

// Mock localStorage
export const mockLocalStorage = () => {
  const store: { [key: string]: string } = {};

  return {
    getItem: (key: string) => store[key] || null,
    setItem: (key: string, value: string) => {
      store[key] = value;
    },
    removeItem: (key: string) => {
      delete store[key];
    },
    clear: () => {
      Object.keys(store).forEach(key => delete store[key]);
    },
  };
};

// Mock window resize for responsive tests
export const mockWindowResize = (width: number) => {
  Object.defineProperty(window, 'innerWidth', {
    writable: true,
    configurable: true,
    value: width,
  });

  window.dispatchEvent(new Event('resize'));
};

// Test IDs for consistent element selection
export const TEST_IDS = {
  fileInput: 'file-input',
  uploadButton: 'upload-button',
  deleteButton: 'delete-photo',
  photoCard: 'photo-card',
  photoPreview: 'photo-preview',
  photoCount: 'photo-count',
  loadingContainer: 'loading-container',
  errorContainer: 'error-container',
  placeholderContainer: 'placeholder-container',
  photoIcon: 'photo-icon',
  modalClose: 'modal-close',
  nextPhoto: 'next-photo',
  prevPhoto: 'prev-photo',
  deleteFile: 'delete-file',
} as const;

// Common test patterns
export const commonTests = {
  // Test component renders without crashing
  rendersWithoutCrashing: (component: React.ReactElement) => {
    it('renders without crashing', () => {
      expect(() => renderWithTheme(component)).not.toThrow();
    });
  },

  // Test component handles loading state
  handlesLoadingState: (component: React.ReactElement, loadingElement?: string) => {
    it('shows loading state', () => {
      renderWithTheme(component);
      const loader = loadingElement
        ? document.querySelector(loadingElement)
        : screen.getByTestId('loading-container') || document.querySelector('.loading-container');

      expect(loader).toBeInTheDocument();
    });
  },

  // Test component handles error state
  handlesErrorState: (component: React.ReactElement, errorMessage: string) => {
    it('shows error state', () => {
      renderWithTheme(component);
      expect(screen.getByText(errorMessage)).toBeInTheDocument();
    });
  },

  // Test accessibility
  hasProperAriaLabels: (component: React.ReactElement) => {
    it('has proper ARIA labels', () => {
      renderWithTheme(component);
      // Add specific accessibility checks here
    });
  },
};

// Custom matchers for better test assertions
export const customMatchers = {
  toBeVisibleInViewport: () => ({
    compare: (element: Element) => {
      const rect = element.getBoundingClientRect();
      const isVisible = rect.top >= 0 &&
                       rect.left >= 0 &&
                       rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
                       rect.right <= (window.innerWidth || document.documentElement.clientWidth);

      return {
        pass: isVisible,
        message: () => `Expected element to ${isVisible ? 'not ' : ''}be visible in viewport`,
      };
    },
  }),

  toHaveFileCount: () => ({
    compare: (files: FileList | File[], expectedCount: number) => {
      const actualCount = files.length;
      return {
        pass: actualCount === expectedCount,
        message: () => `Expected ${expectedCount} files, but got ${actualCount}`,
      };
    },
  }),
};

// Setup function for tests that need common mocks
export const setupTestEnvironment = () => {
  // Mock IntersectionObserver with proper implementation
  const mockIntersectionObserver = jest.fn();
  mockIntersectionObserver.mockReturnValue({
    observe: jest.fn(),
    unobserve: jest.fn(),
    disconnect: jest.fn(),
  });
  global.IntersectionObserver = mockIntersectionObserver;

  // Mock FileReader
  global.FileReader = jest.fn().mockImplementation(() => ({
    onload: null,
    result: null,
    readAsDataURL: jest.fn(function(this: FileReader) {
      setTimeout(() => {
        if (this.onload) {
          (this.onload as any)({ target: { result: 'data:image/jpeg;base64,mock' } });
        }
      }, 0);
    }),
  })) as any;

  // Mock localStorage
  Object.defineProperty(window, 'localStorage', {
    value: mockLocalStorage(),
    writable: true,
  });
};