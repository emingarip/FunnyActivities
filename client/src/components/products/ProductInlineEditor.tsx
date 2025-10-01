import React, { useState, useEffect, useRef } from 'react';
import { ProductVariantDto, UnitOfMeasureDto } from '../../services/api.types';
import { productsAPI } from '../../services/api';

interface ProductInlineEditorProps {
  variant: ProductVariantDto;
  unitsOfMeasure: UnitOfMeasureDto[];
  onSave: (updatedVariant: ProductVariantDto) => void;
  onCancel: () => void;
  onError: (error: string) => void;
}

const ProductInlineEditor: React.FC<ProductInlineEditorProps> = ({
  variant,
  unitsOfMeasure,
  onSave,
  onCancel,
  onError
}) => {
  const [formData, setFormData] = useState({
    name: variant.name,
    stockQuantity: variant.stockQuantity,
    unitOfMeasureId: variant.unitOfMeasureId,
    unitValue: variant.unitValue,
    usageNotes: variant.usageNotes || ''
  });
  const [isSaving, setIsSaving] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const nameInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    // Focus on name input when component mounts
    if (nameInputRef.current) {
      nameInputRef.current.focus();
      nameInputRef.current.select();
    }
  }, []);

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.name.trim()) {
      newErrors.name = 'Name is required';
    }

    if (formData.stockQuantity < 0) {
      newErrors.stockQuantity = 'Stock quantity cannot be negative';
    }

    if (formData.unitValue <= 0) {
      newErrors.unitValue = 'Unit value must be greater than 0';
    }

    if (!formData.unitOfMeasureId) {
      newErrors.unitOfMeasureId = 'Unit of measure is required';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSave = async () => {
    if (!validateForm()) {
      return;
    }

    setIsSaving(true);
    try {
      const response = await productsAPI.updateProductVariant(variant.id, {
        name: formData.name !== variant.name ? formData.name : undefined,
        stockQuantity: formData.stockQuantity !== variant.stockQuantity ? formData.stockQuantity : undefined,
        unitOfMeasureId: formData.unitOfMeasureId !== variant.unitOfMeasureId ? formData.unitOfMeasureId : undefined,
        unitValue: formData.unitValue !== variant.unitValue ? formData.unitValue : undefined,
        usageNotes: formData.usageNotes !== (variant.usageNotes || '') ? formData.usageNotes : undefined
      });

      const updatedVariant: ProductVariantDto = {
        ...variant,
        ...response.data,
        updatedAt: new Date().toISOString()
      };

      onSave(updatedVariant);
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || 'Failed to update variant';
      onError(errorMessage);
    } finally {
      setIsSaving(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      handleSave();
    } else if (e.key === 'Escape') {
      e.preventDefault();
      onCancel();
    }
  };

  const handleInputChange = (field: string, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // Clear error for this field when user starts typing
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  return (
    <div className="product-inline-editor">
      <div className="editor-row">
        <div className="editor-field">
          <label htmlFor={`name-${variant.id}`}>Name</label>
          <input
            ref={nameInputRef}
            id={`name-${variant.id}`}
            type="text"
            value={formData.name}
            onChange={(e) => handleInputChange('name', e.target.value)}
            onKeyDown={handleKeyDown}
            className={errors.name ? 'error' : ''}
            disabled={isSaving}
          />
          {errors.name && <span className="error-message">{errors.name}</span>}
        </div>

        <div className="editor-field">
          <label htmlFor={`stock-${variant.id}`}>Stock</label>
          <input
            id={`stock-${variant.id}`}
            type="number"
            min="0"
            step="0.01"
            value={formData.stockQuantity}
            onChange={(e) => handleInputChange('stockQuantity', parseFloat(e.target.value) || 0)}
            onKeyDown={handleKeyDown}
            className={errors.stockQuantity ? 'error' : ''}
            disabled={isSaving}
          />
          {errors.stockQuantity && <span className="error-message">{errors.stockQuantity}</span>}
        </div>

        <div className="editor-field">
          <label htmlFor={`unit-${variant.id}`}>Unit</label>
          <select
            id={`unit-${variant.id}`}
            value={formData.unitOfMeasureId}
            onChange={(e) => handleInputChange('unitOfMeasureId', e.target.value)}
            onKeyDown={handleKeyDown}
            className={errors.unitOfMeasureId ? 'error' : ''}
            disabled={isSaving}
          >
            {unitsOfMeasure.map(unit => (
              <option key={unit.id} value={unit.id}>
                {unit.name} ({unit.symbol})
              </option>
            ))}
          </select>
          {errors.unitOfMeasureId && <span className="error-message">{errors.unitOfMeasureId}</span>}
        </div>

        <div className="editor-field">
          <label htmlFor={`value-${variant.id}`}>Value</label>
          <input
            id={`value-${variant.id}`}
            type="number"
            min="0.01"
            step="0.01"
            value={formData.unitValue}
            onChange={(e) => handleInputChange('unitValue', parseFloat(e.target.value) || 0)}
            onKeyDown={handleKeyDown}
            className={errors.unitValue ? 'error' : ''}
            disabled={isSaving}
          />
          {errors.unitValue && <span className="error-message">{errors.unitValue}</span>}
        </div>
      </div>

      <div className="editor-row">
        <div className="editor-field full-width">
          <label htmlFor={`notes-${variant.id}`}>Usage Notes</label>
          <textarea
            id={`notes-${variant.id}`}
            value={formData.usageNotes}
            onChange={(e) => handleInputChange('usageNotes', e.target.value)}
            onKeyDown={handleKeyDown}
            rows={2}
            disabled={isSaving}
          />
        </div>
      </div>

      <div className="editor-actions">
        <button
          type="button"
          className="btn btn-sm btn-success"
          onClick={handleSave}
          disabled={isSaving}
        >
          {isSaving ? '💾 Saving...' : '💾 Save'}
        </button>
        <button
          type="button"
          className="btn btn-sm btn-secondary"
          onClick={onCancel}
          disabled={isSaving}
        >
          ❌ Cancel
        </button>
      </div>
    </div>
  );
};

export default ProductInlineEditor;