import React from 'react';
import { MaterialListDto } from '../../services/api.types';
import PhotoPreview from './PhotoPreview';
import { formatStockQuantity, getStockStatus, getUnitDisplayName } from './utils/formattingUtils';

/**
 * Props for MaterialsTableRow component
 */
interface MaterialsTableRowProps {
  /** The material data to display */
  material: MaterialListDto;
  /** Callback when material is selected */
  onSelect?: (material: MaterialListDto) => void;
  /** Callback when material is edited */
  onEdit?: (material: MaterialListDto) => void;
  /** Callback when material is deleted */
  onDelete?: (material: MaterialListDto) => void;
  /** Callback when material photo is clicked */
  onPhotoClick?: (material: MaterialListDto) => void;
  /** Whether to show action buttons */
  showActions?: boolean;
}

/**
 * Individual table row component for materials with improved performance
 */
const MaterialsTableRow: React.FC<MaterialsTableRowProps> = React.memo(({
  material,
  onSelect,
  onEdit,
  onDelete,
  onPhotoClick,
  showActions = true
}) => {
  // Memoized event handlers for performance
  const handleRowClick = React.useCallback(() => {
    onSelect?.(material);
  }, [onSelect, material]);

  const handleEdit = React.useCallback((e: React.MouseEvent) => {
    e.stopPropagation();
    onEdit?.(material);
  }, [onEdit, material]);

  const handleDelete = React.useCallback((e: React.MouseEvent) => {
    e.stopPropagation();
    onDelete?.(material);
  }, [onDelete, material]);

  const handlePhotoClick = React.useCallback(() => {
    onPhotoClick?.(material);
  }, [onPhotoClick, material]);


  // Memoized formatted values for performance
  const formattedStockQuantity = React.useMemo(
    () => formatStockQuantity(material.stockQuantity, material.unit),
    [material.stockQuantity, material.unit]
  );

  const stockStatusClass = React.useMemo(
    () => getStockStatus(material.stockQuantity),
    [material.stockQuantity]
  );

  const unitDisplayName = React.useMemo(
    () => getUnitDisplayName(material.unit),
    [material.unit]
  );

  return (
    <tr
      className={`materials-table-row ${onSelect ? 'clickable' : ''}`}
      onClick={handleRowClick}
    >
      <td className="material-name">
        <div className="material-info">
          <span className="name">{material.name}</span>
        </div>
      </td>
      <td className="material-photos">
        <PhotoPreview
          materialId={material.id}
          photoCount={material.photoCount || 0}
          thumbnailUrl={material.thumbnailUrl}
          onClick={handlePhotoClick}
          size="small"
        />
      </td>
      <td className="material-category">
        <span className={`category-badge ${material.category ? 'has-category' : 'no-category'}`}>
          {material.category || 'Uncategorized'}
        </span>
      </td>
      <td className="material-stock">
        <span className={`stock-quantity ${stockStatusClass}`}>
          {formattedStockQuantity}
        </span>
      </td>
      <td className="material-unit">
        <span className="unit-display">
          {unitDisplayName}
        </span>
      </td>
      <td className="material-updated">
        <span className="updated-date">
          {/* For now, we'll show a placeholder since MaterialListDto doesn't include updated date */}
          Recent
        </span>
      </td>
      {showActions && (
        <td className="material-actions">
          <div className="action-buttons">
            {onEdit && (
              <button
                className="btn btn-sm btn-outline-primary"
                onClick={handleEdit}
                title="Edit material"
                aria-label={`Edit ${material.name}`}
              >
                ✏️
              </button>
            )}
            {onDelete && (
              <button
                className="btn btn-sm btn-outline-danger"
                onClick={handleDelete}
                title="Delete material"
                aria-label={`Delete ${material.name}`}
              >
                🗑️
              </button>
            )}
          </div>
        </td>
      )}
    </tr>
  );
});

MaterialsTableRow.displayName = 'MaterialsTableRow';

export default MaterialsTableRow;