import React from 'react';
import { render, screen, fireEvent, waitFor, act } from '@testing-library/react';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import DynamicPropertiesInput from '../../../client/src/components/materials/DynamicPropertiesInput';

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

describe('DynamicPropertiesInput', () => {
  const defaultProps = {
    properties: {},
    onChange: jest.fn(),
    disabled: false,
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('Initial Rendering', () => {
    it('renders with empty properties', () => {
      renderWithTheme(<DynamicPropertiesInput {...defaultProps} />);

      expect(screen.getByText('Dynamic Properties')).toBeInTheDocument();
      expect(screen.getByText('Add New Property:')).toBeInTheDocument();
      expect(screen.getByLabelText('Property Key')).toBeInTheDocument();
      expect(screen.getByText('Add Property')).toBeInTheDocument();
    });

    it('renders with existing properties', () => {
      const existingProps = {
        properties: {
          color: 'red',
          weight: 100,
          active: true,
        },
        onChange: jest.fn(),
        disabled: false,
      };

      renderWithTheme(<DynamicPropertiesInput {...existingProps} />);

      expect(screen.getByText('Current Properties:')).toBeInTheDocument();
      expect(screen.getByText('color')).toBeInTheDocument();
      expect(screen.getByText('red')).toBeInTheDocument();
      expect(screen.getByText('weight')).toBeInTheDocument();
      expect(screen.getByText('100')).toBeInTheDocument();
      expect(screen.getByText('active')).toBeInTheDocument();
      expect(screen.getByText('true')).toBeInTheDocument();
    });

    it('shows error message when provided', () => {
      const propsWithError = {
        ...defaultProps,
        error: 'Invalid property format',
      };

      renderWithTheme(<DynamicPropertiesInput {...propsWithError} />);

      expect(screen.getByText('Invalid property format')).toBeInTheDocument();
    });
  });

  describe('Adding Properties', () => {
    it('adds a string property successfully', async () => {
      const mockOnChange = jest.fn();
      renderWithTheme(
        <DynamicPropertiesInput
          {...defaultProps}
          onChange={mockOnChange}
        />
      );

      // Fill in the form
      const keyInput = screen.getByLabelText('Property Key');
      const valueInput = screen.getByPlaceholderText('Enter value');
      const addButton = screen.getByText('Add Property');

      fireEvent.change(keyInput, { target: { value: 'color' } });
      fireEvent.change(valueInput, { target: { value: 'blue' } });
      fireEvent.click(addButton);

      expect(mockOnChange).toHaveBeenCalledWith({ color: 'blue' });
    });

    it('adds a number property successfully', async () => {
      const mockOnChange = jest.fn();
      renderWithTheme(
        <DynamicPropertiesInput
          {...defaultProps}
          onChange={mockOnChange}
        />
      );

      // Fill in the form
      const keyInput = screen.getByLabelText('Property Key');
      const valueInput = screen.getByPlaceholderText('Enter value');
      const typeSelect = screen.getByRole('combobox');
      const addButton = screen.getByText('Add Property');

      fireEvent.change(keyInput, { target: { value: 'weight' } });
      fireEvent.change(valueInput, { target: { value: '150' } });

      // Change type to number
      fireEvent.mouseDown(typeSelect);
      const numberOption = screen.getByText('Number');
      fireEvent.click(numberOption);

      fireEvent.click(addButton);

      expect(mockOnChange).toHaveBeenCalledWith({ weight: 150 });
    });

    it('adds a boolean property successfully', async () => {
      const mockOnChange = jest.fn();
      renderWithTheme(
        <DynamicPropertiesInput
          {...defaultProps}
          onChange={mockOnChange}
        />
      );

      // Fill in the form
      const keyInput = screen.getByLabelText('Property Key');
      const valueInput = screen.getByPlaceholderText('Enter value');
      const typeSelect = screen.getByRole('combobox');
      const addButton = screen.getByText('Add Property');

      fireEvent.change(keyInput, { target: { value: 'active' } });
      fireEvent.change(valueInput, { target: { value: 'true' } });

      // Change type to boolean
      fireEvent.mouseDown(typeSelect);
      const booleanOption = screen.getByText('Boolean');
      fireEvent.click(booleanOption);

      fireEvent.click(addButton);

      expect(mockOnChange).toHaveBeenCalledWith({ active: true });
    });

    it('clears form after adding property', async () => {
      const mockOnChange = jest.fn();
      renderWithTheme(
        <DynamicPropertiesInput
          {...defaultProps}
          onChange={mockOnChange}
        />
      );

      // Fill in the form
      const keyInput = screen.getByLabelText('Property Key');
      const valueInput = screen.getByPlaceholderText('Enter value');
      const addButton = screen.getByText('Add Property');

      fireEvent.change(keyInput, { target: { value: 'color' } });
      fireEvent.change(valueInput, { target: { value: 'blue' } });
      fireEvent.click(addButton);

      // Check that form is cleared
      expect(keyInput).toHaveValue('');
      expect(valueInput).toHaveValue('');
    });

    it('does not add property with empty key', () => {
      const mockOnChange = jest.fn();
      renderWithTheme(
        <DynamicPropertiesInput
          {...defaultProps}
          onChange={mockOnChange}
        />
      );

      const valueInput = screen.getByPlaceholderText('Enter value');
      const addButton = screen.getByText('Add Property');

      fireEvent.change(valueInput, { target: { value: 'blue' } });
      fireEvent.click(addButton);

      expect(mockOnChange).not.toHaveBeenCalled();
    });
  });

  describe('Editing Properties', () => {
    it('starts editing a property when edit button is clicked', () => {
      const existingProps = {
        properties: { color: 'red' },
        onChange: jest.fn(),
        disabled: false,
      };

      renderWithTheme(<DynamicPropertiesInput {...existingProps} />);

      const editButton = screen.getByTestId('EditIcon').closest('button');
      fireEvent.click(editButton!);

      // Should show edit form
      expect(screen.getByDisplayValue('red')).toBeInTheDocument();
      expect(screen.getByText('Save')).toBeInTheDocument();
      expect(screen.getByText('Cancel')).toBeInTheDocument();
    });

    it('saves edited property successfully', () => {
      const mockOnChange = jest.fn();
      const existingProps = {
        properties: { color: 'red' },
        onChange: mockOnChange,
        disabled: false,
      };

      renderWithTheme(<DynamicPropertiesInput {...existingProps} />);

      // Start editing
      const editButton = screen.getByTestId('EditIcon').closest('button');
      fireEvent.click(editButton!);

      // Change value
      const valueInput = screen.getByDisplayValue('red');
      fireEvent.change(valueInput, { target: { value: 'blue' } });

      // Save
      const saveButton = screen.getByText('Save');
      fireEvent.click(saveButton);

      expect(mockOnChange).toHaveBeenCalledWith({ color: 'blue' });
    });

    it('cancels editing without saving changes', () => {
      const mockOnChange = jest.fn();
      const existingProps = {
        properties: { color: 'red' },
        onChange: mockOnChange,
        disabled: false,
      };

      renderWithTheme(<DynamicPropertiesInput {...existingProps} />);

      // Start editing
      const editButton = screen.getByTestId('EditIcon').closest('button');
      fireEvent.click(editButton!);

      // Change value
      const valueInput = screen.getByDisplayValue('red');
      fireEvent.change(valueInput, { target: { value: 'blue' } });

      // Cancel
      const cancelButton = screen.getByText('Cancel');
      fireEvent.click(cancelButton);

      expect(mockOnChange).not.toHaveBeenCalled();
      expect(screen.getByText('red')).toBeInTheDocument();
    });
  });

  describe('Removing Properties', () => {
    it('removes a property when delete button is clicked', () => {
      const mockOnChange = jest.fn();
      const existingProps = {
        properties: { color: 'red', size: 'large' },
        onChange: mockOnChange,
        disabled: false,
      };

      renderWithTheme(<DynamicPropertiesInput {...existingProps} />);

      const deleteButtons = screen.getAllByTestId('DeleteIcon');
      fireEvent.click(deleteButtons[0]); // Click the first delete button (for 'color' property)

      expect(mockOnChange).toHaveBeenCalledWith({ size: 'large' });
    });
  });

  describe('Validation', () => {
    it('validates property key format', async () => {
      const mockOnChange = jest.fn();
      renderWithTheme(
        <DynamicPropertiesInput
          {...defaultProps}
          onChange={mockOnChange}
        />
      );

      const keyInput = screen.getByLabelText('Property Key');
      const valueInput = screen.getByPlaceholderText('Enter value');
      const addButton = screen.getByText('Add Property');

      // Try invalid key
      fireEvent.change(keyInput, { target: { value: '123invalid' } });
      fireEvent.change(valueInput, { target: { value: 'test' } });
      fireEvent.click(addButton);

      // Should not add invalid property
      expect(mockOnChange).not.toHaveBeenCalled();
    });

    it('validates property key length', async () => {
      const mockOnChange = jest.fn();
      renderWithTheme(
        <DynamicPropertiesInput
          {...defaultProps}
          onChange={mockOnChange}
        />
      );

      const keyInput = screen.getByLabelText('Property Key');
      const valueInput = screen.getByPlaceholderText('Enter value');
      const addButton = screen.getByText('Add Property');

      // Try key that's too long
      const longKey = 'a'.repeat(51);
      fireEvent.change(keyInput, { target: { value: longKey } });
      fireEvent.change(valueInput, { target: { value: 'test' } });
      fireEvent.click(addButton);

      // Should not add property with too long key
      expect(mockOnChange).not.toHaveBeenCalled();
    });

    it('validates property value length for strings', async () => {
      const mockOnChange = jest.fn();
      renderWithTheme(
        <DynamicPropertiesInput
          {...defaultProps}
          onChange={mockOnChange}
        />
      );

      const keyInput = screen.getByLabelText('Property Key');
      const valueInput = screen.getByPlaceholderText('Enter value');
      const addButton = screen.getByText('Add Property');

      // Try value that's too long
      const longValue = 'a'.repeat(501);
      fireEvent.change(keyInput, { target: { value: 'description' } });
      fireEvent.change(valueInput, { target: { value: longValue } });
      fireEvent.click(addButton);

      // Should not add property with too long value
      expect(mockOnChange).not.toHaveBeenCalled();
    });
  });

  describe('Disabled State', () => {
    it('disables all inputs and buttons when disabled', () => {
      const disabledProps = {
        ...defaultProps,
        disabled: true,
      };

      renderWithTheme(<DynamicPropertiesInput {...disabledProps} />);

      const keyInput = screen.getByLabelText('Property Key');
      const valueInput = screen.getByPlaceholderText('Enter value');
      const addButton = screen.getByText('Add Property');

      expect(keyInput).toBeDisabled();
      expect(valueInput).toBeDisabled();
      expect(addButton).toBeDisabled();
    });

    it('disables edit and delete buttons when disabled', () => {
      const disabledProps = {
        properties: { color: 'red' },
        onChange: jest.fn(),
        disabled: true,
      };

      renderWithTheme(<DynamicPropertiesInput {...disabledProps} />);

      const editButton = screen.getByTestId('EditIcon').closest('button');
      const deleteButton = screen.getByTestId('DeleteIcon').closest('button');

      expect(editButton).toBeDisabled();
      expect(deleteButton).toBeDisabled();
    });
  });

  describe('Property Limits', () => {
    it('prevents adding more than 20 properties', () => {
      const manyProperties: Record<string, string> = {};
      for (let i = 1; i <= 20; i++) {
        manyProperties[`prop${i}`] = `value${i}`;
      }

      const mockOnChange = jest.fn();
      const propsWithManyProperties = {
        properties: manyProperties,
        onChange: mockOnChange,
        disabled: false,
      };

      renderWithTheme(<DynamicPropertiesInput {...propsWithManyProperties} />);

      const keyInput = screen.getByLabelText('Property Key');
      const valueInput = screen.getByPlaceholderText('Enter value');
      const addButton = screen.getByText('Add Property');

      fireEvent.change(keyInput, { target: { value: 'prop21' } });
      fireEvent.change(valueInput, { target: { value: 'value21' } });
      fireEvent.click(addButton);

      // Should not add the 21st property
      expect(mockOnChange).not.toHaveBeenCalled();
    });
  });
});