import React, { useState } from 'react';
import { PhotoCamera as PhotoIcon } from '@mui/icons-material';
import { LazyLoadImage } from 'react-lazy-load-image-component';
import 'react-lazy-load-image-component/src/effects/blur.css';
import './PhotoPreview.css';

interface PhotoPreviewProps {
  materialId: string;
  photoCount: number;
  thumbnailUrl?: string;
  onClick?: () => void;
  size?: 'small' | 'medium' | 'large';
  showCount?: boolean;
}

const PhotoPreview: React.FC<PhotoPreviewProps> = ({
  materialId,
  photoCount,
  thumbnailUrl,
  onClick,
  size = 'medium',
  showCount = true
}) => {
  const [imageError, setImageError] = useState(false);

  const handleClick = () => {
    if (onClick) {
      onClick();
    }
  };

  if (photoCount === 0) {
    return (
      <div
        className={`photo-preview photo-preview-${size}`}
        onClick={handleClick}
        title="No photos available"
      >
        <PhotoIcon className="photo-icon" />
      </div>
    );
  }

  return (
    <div
      className={`photo-preview photo-preview-${size}`}
      onClick={handleClick}
      title={`${photoCount} photo${photoCount > 1 ? 's' : ''} available`}
    >
      {thumbnailUrl ? (
        <LazyLoadImage
          src={thumbnailUrl}
          alt="Material thumbnail"
          effect="blur"
          placeholder={
            <div className="loading-container">
              <div className="loading-spinner" />
            </div>
          }
          onError={() => setImageError(true)}
          style={{ width: '100%', height: '100%', objectFit: 'cover' }}
        />
      ) : (
        <div className="placeholder-container">
          <PhotoIcon className="photo-icon" />
        </div>
      )}

      {imageError && (
        <div className="error-container">
          <PhotoIcon className="photo-icon" />
        </div>
      )}

      {showCount && photoCount > 1 && (
        <div className="photo-count">
          {photoCount > 99 ? '99+' : photoCount}
        </div>
      )}
    </div>
  );
};

export default PhotoPreview;