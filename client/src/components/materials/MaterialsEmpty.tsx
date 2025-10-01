import React from 'react';
import './MaterialsEmpty.css';

interface MaterialsEmptyProps {
  onCreateMaterial?: () => void;
  onBulkImport?: () => void;
}

const MaterialsEmpty: React.FC<MaterialsEmptyProps> = ({
  onCreateMaterial,
  onBulkImport
}) => {
  return (
    <div className="materials-empty">
      <div className="empty-state">
        <div className="empty-icon">
          📦
        </div>
        <h3>No Materials Found</h3>
        <p>
          You haven't added any materials to your inventory yet.
          Start by creating your first material or importing multiple materials at once.
        </p>

        <div className="empty-actions">
          {onCreateMaterial && (
            <button
              className="btn btn-primary"
              onClick={onCreateMaterial}
            >
              ➕ Create First Material
            </button>
          )}

          {onBulkImport && (
            <button
              className="btn btn-secondary"
              onClick={onBulkImport}
            >
              📤 Bulk Import
            </button>
          )}
        </div>

        <div className="empty-tips">
          <h4>Quick Tips:</h4>
          <ul>
            <li>Use descriptive names for easy identification</li>
            <li>Categorize materials for better organization</li>
            <li>Set appropriate stock levels for reordering alerts</li>
            <li>Choose the correct measurement unit for each material</li>
          </ul>
        </div>
      </div>
    </div>
  );
};

export default MaterialsEmpty;