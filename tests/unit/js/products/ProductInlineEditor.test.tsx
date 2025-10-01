import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import ProductInlineEditor from '../../../client/src/components/products/ProductInlineEditor';
import { productsAPI } from '../../../../client/src/services/api';
import { ProductVariantDto, UnitOfMeasureDto } from '../../../../client/src/services/api.types';

// Mock the API
jest.mock('../../../services/api', () => ({
  productsAPI: {
    updateProductVariant: jest.fn(),
  },
}));

const theme = createTheme();

const renderWithTheme = (component: React.ReactElement) => {
  return render(
    <ThemeProvider theme={theme}>
      {component}
    </ThemeProvider>
  );
};

describe('ProductInlineEditor', () => {
  const mockVariant: ProductVariantDto = {
    id: 'variant-1',
    baseProductId: 'base-1',
    baseProductName: 'Test Product',
    baseProductDescription: 'Test Description',
    baseProductCategoryId: 'cat-1',
    baseProductCategoryName: 'Test Category',
    name: 'Test Variant',
    stockQuantity: 10,
    unitOfMeasureId: 'unit-1',
    unitOfMeasureName: 'Pieces',
    unitSymbol: 'pcs',
    unitValue: 1,
    usageNotes: 'Test notes',
    photos: [],
    dynamicProperties: {},
    createdAt: '2023-01-01T00:00:00Z',
    updatedAt: '2023-01-01T00:00:00Z'
  };

  const mockUnitsOfMeasure: UnitOfMeasureDto[] = [
    { id: 'unit-1', name: 'Pieces', symbol: 'pcs', type: 'count', createdAt: '2023-01-01T00:00:00Z', updatedAt: '2023-01-01T00:00:00Z' },
    { id: 'unit-2', name: 'Kilograms', symbol: 'kg', type: 'weight', createdAt: '2023-01-01T00:00:00Z', updatedAt: '2023-01-01T00:00:00Z' }
  ];

  const mockOnSave = jest.fn();
  const mockOnCancel = jest.fn();
  const mockOnError = jest.fn();

  const defaultProps = {
    variant: mockVariant,
    unitsOfMeasure: mockUnitsOfMeasure,
    onSave: mockOnSave,
    onCancel: mockOnCancel,
    onError: mockOnError
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('Initial Rendering', () => {
    it('renders all form fields with correct initial values', () => {
      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      expect(screen.getByDisplayValue('Test Variant')).toBeInTheDocument();
      expect(screen.getByDisplayValue('10')).toBeInTheDocument();
      expect(screen.getByDisplayValue('1')).toBeInTheDocument();
      expect(screen.getByDisplayValue('Test notes')).toBeInTheDocument();

      // Check that the select has the correct value
      const unitSelect = screen.getByDisplayValue('Pieces (pcs)');
      expect(unitSelect).toBeInTheDocument();
      expect(unitSelect).toHaveValue('unit-1');
    });

    it('renders unit of measure options correctly', () => {
      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      const select = screen.getByDisplayValue('unit-1');
      expect(select).toBeInTheDocument();

      // Check that both units are available as options
      expect(screen.getByText('Pieces (pcs)')).toBeInTheDocument();
      expect(screen.getByText('Kilograms (kg)')).toBeInTheDocument();
    });

    it('renders save and cancel buttons', () => {
      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      expect(screen.getByText('💾 Save')).toBeInTheDocument();
      expect(screen.getByText('❌ Cancel')).toBeInTheDocument();
    });
  });

  describe('Form Validation', () => {
    it('shows error for empty name', async () => {
      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      const nameInput = screen.getByDisplayValue('Test Variant');
      fireEvent.change(nameInput, { target: { value: '' } });

      const saveButton = screen.getByText('💾 Save');
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(screen.getByText('Name is required')).toBeInTheDocument();
      });
    });

    it('shows error for negative stock quantity', async () => {
      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      const stockInput = screen.getByDisplayValue('10');
      fireEvent.change(stockInput, { target: { value: '-5' } });

      const saveButton = screen.getByText('💾 Save');
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(screen.getByText('Stock quantity cannot be negative')).toBeInTheDocument();
      });
    });

    it('shows error for zero or negative unit value', async () => {
      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      const unitValueInput = screen.getByDisplayValue('1');
      fireEvent.change(unitValueInput, { target: { value: '0' } });

      const saveButton = screen.getByText('💾 Save');
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(screen.getByText('Unit value must be greater than 0')).toBeInTheDocument();
      });
    });

    it('shows error for missing unit of measure', async () => {
      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      const unitSelect = screen.getByDisplayValue('Pieces (pcs)');
      fireEvent.change(unitSelect, { target: { value: '' } });

      const saveButton = screen.getByText('💾 Save');
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(screen.getByText('Unit of measure is required')).toBeInTheDocument();
      });
    });

    it('clears error when user starts typing in field with error', async () => {
      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      const nameInput = screen.getByDisplayValue('Test Variant');
      fireEvent.change(nameInput, { target: { value: '' } });

      const saveButton = screen.getByText('💾 Save');
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(screen.getByText('Name is required')).toBeInTheDocument();
      });

      // Start typing again
      fireEvent.change(nameInput, { target: { value: 'a' } });

      await waitFor(() => {
        expect(screen.queryByText('Name is required')).not.toBeInTheDocument();
      });
    });
  });

  describe('Form Submission', () => {
    it('calls onSave with updated variant when save is successful', async () => {
      const mockResponse = {
        data: {
          id: 'variant-1',
          name: 'Updated Variant',
          stockQuantity: 15,
          unitOfMeasureId: 'unit-2',
          unitValue: 2,
          usageNotes: 'Updated notes'
        }
      };

      (productsAPI.updateProductVariant as jest.Mock).mockResolvedValue(mockResponse);

      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      // Update form fields
      const nameInput = screen.getByDisplayValue('Test Variant');
      fireEvent.change(nameInput, { target: { value: 'Updated Variant' } });

      const stockInput = screen.getByDisplayValue('10');
      fireEvent.change(stockInput, { target: { value: '15' } });

      const unitSelect = screen.getByDisplayValue('Pieces (pcs)');
      fireEvent.change(unitSelect, { target: { value: 'unit-2' } });

      const unitValueInput = screen.getByDisplayValue('1');
      fireEvent.change(unitValueInput, { target: { value: '2' } });

      const notesTextarea = screen.getByDisplayValue('Test notes');
      fireEvent.change(notesTextarea, { target: { value: 'Updated notes' } });

      const saveButton = screen.getByText('💾 Save');
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(productsAPI.updateProductVariant).toHaveBeenCalledWith('variant-1', {
          name: 'Updated Variant',
          stockQuantity: 15,
          unitOfMeasureId: 'unit-2',
          unitValue: 2,
          usageNotes: 'Updated notes'
        });
        expect(mockOnSave).toHaveBeenCalled();
      });
    });

    it('only sends changed fields to API', async () => {
      const mockResponse = {
        data: {
          id: 'variant-1',
          name: 'Test Variant',
          stockQuantity: 20,
          unitOfMeasureId: 'unit-1',
          unitValue: 1,
          usageNotes: 'Test notes'
        }
      };

      (productsAPI.updateProductVariant as jest.Mock).mockResolvedValue(mockResponse);

      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      // Only change stock quantity
      const stockInput = screen.getByDisplayValue('10');
      fireEvent.change(stockInput, { target: { value: '20' } });

      const saveButton = screen.getByText('💾 Save');
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(productsAPI.updateProductVariant).toHaveBeenCalledWith('variant-1', {
          name: undefined,
          stockQuantity: 20,
          unitOfMeasureId: undefined,
          unitValue: undefined,
          usageNotes: undefined
        });
      });
    });

    it('calls onError when API call fails', async () => {
      const errorMessage = 'Failed to update variant';
      (productsAPI.updateProductVariant as jest.Mock).mockRejectedValue({
        response: { data: { message: errorMessage } }
      });

      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      const saveButton = screen.getByText('💾 Save');
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(mockOnError).toHaveBeenCalledWith(errorMessage);
      });
    });

    it('handles generic error when no specific error message', async () => {
      (productsAPI.updateProductVariant as jest.Mock).mockRejectedValue(new Error('Network error'));

      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      const saveButton = screen.getByText('💾 Save');
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(mockOnError).toHaveBeenCalledWith('Failed to update variant');
      });
    });
  });

  describe('Keyboard Navigation', () => {
    it('saves when Enter key is pressed', async () => {
      (productsAPI.updateProductVariant as jest.Mock).mockResolvedValue({
        data: { id: 'variant-1' }
      });

      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      const nameInput = screen.getByDisplayValue('Test Variant');
      fireEvent.keyDown(nameInput, { key: 'Enter', preventDefault: jest.fn() });

      await waitFor(() => {
        expect(productsAPI.updateProductVariant).toHaveBeenCalled();
      });
    });

    it('cancels when Escape key is pressed', () => {
      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      const nameInput = screen.getByDisplayValue('Test Variant');
      fireEvent.keyDown(nameInput, { key: 'Escape', preventDefault: jest.fn() });

      expect(mockOnCancel).toHaveBeenCalled();
    });
  });

  describe('Loading States', () => {
    it('disables all inputs and buttons while saving', async () => {
      (productsAPI.updateProductVariant as jest.Mock).mockImplementation(
        () => new Promise(resolve => setTimeout(resolve, 100))
      );

      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      const saveButton = screen.getByText('💾 Save');
      fireEvent.click(saveButton);

      // Check that inputs are disabled during save
      const nameInput = screen.getByDisplayValue('Test Variant');
      const stockInput = screen.getByDisplayValue('10');
      const unitSelect = screen.getByDisplayValue('Pieces (pcs)');
      const unitValueInput = screen.getByDisplayValue('1');
      const notesTextarea = screen.getByDisplayValue('Test notes');
      const cancelButton = screen.getByText('❌ Cancel');

      expect(nameInput).toBeDisabled();
      expect(stockInput).toBeDisabled();
      expect(unitSelect).toBeDisabled();
      expect(unitValueInput).toBeDisabled();
      expect(notesTextarea).toBeDisabled();
      expect(saveButton).toBeDisabled();
      expect(cancelButton).toBeDisabled();

      expect(screen.getByText('💾 Saving...')).toBeInTheDocument();
    });

    it('re-enables inputs and buttons after save completes', async () => {
      (productsAPI.updateProductVariant as jest.Mock).mockResolvedValue({
        data: { id: 'variant-1' }
      });

      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      const saveButton = screen.getByText('💾 Save');
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(mockOnSave).toHaveBeenCalled();
      });

      // Check that inputs are re-enabled after save
      const nameInput = screen.getByDisplayValue('Test Variant');
      expect(nameInput).not.toBeDisabled();
      expect(saveButton).not.toBeDisabled();
    });
  });

  describe('Cancel Action', () => {
    it('calls onCancel when cancel button is clicked', () => {
      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      const cancelButton = screen.getByText('❌ Cancel');
      fireEvent.click(cancelButton);

      expect(mockOnCancel).toHaveBeenCalled();
    });
  });

  describe('Focus Management', () => {
    it('focuses on name input when component mounts', () => {
      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      const nameInput = screen.getByDisplayValue('Test Variant');
      expect(nameInput).toHaveFocus();
    });
  });

  describe('Edge Cases', () => {
    it('handles empty usage notes gracefully', async () => {
      const variantWithEmptyNotes = {
        ...mockVariant,
        usageNotes: undefined
      };

      (productsAPI.updateProductVariant as jest.Mock).mockResolvedValue({
        data: { id: 'variant-1' }
      });

      renderWithTheme(
        <ProductInlineEditor
          {...defaultProps}
          variant={variantWithEmptyNotes}
        />
      );

      const saveButton = screen.getByText('💾 Save');
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(productsAPI.updateProductVariant).toHaveBeenCalledWith('variant-1', {
          name: undefined,
          stockQuantity: undefined,
          unitOfMeasureId: undefined,
          unitValue: undefined,
          usageNotes: undefined
        });
      });
    });

    it('handles invalid number inputs gracefully', () => {
      renderWithTheme(<ProductInlineEditor {...defaultProps} />);

      const stockInput = screen.getByDisplayValue('10');
      fireEvent.change(stockInput, { target: { value: 'invalid' } });

      // Should default to 0
      expect(stockInput).toHaveValue('0');
    });
  });
});