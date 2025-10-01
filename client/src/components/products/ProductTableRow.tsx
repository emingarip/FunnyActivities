import React, { useState } from 'react';
import { ProductListDto, ProductVariantDto, UnitOfMeasureDto } from '../../services/api.types';
import './ProductTableRow.css';
import './ProductInlineEditor.css';
import ProductInlineEditor from './ProductInlineEditor';

/**
 * Props for ProductTableRow component
 */
interface ProductTableRowProps {
  /** The product data to display */
  product: ProductListDto;
  /** Available units of measure for editing */
  unitsOfMeasure?: UnitOfMeasureDto[];
  /** Callback when product is selected */
  onSelect?: (product: ProductListDto) => void;
  /** Callback when product is edited */
  onEdit?: (product: ProductListDto) => void;
  /** Callback when product is deleted */
  onDelete?: (product: ProductListDto) => void;
  /** Callback when a variant is viewed */
  onVariantView?: (variantId: string) => void;
  /** Callback when a variant is updated */
  onVariantUpdate?: (variantId: string, updatedVariant: ProductVariantDto) => void;
  /** Whether to show action buttons */
  showActions?: boolean;
  /** Touch event handlers for mobile interactions */
  onTouchStart?: (e: React.TouchEvent, productId: string) => void;
  onTouchMove?: (e: React.TouchEvent) => void;
  onTouchEnd?: () => void;
  /** Keyboard event handler for accessibility */
  onKeyDown?: (e: React.KeyboardEvent, product: ProductListDto) => void;
}

/**
 * Individual table row component for products with variants
 */
const ProductTableRow: React.FC<ProductTableRowProps> = React.memo(({
  product,
  unitsOfMeasure = [],
  onSelect,
  onEdit,
  onDelete,
  onVariantView,
  onVariantUpdate,
  showActions = true,
  onTouchStart,
  onTouchMove,
  onTouchEnd,
  onKeyDown
}) => {
  // State for tracking which variant is being edited
  const [editingVariantId, setEditingVariantId] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string>('');
  // Memoized event handlers for performance
  const handleRowClick = React.useCallback(() => {
    onSelect?.(product);
  }, [onSelect, product]);

  const handleEdit = React.useCallback((e: React.MouseEvent) => {
    e.stopPropagation();
    onEdit?.(product);
  }, [onEdit, product]);

  const handleDelete = React.useCallback((e: React.MouseEvent) => {
    e.stopPropagation();
    onDelete?.(product);
  }, [onDelete, product]);

  const handleVariantView = React.useCallback((variantId: string) => (e: React.MouseEvent) => {
    e.stopPropagation();
    onVariantView?.(variantId);
  }, [onVariantView]);

  const handleVariantEdit = React.useCallback((variantId: string) => (e: React.MouseEvent) => {
    e.stopPropagation();
    setEditingVariantId(variantId);
    setErrorMessage('');
  }, []);

  const handleVariantSave = React.useCallback((updatedVariant: ProductVariantDto) => {
    setEditingVariantId(null);
    setErrorMessage('');
    onVariantUpdate?.(updatedVariant.id, updatedVariant);
  }, [onVariantUpdate]);

  const handleVariantCancel = React.useCallback(() => {
    setEditingVariantId(null);
    setErrorMessage('');
  }, []);

  const handleVariantError = React.useCallback((error: string) => {
    setErrorMessage(error);
  }, []);

  // Format stock status
  const getStockStatusDisplay = React.useCallback((status: string) => {
    switch (status) {
      case 'in-stock':
        return <span className="stock-status in-stock">In Stock</span>;
      case 'low-stock':
        return <span className="stock-status low-stock">Low Stock</span>;
      case 'out-of-stock':
        return <span className="stock-status out-of-stock">Out of Stock</span>;
      default:
        return <span className="stock-status unknown">Unknown</span>;
    }
  }, []);

  // Format price
  const formatPrice = React.useCallback((price?: number) => {
    if (price === undefined || price === null) return 'N/A';
    return `$${price.toFixed(2)}`;
  }, []);

  // Get variant details summary
  const getVariantSummary = React.useCallback((variants: ProductVariantDto[]) => {
    if (variants.length === 0) return 'No variants';

    const sizes = Array.from(new Set(variants.map(v => v.dynamicProperties?.size).filter(Boolean)));
    const colors = Array.from(new Set(variants.map(v => v.dynamicProperties?.color).filter(Boolean)));

    const parts = [];
    if (sizes.length > 0) parts.push(`Sizes: ${sizes.join(', ')}`);
    if (colors.length > 0) parts.push(`Colors: ${colors.join(', ')}`);

    return parts.length > 0 ? parts.join(' | ') : `${variants.length} variant${variants.length > 1 ? 's' : ''}`;
  }, []);

  // Calculate total stock across all variants
  const totalStock = React.useMemo(() => {
    return product.variants.reduce((sum, variant) => sum + variant.stockQuantity, 0);
  }, [product.variants]);

  return (
    <>
      {errorMessage && (
        <tr className="error-row" role="row">
          <td colSpan={showActions ? 7 : 6} className="error-cell" role="cell">
            <div className="alert alert-danger alert-sm" role="alert" aria-live="assertive">
              <strong>❌ Update Failed:</strong> {errorMessage}
              <button
                type="button"
                className="btn-close float-end"
                onClick={() => setErrorMessage('')}
                aria-label="Close error message"
              />
            </div>
          </td>
        </tr>
      )}
      <tr
        className={`products-table-row ${onSelect ? 'clickable' : ''}`}
        onClick={handleRowClick}
        role="row"
        tabIndex={onSelect ? 0 : -1}
        aria-label={`Product: ${product.name}`}
        onTouchStart={onTouchStart ? (e) => onTouchStart(e, product.id) : undefined}
        onTouchMove={onTouchMove}
        onTouchEnd={onTouchEnd}
        onKeyDown={onKeyDown ? (e) => onKeyDown(e, product) : (e) => {
          if (onSelect && (e.key === 'Enter' || e.key === ' ')) {
            e.preventDefault();
            handleRowClick();
          }
        }}
      >
      <td className="product-name" role="cell" aria-label="Product name">
        <div className="product-info">
          <span className="name">{product.name}</span>
        </div>
      </td>
      <td className="product-description" role="cell" aria-label="Product description">
        <span className="description">
          {product.description || 'No description'}
        </span>
      </td>
      <td className="product-category" role="cell" aria-label="Product category">
        <span className={`category-badge ${product.categoryName ? 'has-category' : 'no-category'}`}>
          {product.categoryName || 'Uncategorized'}
        </span>
      </td>
      <td className="product-price" role="cell" aria-label={`Base price: ${formatPrice(product.basePrice)}`}>
        <span className="price">
          {formatPrice(product.basePrice)}
        </span>
      </td>
      <td className="product-stock-status" role="cell" aria-label={`Stock status: ${product.stockStatus.replace('-', ' ')}, Total units: ${totalStock}`}>
        {getStockStatusDisplay(product.stockStatus)}
        <div className="stock-details">
          Total: {totalStock} units
        </div>
      </td>
      <td className="product-variants" role="cell" aria-label={`Product variants: ${product.totalVariants} total`}>
        <div className="variants-summary">
          <span className="variant-count">{product.totalVariants} variant{product.totalVariants !== 1 ? 's' : ''}</span>
          <div className="variant-details">
            {getVariantSummary(product.variants)}
          </div>
          {product.variants.length > 0 && (
            <div className="variant-list" role="list" aria-label="Variant details">
              {product.variants.slice(0, 2).map((variant) => (
                <div key={variant.id} className="variant-item" role="listitem">
                  {editingVariantId === variant.id ? (
                    <ProductInlineEditor
                      variant={variant}
                      unitsOfMeasure={unitsOfMeasure}
                      onSave={handleVariantSave}
                      onCancel={handleVariantCancel}
                      onError={handleVariantError}
                    />
                  ) : (
                    <>
                      <span className="variant-name">{variant.name}</span>
                      <span className="variant-stock">({variant.stockQuantity} {variant.unitSymbol})</span>
                      <div className="variant-actions" role="group" aria-label={`Actions for ${variant.name}`}>
                        {onVariantUpdate && (
                          <button
                            className="btn btn-sm btn-link variant-edit-btn"
                            onClick={handleVariantEdit(variant.id)}
                            title={`Edit ${variant.name}`}
                            aria-label={`Edit variant ${variant.name}`}
                          >
                            ✏️
                          </button>
                        )}
                        {onVariantView && (
                          <button
                            className="btn btn-sm btn-link variant-view-btn"
                            onClick={handleVariantView(variant.id)}
                            title={`View ${variant.name} details`}
                            aria-label={`View details for variant ${variant.name}`}
                          >
                            👁️
                          </button>
                        )}
                      </div>
                    </>
                  )}
                </div>
              ))}
              {product.variants.length > 2 && (
                <div className="variant-more" aria-label={`${product.variants.length - 2} more variants available`}>
                  +{product.variants.length - 2} more...
                </div>
              )}
            </div>
          )}
        </div>
      </td>
      <td className="product-created" role="cell" aria-label={`Created on ${new Date(product.createdAt).toLocaleDateString()}`}>
        <span className="created-date">
          {new Date(product.createdAt).toLocaleDateString()}
        </span>
      </td>
      {showActions && (
        <td className="product-actions" role="cell">
          <div className="action-buttons" role="group" aria-label="Product actions">
            {onEdit && (
              <button
                className="btn btn-sm btn-outline-primary"
                onClick={handleEdit}
                title="Edit product"
                aria-label={`Edit ${product.name}`}
              >
                ✏️
              </button>
            )}
            {onDelete && (
              <button
                className="btn btn-sm btn-outline-danger"
                onClick={handleDelete}
                title="Delete product"
                aria-label={`Delete ${product.name}`}
              >
                🗑️
              </button>
            )}
          </div>
        </td>
      )}
    </tr>
    </>
  );
});

ProductTableRow.displayName = 'ProductTableRow';

export default ProductTableRow;