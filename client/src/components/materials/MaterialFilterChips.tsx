import React from 'react';
import './MaterialFilters.css';

export interface ActiveFilter {
  key: string;
  label: string;
  value: string;
  type: 'search' | 'category' | 'stock' | 'unit' | 'date';
}

interface MaterialFilterChipsProps {
  filters: ActiveFilter[];
  onRemoveFilter: (filterKey: string) => void;
  onClearAll: () => void;
  className?: string;
}

const MaterialFilterChips: React.FC<MaterialFilterChipsProps> = ({
  filters,
  onRemoveFilter,
  onClearAll,
  className = ""
}) => {
  if (filters.length === 0) {
    return null;
  }

  const getFilterIcon = (type: ActiveFilter['type']) => {
    switch (type) {
      case 'search':
        return (
          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <circle cx="11" cy="11" r="8"></circle>
            <path d="m21 21-4.35-4.35"></path>
          </svg>
        );
      case 'category':
        return (
          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M14.5 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7.5L14.5 2z"></path>
            <polyline points="14,2 14,8 20,8"></polyline>
          </svg>
        );
      case 'stock':
        return (
          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
            <line x1="9" y1="9" x2="15" y2="15"></line>
            <line x1="15" y1="9" x2="9" y2="15"></line>
          </svg>
        );
      case 'unit':
        return (
          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <circle cx="12" cy="12" r="3"></circle>
            <path d="M12 1v6m0 6v6m11-7h-6m-6 0H1m16.24-3.76l-4.24 4.24m-4.24-4.24L5.76 8.24"></path>
          </svg>
        );
      case 'date':
        return (
          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
            <line x1="16" y1="2" x2="16" y2="6"></line>
            <line x1="8" y1="2" x2="8" y2="6"></line>
            <line x1="3" y1="10" x2="21" y2="10"></line>
          </svg>
        );
      default:
        return null;
    }
  };

  return (
    <div className={`material-filter-chips ${className}`}>
      <div className="chips-header">
        <span className="chips-label">Active Filters:</span>
        <button
          type="button"
          onClick={onClearAll}
          className="clear-all-btn"
          aria-label="Clear all filters"
        >
          Clear All
        </button>
      </div>
      <div className="chips-container">
        {filters.map((filter) => (
          <div key={filter.key} className={`filter-chip chip-${filter.type}`}>
            <div className="chip-icon">
              {getFilterIcon(filter.type)}
            </div>
            <span className="chip-label">{filter.label}:</span>
            <span className="chip-value">{filter.value}</span>
            <button
              type="button"
              onClick={() => onRemoveFilter(filter.key)}
              className="chip-remove-btn"
              aria-label={`Remove ${filter.label} filter`}
            >
              <svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <line x1="18" y1="6" x2="6" y2="18"></line>
                <line x1="6" y1="6" x2="18" y2="18"></line>
              </svg>
            </button>
          </div>
        ))}
      </div>
    </div>
  );
};

export default MaterialFilterChips;