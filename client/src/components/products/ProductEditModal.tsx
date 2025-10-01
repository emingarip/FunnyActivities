import React, { useState, useEffect } from 'react';
import { ProductListDto, ProductVariantDto, UnitOfMeasureDto, ProductCategoryDto } from '../../services/api.types';
import { productsAPI } from '../../services/api';

interface ProductEditModalProps {
  product: ProductListDto | null;
  unitsOfMeasure: UnitOfMeasureDto[];
  categories: ProductCategoryDto[];
  isOpen: boolean;
  onClose: () => void;
  onSave: (updatedProduct: ProductListDto) => void;
  onError: (error: string) => void;
}

interface EditFormData {
  baseProduct: {
    name: string;
    description: string;
  };
  variants: Array<{
    id?: string;
    name: string;
    stockQuantity: number;
    unitOfMeasureId: string;
    unitValue: number;
    usageNotes: string;
    isNew: boolean;
    isDeleted: boolean;
  }>;
}

const ProductEditModal: React.FC<ProductEditModalProps> = ({
  product,
  unitsOfMeasure,
  categories,
  isOpen,
  onClose,
  onSave,
  onError
}) => {
  const [formData, setFormData] = useState<EditFormData>({
    baseProduct: {
      name: '',
      description: ''
    },
    variants: []
  });
  const [isSaving, setIsSaving] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [activeTab, setActiveTab] = useState<'base' | 'variants'>('base');

  // Initialize form data when product changes
  useEffect(() => {
    if (product && isOpen) {
      setFormData({
        baseProduct: {
          name: product.name,
          description: product.description || ''
        },
        variants: product.variants.map(variant => ({
          id: variant.id,
          name: variant.name,
          stockQuantity: variant.stockQuantity,
          unitOfMeasureId: variant.unitOfMeasureId,
          unitValue: variant.unitValue,
          usageNotes: variant.usageNotes || '',
          isNew: false,
          isDeleted: false
        }))
      });
      setErrors({});
      setActiveTab('base');
    }
  }, [product, isOpen]);

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.baseProduct.name.trim()) {
      newErrors['baseProduct.name'] = 'Product name is required';
    }

    formData.variants.forEach((variant, index) => {
      if (!variant.isDeleted) {
        if (!variant.name.trim()) {
          newErrors[`variant.${index}.name`] = 'Variant name is required';
        }
        if (variant.stockQuantity < 0) {
          newErrors[`variant.${index}.stock`] = 'Stock quantity cannot be negative';
        }
        if (variant.unitValue <= 0) {
          newErrors[`variant.${index}.unitValue`] = 'Unit value must be greater than 0';
        }
        if (!variant.unitOfMeasureId) {
          newErrors[`variant.${index}.unit`] = 'Unit of measure is required';
        }
      }
    });

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSave = async () => {
    if (!product || !validateForm()) {
      return;
    }

    setIsSaving(true);
    try {
      // Update base product
      await productsAPI.updateBaseProduct(product.id, {
        name: formData.baseProduct.name !== product.name ? formData.baseProduct.name : undefined,
        description: formData.baseProduct.description !== (product.description || '') ? formData.baseProduct.description : undefined
      });

      // Handle variants
      const variantPromises = formData.variants.map(async (variantForm) => {
        if (variantForm.isDeleted && variantForm.id) {
          // Delete existing variant
          return productsAPI.deleteProductVariant(variantForm.id);
        } else if (variantForm.isNew) {
          // Create new variant
          return productsAPI.createProductVariant({
            baseProductId: product.id,
            name: variantForm.name,
            stockQuantity: variantForm.stockQuantity,
            unitOfMeasureId: variantForm.unitOfMeasureId,
            unitValue: variantForm.unitValue,
            usageNotes: variantForm.usageNotes
          });
        } else if (variantForm.id) {
          // Update existing variant
          const originalVariant = product.variants.find(v => v.id === variantForm.id);
          if (originalVariant) {
            return productsAPI.updateProductVariant(variantForm.id, {
              name: variantForm.name !== originalVariant.name ? variantForm.name : undefined,
              stockQuantity: variantForm.stockQuantity !== originalVariant.stockQuantity ? variantForm.stockQuantity : undefined,
              unitOfMeasureId: variantForm.unitOfMeasureId !== originalVariant.unitOfMeasureId ? variantForm.unitOfMeasureId : undefined,
              unitValue: variantForm.unitValue !== originalVariant.unitValue ? variantForm.unitValue : undefined,
              usageNotes: variantForm.usageNotes !== (originalVariant.usageNotes || '') ? variantForm.usageNotes : undefined
            });
          }
        }
        return Promise.resolve();
      });

      await Promise.all(variantPromises);

      // Create updated product object
      const updatedProduct: ProductListDto = {
        ...product,
        name: formData.baseProduct.name,
        description: formData.baseProduct.description,
        variants: formData.variants
          .filter(v => !v.isDeleted)
          .map(variant => ({
            ...product.variants.find(v => v.id === variant.id)!,
            ...variant,
            updatedAt: new Date().toISOString()
          }))
      };

      onSave(updatedProduct);
      onClose();
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || 'Failed to update product';
      onError(errorMessage);
    } finally {
      setIsSaving(false);
    }
  };

  const addVariant = () => {
    setFormData(prev => ({
      ...prev,
      variants: [...prev.variants, {
        name: '',
        stockQuantity: 0,
        unitOfMeasureId: unitsOfMeasure[0]?.id || '',
        unitValue: 1,
        usageNotes: '',
        isNew: true,
        isDeleted: false
      }]
    }));
  };

  const removeVariant = (index: number) => {
    setFormData(prev => ({
      ...prev,
      variants: prev.variants.map((variant, i) =>
        i === index ? { ...variant, isDeleted: true } : variant
      )
    }));
  };

  const updateVariant = (index: number, field: string, value: any) => {
    setFormData(prev => ({
      ...prev,
      variants: prev.variants.map((variant, i) =>
        i === index ? { ...variant, [field]: value } : variant
      )
    }));
  };

  if (!isOpen || !product) return null;

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content product-edit-modal" onClick={e => e.stopPropagation()}>
        <div className="modal-header">
          <h3>Edit Product: {product.name}</h3>
          <button className="modal-close" onClick={onClose}>×</button>
        </div>

        <div className="modal-body">
          <div className="edit-tabs">
            <button
              className={`tab-button ${activeTab === 'base' ? 'active' : ''}`}
              onClick={() => setActiveTab('base')}
            >
              Base Product
            </button>
            <button
              className={`tab-button ${activeTab === 'variants' ? 'active' : ''}`}
              onClick={() => setActiveTab('variants')}
            >
              Variants ({formData.variants.filter(v => !v.isDeleted).length})
            </button>
          </div>

          {activeTab === 'base' && (
            <div className="edit-section">
              <div className="form-group">
                <label htmlFor="product-name">Product Name *</label>
                <input
                  id="product-name"
                  type="text"
                  value={formData.baseProduct.name}
                  onChange={(e) => setFormData(prev => ({
                    ...prev,
                    baseProduct: { ...prev.baseProduct, name: e.target.value }
                  }))}
                  className={errors['baseProduct.name'] ? 'error' : ''}
                  disabled={isSaving}
                />
                {errors['baseProduct.name'] && <span className="error-message">{errors['baseProduct.name']}</span>}
              </div>

              <div className="form-group">
                <label htmlFor="product-description">Description</label>
                <textarea
                  id="product-description"
                  value={formData.baseProduct.description}
                  onChange={(e) => setFormData(prev => ({
                    ...prev,
                    baseProduct: { ...prev.baseProduct, description: e.target.value }
                  }))}
                  rows={3}
                  disabled={isSaving}
                />
              </div>

            </div>
          )}

          {activeTab === 'variants' && (
            <div className="edit-section">
              <div className="variants-header">
                <h4>Product Variants</h4>
                <button
                  type="button"
                  className="btn btn-sm btn-success"
                  onClick={addVariant}
                  disabled={isSaving}
                >
                  ➕ Add Variant
                </button>
              </div>

              <div className="variants-list">
                {formData.variants
                  .map((variant, index) => ({ variant, originalIndex: index }))
                  .filter(({ variant }) => !variant.isDeleted)
                  .map(({ variant, originalIndex }) => (
                    <div key={originalIndex} className="variant-edit-item">
                      <div className="variant-edit-header">
                        <h5>{variant.isNew ? 'New Variant' : `Variant: ${variant.name}`}</h5>
                        <button
                          type="button"
                          className="btn btn-sm btn-danger"
                          onClick={() => removeVariant(originalIndex)}
                          disabled={isSaving}
                        >
                          🗑️ Remove
                        </button>
                      </div>

                      <div className="variant-edit-form">
                        <div className="form-row">
                          <div className="form-group">
                            <label>Name *</label>
                            <input
                              type="text"
                              value={variant.name}
                              onChange={(e) => updateVariant(originalIndex, 'name', e.target.value)}
                              className={errors[`variant.${originalIndex}.name`] ? 'error' : ''}
                              disabled={isSaving}
                            />
                            {errors[`variant.${originalIndex}.name`] && (
                              <span className="error-message">{errors[`variant.${originalIndex}.name`]}</span>
                            )}
                          </div>

                          <div className="form-group">
                            <label>Stock Quantity *</label>
                            <input
                              type="number"
                              min="0"
                              step="0.01"
                              value={variant.stockQuantity}
                              onChange={(e) => updateVariant(originalIndex, 'stockQuantity', parseFloat(e.target.value) || 0)}
                              className={errors[`variant.${originalIndex}.stock`] ? 'error' : ''}
                              disabled={isSaving}
                            />
                            {errors[`variant.${originalIndex}.stock`] && (
                              <span className="error-message">{errors[`variant.${originalIndex}.stock`]}</span>
                            )}
                          </div>
                        </div>

                        <div className="form-row">
                          <div className="form-group">
                            <label>Unit of Measure *</label>
                            <select
                              value={variant.unitOfMeasureId}
                              onChange={(e) => updateVariant(originalIndex, 'unitOfMeasureId', e.target.value)}
                              className={errors[`variant.${originalIndex}.unit`] ? 'error' : ''}
                              disabled={isSaving}
                            >
                              {unitsOfMeasure.map(unit => (
                                <option key={unit.id} value={unit.id}>
                                  {unit.name} ({unit.symbol})
                                </option>
                              ))}
                            </select>
                            {errors[`variant.${originalIndex}.unit`] && (
                              <span className="error-message">{errors[`variant.${originalIndex}.unit`]}</span>
                            )}
                          </div>

                          <div className="form-group">
                            <label>Unit Value *</label>
                            <input
                              type="number"
                              min="0.01"
                              step="0.01"
                              value={variant.unitValue}
                              onChange={(e) => updateVariant(originalIndex, 'unitValue', parseFloat(e.target.value) || 0)}
                              className={errors[`variant.${originalIndex}.unitValue`] ? 'error' : ''}
                              disabled={isSaving}
                            />
                            {errors[`variant.${originalIndex}.unitValue`] && (
                              <span className="error-message">{errors[`variant.${originalIndex}.unitValue`]}</span>
                            )}
                          </div>
                        </div>

                        <div className="form-group">
                          <label>Usage Notes</label>
                          <textarea
                            value={variant.usageNotes}
                            onChange={(e) => updateVariant(originalIndex, 'usageNotes', e.target.value)}
                            rows={2}
                            disabled={isSaving}
                          />
                        </div>
                      </div>
                    </div>
                  ))}
              </div>
            </div>
          )}
        </div>

        <div className="modal-footer">
          <button
            type="button"
            className="btn btn-secondary"
            onClick={onClose}
            disabled={isSaving}
          >
            Cancel
          </button>
          <button
            type="button"
            className="btn btn-primary"
            onClick={handleSave}
            disabled={isSaving}
          >
            {isSaving ? '💾 Saving...' : '💾 Save Changes'}
          </button>
        </div>
      </div>
    </div>
  );
};

export default ProductEditModal;