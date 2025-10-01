import React, { useState } from 'react';
import './ProductFilters.css';

export interface ProductFilters {
  searchTerm: string;
  categoryId: string;
  stockStatus: 'all' | 'in-stock' | 'low-stock' | 'out-of-stock';
  minPrice: string;
  maxPrice: string;
  createdFrom: string;
  createdTo: string;
  updatedFrom: string;
  updatedTo: string;
}

interface ProductFilterPanelProps {
  filters: ProductFilters;
  onFiltersChange: (filters: Partial<ProductFilters>) => void;
  onApply: () => void;
  onReset: () => void;
  isOpen: boolean;
  onToggle: () => void;
  categories?: { id: string; name: string }[];
  className?: string;
}

const ProductFilterPanel: React.FC<ProductFilterPanelProps> = ({
  filters,
  onFiltersChange,
  onApply,
  onReset,
  isOpen,
  onToggle,
  categories = [],
  className = ""
}) => {
  const [localFilters, setLocalFilters] = useState<ProductFilters>(filters);

  // Update local filters when props change
  React.useEffect(() => {
    setLocalFilters(filters);
  }, [filters]);

  const handleLocalChange = (updates: Partial<ProductFilters>) => {
    setLocalFilters(prev => ({ ...prev, ...updates }));
  };

  const handleApply = () => {
    onFiltersChange(localFilters);
    onApply();
  };

  const handleReset = () => {
    const resetFilters: ProductFilters = {
      searchTerm: '',
      categoryId: '',
      stockStatus: 'all',
      minPrice: '',
      maxPrice: '',
      createdFrom: '',
      createdTo: '',
      updatedFrom: '',
      updatedTo: ''
    };
    setLocalFilters(resetFilters);
    onReset();
  };

  const handleStockStatusChange = (status: ProductFilters['stockStatus']) => {
    handleLocalChange({ stockStatus: status });
  };

  if (!isOpen) {
    return null;
  }

  return (
    <div className={`product-filter-panel ${className}`}>
      <div className="panel-header">
        <h3>Product Filters</h3>
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
        {/* Search Term */}
        <div className="filter-section">
          <label className="section-label">Search</label>
          <input
            type="text"
            placeholder="Search by name or description..."
            value={localFilters.searchTerm}
            onChange={(e) => handleLocalChange({ searchTerm: e.target.value })}
            className="panel-input"
          />
        </div>

        {/* Category Filter */}
        <div className="filter-section">
          <label className="section-label">Category</label>
          <select
            value={localFilters.categoryId}
            onChange={(e) => handleLocalChange({ categoryId: e.target.value })}
            className="panel-select"
          >
            <option value="">All Categories</option>
            {categories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
        </div>

        {/* Stock Status */}
        <div className="filter-section">
          <label className="section-label">Stock Status</label>
          <div className="status-options">
            {[
              { value: 'all', label: 'All Products' },
              { value: 'in-stock', label: 'In Stock' },
              { value: 'low-stock', label: 'Low Stock' },
              { value: 'out-of-stock', label: 'Out of Stock' }
            ].map((option) => (
              <label key={option.value} className="status-option">
                <input
                  type="radio"
                  name="stockStatus"
                  value={option.value}
                  checked={localFilters.stockStatus === option.value}
                  onChange={(e) => handleStockStatusChange(e.target.value as ProductFilters['stockStatus'])}
                />
                <span className="option-label">{option.label}</span>
              </label>
            ))}
          </div>
        </div>

        {/* Price Range */}
        <div className="filter-section">
          <label className="section-label">Price Range</label>
          <div className="range-inputs">
            <div className="input-group">
              <label htmlFor="minPrice">Minimum ($)</label>
              <input
                id="minPrice"
                type="number"
                placeholder="0"
                value={localFilters.minPrice}
                onChange={(e) => handleLocalChange({ minPrice: e.target.value })}
                className="panel-input"
                min="0"
                step="0.01"
              />
            </div>
            <div className="input-group">
              <label htmlFor="maxPrice">Maximum ($)</label>
              <input
                id="maxPrice"
                type="number"
                placeholder="No limit"
                value={localFilters.maxPrice}
                onChange={(e) => handleLocalChange({ maxPrice: e.target.value })}
                className="panel-input"
                min="0"
                step="0.01"
              />
            </div>
          </div>
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

export default ProductFilterPanel;