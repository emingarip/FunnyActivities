import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import ActivityMaterialsDialog from '../components/activities/ActivityMaterialsDialog';

// Mock the API
const mockProductsAPI = {
  getProductVariant: jest.fn(),
  getProductVariantPhotos: jest.fn(),
};

jest.mock('../../services/api', () => ({
  productsAPI: mockProductsAPI,
}));

// Mock LazyLoadImage
jest.mock('react-lazy-load-image-component', () => ({
  LazyLoadImage: ({ src, alt, ...props }: any) => (
    <img src={src} alt={alt} {...props} />
  ),
}));

const theme = createTheme();

const renderWithTheme = (component: React.ReactElement) => {
  return render(
    <ThemeProvider theme={theme}>
      {component}
    </ThemeProvider>
  );
};

const mockMaterials = [
  {
    id: 'material-1',
    productVariantId: 'variant-1',
    quantity: 2,
    unitOfMeasureId: 'unit-1',
    productVariant: {
      id: 'variant-1',
      name: 'Test Product',
      baseProduct: {
        id: 'product-1',
        name: 'Base Product',
      },
    },
    unitOfMeasure: {
      id: 'unit-1',
      name: 'pieces',
      symbol: 'pcs',
    },
  },
  {
    id: 'material-2',
    productVariantId: 'variant-2',
    quantity: 1,
    unitOfMeasureId: 'unit-2',
    productVariant: {
      id: 'variant-2',
      name: 'Another Product',
      baseProduct: {
        id: 'product-2',
        name: 'Another Base Product',
      },
    },
    unitOfMeasure: {
      id: 'unit-2',
      name: 'kilograms',
      symbol: 'kg',
    },
  },
];

const defaultProps = {
  open: true,
  onClose: jest.fn(),
  activityName: 'Test Activity',
  materials: mockMaterials,
};

describe('ActivityMaterialsDialog', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders dialog with correct title', () => {
    renderWithTheme(<ActivityMaterialsDialog {...defaultProps} />);

    expect(screen.getByText('Materials for Test Activity')).toBeInTheDocument();
  });

  it('shows loading state initially', () => {
    renderWithTheme(<ActivityMaterialsDialog {...defaultProps} />);

    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });

  it('loads and displays materials successfully', async () => {
    // Mock successful API responses
    mockProductsAPI.getProductVariant.mockResolvedValue({
      data: {
        success: true,
        data: {
          name: 'Detailed Product',
          baseProductName: 'Detailed Base Product',
          baseProductDescription: 'Detailed description',
        },
      },
    });

    mockProductsAPI.getProductVariantPhotos.mockResolvedValue({
      data: {
        success: true,
        data: ['photo1.jpg', 'photo2.jpg'],
      },
    });

    renderWithTheme(<ActivityMaterialsDialog {...defaultProps} />);

    await waitFor(() => {
      expect(screen.getByText('Detailed Base Product')).toBeInTheDocument();
    });

    expect(screen.getByText('2 pcs')).toBeInTheDocument();
    expect(screen.getByText('1 kg')).toBeInTheDocument();
  });

  it('handles API errors gracefully', async () => {
    mockProductsAPI.getProductVariant.mockRejectedValue(new Error('API Error'));
    mockProductsAPI.getProductVariantPhotos.mockRejectedValue(new Error('Photos API Error'));

    renderWithTheme(<ActivityMaterialsDialog {...defaultProps} />);

    await waitFor(() => {
      expect(screen.getByText('Failed to load materials details')).toBeInTheDocument();
    });
  });

  it('shows fallback data when API calls fail', async () => {
    mockProductsAPI.getProductVariant.mockRejectedValue(new Error('API Error'));
    mockProductsAPI.getProductVariantPhotos.mockRejectedValue(new Error('Photos API Error'));

    renderWithTheme(<ActivityMaterialsDialog {...defaultProps} />);

    await waitFor(() => {
      // Should show fallback data from the material object
      expect(screen.getByText('Test Product')).toBeInTheDocument();
      expect(screen.getByText('Base Product')).toBeInTheDocument();
    });
  });

  it('displays photos when available', async () => {
    mockProductsAPI.getProductVariant.mockResolvedValue({
      data: {
        success: true,
        data: {
          name: 'Product with Photos',
          baseProductName: 'Base Product with Photos',
          baseProductDescription: 'Description',
        },
      },
    });

    mockProductsAPI.getProductVariantPhotos.mockResolvedValue({
      data: {
        success: true,
        data: ['photo1.jpg'],
      },
    });

    renderWithTheme(<ActivityMaterialsDialog {...defaultProps} />);

    await waitFor(() => {
      const images = screen.getAllByRole('img');
      expect(images.length).toBeGreaterThan(0);
    });
  });

  it('shows placeholder when no photos available', async () => {
    mockProductsAPI.getProductVariant.mockResolvedValue({
      data: {
        success: true,
        data: {
          name: 'Product without Photos',
          baseProductName: 'Base Product without Photos',
          baseProductDescription: 'Description',
        },
      },
    });

    mockProductsAPI.getProductVariantPhotos.mockResolvedValue({
      data: {
        success: true,
        data: [],
      },
    });

    renderWithTheme(<ActivityMaterialsDialog {...defaultProps} />);

    await waitFor(() => {
      // Should show ImageIcon as placeholder
      expect(screen.getByTestId('ImageIcon')).toBeInTheDocument();
    });
  });

  it('handles empty materials array', () => {
    renderWithTheme(
      <ActivityMaterialsDialog
        {...defaultProps}
        materials={[]}
      />
    );

    expect(screen.getByText('No materials required for this activity')).toBeInTheDocument();
  });

  it('calls onClose when close button is clicked', () => {
    renderWithTheme(<ActivityMaterialsDialog {...defaultProps} />);

    const closeButton = screen.getByRole('button', { name: /close/i });
    fireEvent.click(closeButton);

    expect(defaultProps.onClose).toHaveBeenCalledTimes(1);
  });

  it('calls onClose when dialog is closed', () => {
    renderWithTheme(<ActivityMaterialsDialog {...defaultProps} />);

    // Simulate dialog close (backdrop click, escape key, etc.)
    const dialog = screen.getByRole('dialog');
    fireEvent.keyDown(dialog, { key: 'Escape' });

    // The dialog should handle close events
    expect(defaultProps.onClose).toHaveBeenCalledTimes(1);
  });

  it('renders with mobile styling', () => {
    // Mock mobile screen
    Object.defineProperty(window, 'innerWidth', {
      writable: true,
      configurable: true,
      value: 400,
    });

    renderWithTheme(<ActivityMaterialsDialog {...defaultProps} />);

    const dialog = screen.getByRole('dialog');
    expect(dialog).toBeInTheDocument();
    // Should have fullScreen on mobile
  });

  it('renders with desktop styling', () => {
    // Mock desktop screen
    Object.defineProperty(window, 'innerWidth', {
      writable: true,
      configurable: true,
      value: 1200,
    });

    renderWithTheme(<ActivityMaterialsDialog {...defaultProps} />);

    const dialog = screen.getByRole('dialog');
    expect(dialog).toBeInTheDocument();
    // Should not have fullScreen on desktop
  });

  it('handles materials with missing productVariant data', async () => {
    const materialsWithMissingData = [
      {
        id: 'material-1',
        productVariantId: 'variant-1',
        quantity: 1,
        unitOfMeasureId: 'unit-1',
        productVariant: undefined, // Missing productVariant
        unitOfMeasure: undefined, // Missing unitOfMeasure
      },
    ];

    mockProductsAPI.getProductVariant.mockRejectedValue(new Error('API Error'));

    renderWithTheme(
      <ActivityMaterialsDialog
        {...defaultProps}
        materials={materialsWithMissingData}
      />
    );

    await waitFor(() => {
      expect(screen.getByText('Unknown')).toBeInTheDocument();
    });
  });

  it('displays base product description when available', async () => {
    mockProductsAPI.getProductVariant.mockResolvedValue({
      data: {
        success: true,
        data: {
          name: 'Product Name',
          baseProductName: 'Base Product Name',
          baseProductDescription: 'This is a detailed description of the base product.',
        },
      },
    });

    mockProductsAPI.getProductVariantPhotos.mockResolvedValue({
      data: {
        success: true,
        data: [],
      },
    });

    renderWithTheme(<ActivityMaterialsDialog {...defaultProps} />);

    await waitFor(() => {
      expect(screen.getByText('This is a detailed description of the base product.')).toBeInTheDocument();
    });
  });

  it('handles dialog not open', () => {
    renderWithTheme(
      <ActivityMaterialsDialog
        {...defaultProps}
        open={false}
      />
    );

    // Dialog should not be visible
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  it('reloads data when dialog opens', () => {
    const { rerender } = renderWithTheme(
      <ActivityMaterialsDialog
        {...defaultProps}
        open={false}
      />
    );

    // Re-open dialog
    rerender(
      <ThemeProvider theme={theme}>
        <ActivityMaterialsDialog
          {...defaultProps}
          open={true}
        />
      </ThemeProvider>
    );

    // Should trigger data loading
    expect(mockProductsAPI.getProductVariant).toHaveBeenCalled();
  });
});