import React from 'react';
import { ProductListDto, ProductVariantDto, UnitOfMeasureDto } from '../../services/api.types';
import { ProductTableRow } from './index';

interface ProductTableProps {
  products: ProductListDto[];
  unitsOfMeasure?: UnitOfMeasureDto[];
  sortBy?: 'name' | 'createdAt' | 'updatedAt';
  sortOrder?: 'asc' | 'desc';
  onSort: (sortBy: 'name' | 'createdAt' | 'updatedAt', sortOrder: 'asc' | 'desc') => void;
  onProductSelect?: (product: ProductListDto) => void;
  onProductEdit?: (product: ProductListDto) => void;
  onProductDelete?: (product: ProductListDto) => void;
  onVariantView?: (variantId: string) => void;
  onVariantUpdate?: (variantId: string, updatedVariant: ProductVariantDto) => void;
  showActions?: boolean;
  onTouchStart?: (e: React.TouchEvent, productId: string) => void;
  onTouchMove?: (e: React.TouchEvent) => void;
  onTouchEnd?: () => void;
  onKeyDown?: (e: React.KeyboardEvent, product: ProductListDto) => void;
}

/**
 * Table component for displaying products list with sorting
 */
const ProductTable: React.FC<ProductTableProps> = React.memo(({
  products,
  unitsOfMeasure = [],
  sortBy,
  sortOrder,
  onSort,
  onProductSelect,
  onProductEdit,
  onProductDelete,
  onVariantView,
  onVariantUpdate,
  showActions = true,
  onTouchStart,
  onTouchMove,
  onTouchEnd,
  onKeyDown
}) => {
  // Memoized sort handler
  const handleSort = React.useCallback((column: 'name' | 'createdAt' | 'updatedAt') => {
    const newSortOrder = sortBy === column && sortOrder === 'asc' ? 'desc' : 'asc';
    onSort(column, newSortOrder);
  }, [sortBy, sortOrder, onSort]);

  // Memoized sort icon
  const getSortIcon = React.useCallback((column: 'name' | 'createdAt' | 'updatedAt') => {
    if (sortBy !== column) return '↕️';
    return sortOrder === 'asc' ? '↑' : '↓';
  }, [sortBy, sortOrder]);

  // Helper functions to reduce cognitive complexity
  const getAriaSort = (column: 'name' | 'createdAt' | 'updatedAt'): 'ascending' | 'descending' | undefined => {
    if (sortBy !== column) return undefined;
    return sortOrder === 'asc' ? 'ascending' : 'descending';
  };

  const getNextSortOrderLabel = (column: 'name' | 'createdAt' | 'updatedAt'): 'ascending' | 'descending' => {
    if (sortBy === column) {
      return sortOrder === 'asc' ? 'descending' : 'ascending';
    }
    return 'ascending';
  };

  const nameAriaSort = getAriaSort('name');
  const createdAtAriaSort = getAriaSort('createdAt');

  return (
    <div className="products-table-container" role="region" aria-label="Products table" aria-live="polite">
      <table
        className="products-table"
        role="table"
        aria-label={`Products table with ${products.length} products`}
        aria-rowcount={products.length}
      >
        <thead>
          <tr role="row">
            <th
              aria-sort={nameAriaSort}
              className="sortable-header"
              scope="col"
              role="columnheader"
            >
              <button
                type="button"
                className="sortable-header"
                onClick={() => handleSort('name')}
                aria-label={`Sort by name ${getNextSortOrderLabel('name')}`}
                aria-pressed={sortBy === 'name'}
              >
                Product Name {getSortIcon('name')}
              </button>
            </th>
            <th scope="col" role="columnheader">Description</th>
            <th scope="col" role="columnheader">Category</th>
            <th scope="col" role="columnheader" aria-label="Base price in dollars">Base Price</th>
            <th scope="col" role="columnheader">Stock Status</th>
            <th scope="col" role="columnheader" aria-label="Number of product variants">Variants</th>
            <th
              aria-sort={createdAtAriaSort}
              className="sortable-header"
              scope="col"
              role="columnheader"
            >
              <button
                type="button"
                className="sortable-header"
                onClick={() => handleSort('createdAt')}
                aria-label={`Sort by created date ${getNextSortOrderLabel('createdAt')}`}
                aria-pressed={sortBy === 'createdAt'}
              >
                Created {getSortIcon('createdAt')}
              </button>
            </th>
            {showActions && (
              <th scope="col" role="columnheader" aria-label="Available actions">Actions</th>
            )}
          </tr>
        </thead>
        <tbody>
          {products.map((product) => (
            <ProductTableRow
              key={product.id}
              product={product}
              unitsOfMeasure={unitsOfMeasure}
              onSelect={onProductSelect}
              onEdit={onProductEdit}
              onDelete={onProductDelete}
              onVariantView={onVariantView}
              onVariantUpdate={onVariantUpdate}
              showActions={showActions}
              onTouchStart={onTouchStart}
              onTouchMove={onTouchMove}
              onTouchEnd={onTouchEnd}
              onKeyDown={onKeyDown}
            />
          ))}
        </tbody>
      </table>
    </div>
  );
});

ProductTable.displayName = 'ProductTable';

export default ProductTable;