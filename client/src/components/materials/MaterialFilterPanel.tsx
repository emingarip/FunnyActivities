import React, { useState } from 'react';
import './MaterialFilters.css';

export interface AdvancedFilters {
  minStock: string;
  maxStock: string;
  unit: string;
  createdFrom: string;
  createdTo: string;
  updatedFrom: string;
  updatedTo: string;
  stockStatus: 'all' | 'in-stock' | 'low-stock' | 'out-of-stock';
}

interface MaterialFilterPanelProps {
  filters: AdvancedFilters;
  onFiltersChange: (filters: Partial<AdvancedFilters>) => void;
  onApply: () => void;
  onReset: () => void;
  isOpen: boolean;
  onToggle: () => void;
  className?: string;
}

const MaterialFilterPanel: React.FC<MaterialFilterPanelProps> = ({
  filters,
  onFiltersChange,
  onApply,
  onReset,
  isOpen,
  onToggle,
  className = ""
}) => {
  const [localFilters, setLocalFilters] = useState<AdvancedFilters>(filters);

  // Update local filters when props change
  React.useEffect(() => {
    setLocalFilters(filters);
  }, [filters]);

  const handleLocalChange = (updates: Partial<AdvancedFilters>) => {
    setLocalFilters(prev => ({ ...prev, ...updates }));
  };

  const handleApply = () => {
    onFiltersChange(localFilters);
    onApply();
  };

  const handleReset = () => {
    const resetFilters: AdvancedFilters = {
      minStock: '',
      maxStock: '',
      unit: '',
      createdFrom: '',
      createdTo: '',
      updatedFrom: '',
      updatedTo: '',
      stockStatus: 'all'
    };
    setLocalFilters(resetFilters);
    onReset();
  };

  const handleStockStatusChange = (status: AdvancedFilters['stockStatus']) => {
    handleLocalChange({ stockStatus: status });

    // Auto-fill stock values based on status
    switch (status) {
      case 'in-stock':
        handleLocalChange({ minStock: '1', maxStock: '' });
        break;
      case 'low-stock':
        handleLocalChange({ minStock: '0', maxStock: '10' });
        break;
      case 'out-of-stock':
        handleLocalChange({ minStock: '0', maxStock: '0' });
        break;
      default:
        handleLocalChange({ minStock: '', maxStock: '' });
    }
  };

  if (!isOpen) {
    return null;
  }

  return (
    <div className={`material-filter-panel ${className}`}>
      <div className="panel-header">
        <h3>Advanced Filters</h3>
        <button
          type="button"
          onClick={onToggle}
          className="panel-close-btn"
          aria-label="Close filter panel"
        >
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <line x1="18" y1="6" x2="6" y2="18"></line>
            <line x1="6" y1="6" x2="18" y2="18"></line>
          </svg>
        </button>
      </div>

      <div className="panel-content">
        {/* Stock Status Presets */}
        <div className="filter-section">
          <label className="section-label">Stock Status</label>
          <div className="status-options">
            {[
              { value: 'all', label: 'All Items' },
              { value: 'in-stock', label: 'In Stock' },
              { value: 'low-stock', label: 'Low Stock (≤10)' },
              { value: 'out-of-stock', label: 'Out of Stock' }
            ].map((option) => (
              <label key={option.value} className="status-option">
                <input
                  type="radio"
                  name="stockStatus"
                  value={option.value}
                  checked={localFilters.stockStatus === option.value}
                  onChange={(e) => handleStockStatusChange(e.target.value as AdvancedFilters['stockStatus'])}
                />
                <span className="option-label">{option.label}</span>
              </label>
            ))}
          </div>
        </div>

        {/* Custom Stock Range */}
        <div className="filter-section">
          <label className="section-label">Custom Stock Range</label>
          <div className="range-inputs">
            <div className="input-group">
              <label htmlFor="minStock">Minimum</label>
              <input
                id="minStock"
                type="number"
                placeholder="0"
                value={localFilters.minStock}
                onChange={(e) => handleLocalChange({ minStock: e.target.value })}
                className="panel-input"
                min="0"
                step="0.01"
              />
            </div>
            <div className="input-group">
              <label htmlFor="maxStock">Maximum</label>
              <input
                id="maxStock"
                type="number"
                placeholder="No limit"
                value={localFilters.maxStock}
                onChange={(e) => handleLocalChange({ maxStock: e.target.value })}
                className="panel-input"
                min="0"
                step="0.01"
              />
            </div>
          </div>
        </div>

        {/* Unit Filter */}
        <div className="filter-section">
          <label className="section-label">Measurement Unit</label>
          <select
            value={localFilters.unit}
            onChange={(e) => handleLocalChange({ unit: e.target.value })}
            className="panel-select"
          >
            <option value="">All Units</option>
            <option value="pieces">Pieces</option>
            <option value="pcs">Pcs</option>
            <option value="kg">Kilograms (kg)</option>
            <option value="kilograms">Kilograms</option>
            <option value="liters">Liters</option>
            <option value="l">L</option>
            <option value="meters">Meters</option>
            <option value="m">M</option>
            <option value="square meters">Square Meters</option>
            <option value="m²">M²</option>
            <option value="tons">Tons</option>
            <option value="boxes">Boxes</option>
            <option value="packs">Packs</option>
            <option value="rolls">Rolls</option>
          </select>
        </div>

        {/* Date Filters */}
        <div className="filter-section">
          <label className="section-label">Created Date Range</label>
          <div className="date-range">
            <div className="input-group">
              <label htmlFor="createdFrom">From</label>
              <input
                id="createdFrom"
                type="date"
                value={localFilters.createdFrom}
                onChange={(e) => handleLocalChange({ createdFrom: e.target.value })}
                className="panel-input"
              />
            </div>
            <div className="input-group">
              <label htmlFor="createdTo">To</label>
              <input
                id="createdTo"
                type="date"
                value={localFilters.createdTo}
                onChange={(e) => handleLocalChange({ createdTo: e.target.value })}
                className="panel-input"
              />
            </div>
          </div>
        </div>

        <div className="filter-section">
          <label className="section-label">Updated Date Range</label>
          <div className="date-range">
            <div className="input-group">
              <label htmlFor="updatedFrom">From</label>
              <input
                id="updatedFrom"
                type="date"
                value={localFilters.updatedFrom}
                onChange={(e) => handleLocalChange({ updatedFrom: e.target.value })}
                className="panel-input"
              />
            </div>
            <div className="input-group">
              <label htmlFor="updatedTo">To</label>
              <input
                id="updatedTo"
                type="date"
                value={localFilters.updatedTo}
                onChange={(e) => handleLocalChange({ updatedTo: e.target.value })}
                className="panel-input"
              />
            </div>
          </div>
        </div>
      </div>

      <div className="panel-actions">
        <button
          type="button"
          onClick={handleReset}
          className="btn btn-secondary"
        >
          Reset All
        </button>
        <button
          type="button"
          onClick={handleApply}
          className="btn btn-primary"
        >
          Apply Filters
        </button>
      </div>
    </div>
  );
};

export default MaterialFilterPanel;