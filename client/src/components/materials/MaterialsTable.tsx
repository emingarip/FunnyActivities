import React from 'react';
import { MaterialListDto } from '../../services/api.types';
import MaterialsTableRow from './MaterialsTableRow';

interface MaterialsTableProps {
  materials: MaterialListDto[];
  sortBy?: 'name' | 'stockQuantity';
  sortOrder?: 'asc' | 'desc';
  onSort: (sortBy: 'name' | 'stockQuantity', sortOrder: 'asc' | 'desc') => void;
  onMaterialSelect?: (material: MaterialListDto) => void;
  onMaterialEdit?: (material: MaterialListDto) => void;
  onMaterialDelete?: (material: MaterialListDto) => void;
  onPhotoClick?: (material: MaterialListDto) => void;
  showActions?: boolean;
}

/**
 * Table component for displaying materials list with sorting
 */
const MaterialsTable: React.FC<MaterialsTableProps> = React.memo(({
  materials,
  sortBy,
  sortOrder,
  onSort,
  onMaterialSelect,
  onMaterialEdit,
  onMaterialDelete,
  onPhotoClick,
  showActions = true
}) => {
  // Memoized sort handler
  const handleSort = React.useCallback((column: 'name' | 'stockQuantity') => {
    const newSortOrder = sortBy === column && sortOrder === 'asc' ? 'desc' : 'asc';
    onSort(column, newSortOrder);
  }, [sortBy, sortOrder, onSort]);

  // Memoized sort icon
  const getSortIcon = React.useCallback((column: 'name' | 'stockQuantity') => {
    if (sortBy !== column) return '↕️';
    return sortOrder === 'asc' ? '↑' : '↓';
  }, [sortBy, sortOrder]);

  // Helper functions to reduce cognitive complexity
  const getAriaSort = (column: 'name' | 'stockQuantity'): 'ascending' | 'descending' | undefined => {
    if (sortBy !== column) return undefined;
    return sortOrder === 'asc' ? 'ascending' : 'descending';
  };

  const getNextSortOrderLabel = (column: 'name' | 'stockQuantity'): 'ascending' | 'descending' => {
    if (sortBy === column) {
      return sortOrder === 'asc' ? 'descending' : 'ascending';
    }
    return 'ascending';
  };

  const nameAriaSort = getAriaSort('name');
  const stockAriaSort = getAriaSort('stockQuantity');
  const nextSortOrderForName = getNextSortOrderLabel('name');

  return (
    <div className="materials-table-container">
      <table className="materials-table">
        <thead>
          <tr>
            <th aria-sort={nameAriaSort} className="sortable-header">
              <button
                type="button"
                className="sortable-header"
                onClick={() => handleSort('name')}
                aria-label={`Sort by name ${nextSortOrderForName}`}
              >
                Name {getSortIcon('name')}
              </button>
            </th>
            <th>Photos</th>
            <th>Category</th>
            <th aria-sort={stockAriaSort} className="sortable-header">
              <button
                type="button"
                className="sortable-header"
                onClick={() => handleSort('stockQuantity')}
                aria-label={`Sort by stock ${getNextSortOrderLabel('stockQuantity')}`}
              >
                Stock {getSortIcon('stockQuantity')}
              </button>
            </th>
            <th>Unit</th>
            <th>Updated</th>
            {showActions && <th>Actions</th>}
          </tr>
        </thead>
        <tbody>
          {materials.map((material) => (
            <MaterialsTableRow
              key={material.id}
              material={material}
              onSelect={onMaterialSelect}
              onEdit={onMaterialEdit}
              onDelete={onMaterialDelete}
              onPhotoClick={onPhotoClick}
              showActions={showActions}
            />
          ))}
        </tbody>
      </table>
    </div>
  );
});

MaterialsTable.displayName = 'MaterialsTable';

export default MaterialsTable;