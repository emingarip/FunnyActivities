import React from 'react';
import { render, screen, fireEvent, waitFor, act } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import ProductWizard from '../ProductWizard';
import { productsAPI } from '../../../services/api';

// Mock the API
jest.mock('../../../services/api', () => ({
  productsAPI: {
    getProductCategories: jest.fn(),
    getUnitsOfMeasure: jest.fn(),
    getProduct: jest.fn(),
    getProductVariants: jest.fn(),
    createBaseProduct: jest.fn(),
    createProductVariant: jest.fn(),
    uploadProductVariantPhotos: jest.fn(),
    saveProductDraft: jest.fn(),
  },
}));

// Mock child components
jest.mock('../ProductWizardStep1', () => {
  return function MockProductWizardStep1({ formData, onChange, categories, errors }: any) {
    return (
      <div data-testid="step1">
        <input
          data-testid="product-name-input"
          value={formData.baseProduct.name}
          onChange={(e) => onChange({
            ...formData,
            baseProduct: { ...formData.baseProduct, name: e.target.value }
          })}
        />
        {errors?.baseProduct?.name && <span data-testid="name-error">{errors.baseProduct.name}</span>}
      </div>
    );
  };
});

jest.mock('../ProductWizardStep2', () => {
  return function MockProductWizardStep2({ formData, onChange, unitsOfMeasure, errors }: any) {
    return (
      <div data-testid="step2">
        <input
          data-testid="variant-name-input"
          value={formData.variants[0]?.name || ''}
          onChange={(e) => onChange({
            ...formData,
            variants: [{ ...formData.variants[0], name: e.target.value }]
          })}
        />
        {errors?.variants?.[0]?.name && <span data-testid="variant-name-error">{errors.variants[0].name}</span>}
      </div>
    );
  };
});

jest.mock('../ProductWizardStep3', () => {
  return function MockProductWizardStep3({ formData, categories, unitsOfMeasure }: any) {
    return (
      <div data-testid="step3">
        <div>Review: {formData.baseProduct.name}</div>
        <div>Variants: {formData.variants.length}</div>
      </div>
    );
  };
});

const theme = createTheme();

const renderWithTheme = (component: React.ReactElement) => {
  return render(
    <ThemeProvider theme={theme}>
      {component}
    </ThemeProvider>
  );
};

describe('ProductWizard', () => {
  const mockOnComplete = jest.fn();
  const mockOnCancel = jest.fn();

  const defaultProps = {
    open: true,
    onComplete: mockOnComplete,
    onCancel: mockOnCancel,
  };

  beforeEach(() => {
    jest.clearAllMocks();

    // Setup default API mocks
    (productsAPI.getProductCategories as jest.Mock).mockResolvedValue({
      data: { success: true, data: [{ id: '1', name: 'Electronics' }] }
    });
    (productsAPI.getUnitsOfMeasure as jest.Mock).mockResolvedValue({
      data: { success: true, data: [{ id: '1', name: 'Pieces', symbol: 'pcs' }] }
    });
  });

  describe('Initial Rendering', () => {
    it('renders wizard dialog when open is true', async () => {
      renderWithTheme(<ProductWizard {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Create New Product')).toBeInTheDocument();
      });
    });

    it('does not render when open is false', () => {
      renderWithTheme(<ProductWizard {...defaultProps} open={false} />);

      expect(screen.queryByText('Create New Product')).not.toBeInTheDocument();
    });

    it('loads dependencies on mount', async () => {
      renderWithTheme(<ProductWizard {...defaultProps} />);

      await waitFor(() => {
        expect(productsAPI.getProductCategories).toHaveBeenCalled();
        expect(productsAPI.getUnitsOfMeasure).toHaveBeenCalled();
      });
    });
  });

  describe('Step Navigation', () => {
    it('starts on step 1 (Base Product Details)', async () => {
      renderWithTheme(<ProductWizard {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByTestId('step1')).toBeInTheDocument();
        expect(screen.queryByTestId('step2')).not.toBeInTheDocument();
      });
    });

    it('navigates to next step when Next button is clicked', async () => {
      renderWithTheme(<ProductWizard {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByTestId('step1')).toBeInTheDocument();
      });

      // Fill required fields
      const nameInput = screen.getByTestId('product-name-input');
      fireEvent.change(nameInput, { target: { value: 'Test Product' } });

      // Click Next
      const nextButton = screen.getByText('Next');
      fireEvent.click(nextButton);

      await waitFor(() => {
        expect(screen.getByTestId('step2')).toBeInTheDocument();
        expect(screen.queryByTestId('step1')).not.toBeInTheDocument();
      });
    });

    it('navigates back to previous step when Back button is clicked', async () => {
      renderWithTheme(<ProductWizard {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByTestId('step1')).toBeInTheDocument();
      });

      // Navigate to step 2
      const nameInput = screen.getByTestId('product-name-input');
      fireEvent.change(nameInput, { target: { value: 'Test Product' } });
      const nextButton = screen.getByText('Next');
      fireEvent.click(nextButton);

      await waitFor(() => {
        expect(screen.getByTestId('step2')).toBeInTheDocument();
      });

      // Navigate back
      const backButton = screen.getByText('Back');
      fireEvent.click(backButton);

      await waitFor(() => {
        expect(screen.getByTestId('step1')).toBeInTheDocument();
        expect(screen.queryByTestId('step2')).not.toBeInTheDocument();
      });
    });
  });

  describe('Form Validation', () => {
    it('shows validation error for empty product name', async () => {
      renderWithTheme(<ProductWizard {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByTestId('step1')).toBeInTheDocument();
      });

      // Try to navigate without filling required fields
      const nextButton = screen.getByText('Next');
      fireEvent.click(nextButton);

      await waitFor(() => {
        expect(screen.getByTestId('name-error')).toBeInTheDocument();
        expect(screen.getByTestId('name-error')).toHaveTextContent('Product name is required');
      });
    });

    it('shows validation error for empty variant name', async () => {
      renderWithTheme(<ProductWizard {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByTestId('step1')).toBeInTheDocument();
      });

      // Fill step 1 and navigate to step 2
      const nameInput = screen.getByTestId('product-name-input');
      fireEvent.change(nameInput, { target: { value: 'Test Product' } });
      const nextButton = screen.getByText('Next');
      fireEvent.click(nextButton);

      await waitFor(() => {
        expect(screen.getByTestId('step2')).toBeInTheDocument();
      });

      // Try to navigate to step 3 without filling variant name
      const nextButton2 = screen.getByText('Next');
      fireEvent.click(nextButton2);

      await waitFor(() => {
        expect(screen.getByTestId('variant-name-error')).toBeInTheDocument();
        expect(screen.getByTestId('variant-name-error')).toHaveTextContent('Variant name is required');
      });
    });
  });

  describe('Product Creation', () => {
    it('creates product successfully with valid data', async () => {
      // Mock successful API responses
      (productsAPI.createBaseProduct as jest.Mock).mockResolvedValue({
        data: { success: true, data: { id: 'base-product-id' } }
      });
      (productsAPI.createProductVariant as jest.Mock).mockResolvedValue({
        data: { success: true, data: { id: 'variant-id' } }
      });

      renderWithTheme(<ProductWizard {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByTestId('step1')).toBeInTheDocument();
      });

      // Fill step 1
      const nameInput = screen.getByTestId('product-name-input');
      fireEvent.change(nameInput, { target: { value: 'Test Product' } });

      // Navigate to step 2
      const nextButton = screen.getByText('Next');
      fireEvent.click(nextButton);

      await waitFor(() => {
        expect(screen.getByTestId('step2')).toBeInTheDocument();
      });

      // Fill step 2
      const variantNameInput = screen.getByTestId('variant-name-input');
      fireEvent.change(variantNameInput, { target: { value: 'Test Variant' } });

      // Navigate to step 3
      const nextButton2 = screen.getByText('Next');
      fireEvent.click(nextButton2);

      await waitFor(() => {
        expect(screen.getByTestId('step3')).toBeInTheDocument();
      });

      // Submit the form
      const createButton = screen.getByText('Create Product');
      fireEvent.click(createButton);

      await waitFor(() => {
        expect(productsAPI.createBaseProduct).toHaveBeenCalledWith({
          name: 'Test Product',
          description: '',
          categoryId: undefined
        });
        expect(productsAPI.createProductVariant).toHaveBeenCalled();
        expect(mockOnComplete).toHaveBeenCalledWith('base-product-id');
      });
    });

    it('handles API errors during product creation', async () => {
      // Mock API error
      (productsAPI.createBaseProduct as jest.Mock).mockRejectedValue(new Error('API Error'));

      renderWithTheme(<ProductWizard {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByTestId('step1')).toBeInTheDocument();
      });

      // Fill form and submit
      const nameInput = screen.getByTestId('product-name-input');
      fireEvent.change(nameInput, { target: { value: 'Test Product' } });

      // Navigate through steps
      const nextButton = screen.getByText('Next');
      fireEvent.click(nextButton);

      await waitFor(() => {
        expect(screen.getByTestId('step2')).toBeInTheDocument();
      });

      const variantNameInput = screen.getByTestId('variant-name-input');
      fireEvent.change(variantNameInput, { target: { value: 'Test Variant' } });

      const nextButton2 = screen.getByText('Next');
      fireEvent.click(nextButton2);

      await waitFor(() => {
        expect(screen.getByTestId('step3')).toBeInTheDocument();
      });

      const createButton = screen.getByText('Create Product');
      fireEvent.click(createButton);

      await waitFor(() => {
        expect(screen.getByText('Failed to create product')).toBeInTheDocument();
        expect(mockOnComplete).not.toHaveBeenCalled();
      });
    });
  });

  describe('Draft Management', () => {
    it('saves draft successfully', async () => {
      (productsAPI.saveProductDraft as jest.Mock).mockResolvedValue({
        data: { success: true }
      });

      renderWithTheme(<ProductWizard {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByTestId('step1')).toBeInTheDocument();
      });

      // Fill some data
      const nameInput = screen.getByTestId('product-name-input');
      fireEvent.change(nameInput, { target: { value: 'Draft Product' } });

      // Save draft
      const saveDraftButton = screen.getByText('Save Draft');
      fireEvent.click(saveDraftButton);

      await waitFor(() => {
        expect(productsAPI.saveProductDraft).toHaveBeenCalled();
        expect(screen.getByText('Draft saved successfully')).toBeInTheDocument();
      });
    });
  });

  describe('Edit Mode', () => {
    it('loads existing product data in edit mode', async () => {
      const mockProductData = {
        baseProduct: {
          name: 'Existing Product',
          description: 'Existing Description',
          categoryId: '1',
          photos: [],
          dynamicProperties: {}
        },
        variants: [{
          id: 'variant-1',
          name: 'Existing Variant',
          size: 'M',
          color: 'Blue',
          stockQuantity: 10,
          unitOfMeasureId: '1',
          unitValue: 1,
          usageNotes: 'Test notes',
          photos: [],
          dynamicProperties: {},
          isNew: false
        }],
        isDraft: false,
        currentStep: 0
      };

      (productsAPI.getProduct as jest.Mock).mockResolvedValue({
        data: { success: true, data: mockProductData.baseProduct }
      });
      (productsAPI.getProductVariants as jest.Mock).mockResolvedValue({
        data: { success: true, data: mockProductData.variants }
      });

      renderWithTheme(
        <ProductWizard
          {...defaultProps}
          isEdit={true}
          productId="test-product-id"
        />
      );

      await waitFor(() => {
        expect(productsAPI.getProduct).toHaveBeenCalledWith('test-product-id');
        expect(productsAPI.getProductVariants).toHaveBeenCalledWith('test-product-id');
      });
    });
  });

  describe('Exit Dialog', () => {
    it('shows exit dialog when trying to cancel with unsaved changes', async () => {
      renderWithTheme(<ProductWizard {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByTestId('step1')).toBeInTheDocument();
      });

      // Fill some data
      const nameInput = screen.getByTestId('product-name-input');
      fireEvent.change(nameInput, { target: { value: 'Test Product' } });

      // Try to cancel
      const cancelButton = screen.getByText('Cancel');
      fireEvent.click(cancelButton);

      await waitFor(() => {
        expect(screen.getByText('Discard Changes?')).toBeInTheDocument();
      });
    });

    it('calls onCancel directly when no changes made', async () => {
      renderWithTheme(<ProductWizard {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByTestId('step1')).toBeInTheDocument();
      });

      // Try to cancel without making changes
      const cancelButton = screen.getByText('Cancel');
      fireEvent.click(cancelButton);

      await waitFor(() => {
        expect(mockOnCancel).toHaveBeenCalled();
        expect(screen.queryByText('Discard Changes?')).not.toBeInTheDocument();
      });
    });
  });

  describe('Loading States', () => {
    it('shows loading state when editing existing product', async () => {
      (productsAPI.getProduct as jest.Mock).mockImplementation(() => new Promise(() => {})); // Never resolves

      renderWithTheme(
        <ProductWizard
          {...defaultProps}
          isEdit={true}
          productId="test-product-id"
        />
      );

      await waitFor(() => {
        expect(screen.getByText('Loading product data...')).toBeInTheDocument();
      });
    });
  });

  describe('Error Handling', () => {
    it('handles dependency loading errors gracefully', async () => {
      (productsAPI.getProductCategories as jest.Mock).mockRejectedValue(new Error('Network error'));

      renderWithTheme(<ProductWizard {...defaultProps} />);

      await waitFor(() => {
        expect(screen.getByText('Failed to load form data')).toBeInTheDocument();
      });
    });
  });
});