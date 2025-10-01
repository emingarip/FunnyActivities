import React from 'react';
import './MaterialsLoading.css';

interface MaterialsLoadingProps {
  /** Custom loading message */
  message?: string;
  /** Custom description */
  description?: string;
  /** Show progress indicator */
  showProgress?: boolean;
  /** Progress percentage (0-100) */
  progress?: number;
  /** Number of skeleton rows to show */
  skeletonRows?: number;
  /** Loading variant */
  variant?: 'default' | 'minimal' | 'skeleton-only';
}

const MaterialsLoading: React.FC<MaterialsLoadingProps> = ({
  message = "Loading Materials...",
  description = "Please wait while we fetch your materials data.",
  showProgress = false,
  progress = 0,
  skeletonRows = 5,
  variant = 'default'
}) => {
  if (variant === 'minimal') {
    return (
      <div className="materials-loading minimal" role="status" aria-live="polite">
        <div className="loading-container">
          <div className="loading-spinner" aria-hidden="true"></div>
          <span className="sr-only">{message}</span>
        </div>
      </div>
    );
  }

  if (variant === 'skeleton-only') {
    return (
      <div className="skeleton-table" role="status" aria-live="polite" aria-label="Loading data">
        <div className="skeleton-header">
          <div className="skeleton-cell skeleton-name"></div>
          <div className="skeleton-cell skeleton-category"></div>
          <div className="skeleton-cell skeleton-stock"></div>
          <div className="skeleton-cell skeleton-unit"></div>
          <div className="skeleton-cell skeleton-updated"></div>
          <div className="skeleton-cell skeleton-actions"></div>
        </div>
        {Array.from({ length: skeletonRows }, () => ({ id: crypto.randomUUID() })).map((row) => (
          <div key={row.id} className="skeleton-row">
            <div className="skeleton-cell skeleton-name"></div>
            <div className="skeleton-cell skeleton-category"></div>
            <div className="skeleton-cell skeleton-stock"></div>
            <div className="skeleton-cell skeleton-unit"></div>
            <div className="skeleton-cell skeleton-updated"></div>
            <div className="skeleton-cell skeleton-actions"></div>
          </div>
        ))}
      </div>
    );
  }

  return (
    <div className="materials-loading" role="status" aria-live="polite">
      <div className="loading-container">
        <div className="loading-spinner" aria-hidden="true"></div>
        <h3>{message}</h3>
        <p>{description}</p>

        {showProgress && (
          <div className="loading-progress">
            <div className="progress-bar">
              <div
                className="progress-fill"
                style={{ width: `${Math.min(100, Math.max(0, progress))}%` }}
                aria-valuenow={progress}
                aria-valuemin={0}
                aria-valuemax={100}
                role="progressbar"
                aria-label={`Loading progress: ${progress}%`}
              />
            </div>
            <div className="progress-text">
              {progress}% complete
            </div>
          </div>
        )}
      </div>

      {/* Skeleton table for better UX */}
      <div className="skeleton-table" aria-hidden="true">
        <div className="skeleton-header">
          <div className="skeleton-cell skeleton-name"></div>
          <div className="skeleton-cell skeleton-category"></div>
          <div className="skeleton-cell skeleton-stock"></div>
          <div className="skeleton-cell skeleton-unit"></div>
          <div className="skeleton-cell skeleton-updated"></div>
          <div className="skeleton-cell skeleton-actions"></div>
        </div>
        {Array.from({ length: skeletonRows }, () => ({ id: crypto.randomUUID() })).map((row) => (
          <div key={row.id} className="skeleton-row">
            <div className="skeleton-cell skeleton-name"></div>
            <div className="skeleton-cell skeleton-category"></div>
            <div className="skeleton-cell skeleton-stock"></div>
            <div className="skeleton-cell skeleton-unit"></div>
            <div className="skeleton-cell skeleton-updated"></div>
            <div className="skeleton-cell skeleton-actions"></div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default MaterialsLoading;