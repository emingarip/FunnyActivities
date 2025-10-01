import React, { useState, useCallback, useRef } from 'react';
import {
  Box,
  Typography,
  Button,
  Paper,
  Grid,
  IconButton,
  LinearProgress,
  Alert,
  Chip,
  Card,
  CardContent,
  CardActions,
} from '@mui/material';
import {
  CloudUpload as CloudUploadIcon,
  Delete as DeleteIcon,
  PhotoCamera as PhotoCameraIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  InsertDriveFile as FileIcon,
} from '@mui/icons-material';
import { LazyLoadImage } from 'react-lazy-load-image-component';
import 'react-lazy-load-image-component/src/effects/blur.css';
import { materialsAPI } from '../../services/api';
import { MaterialPhoto } from '../../services/api.types';
import { optimizeImages, MOBILE_OPTIMIZATION_OPTIONS, OptimizedImageResult } from './utils/imageOptimizationUtils';
import './MultiPhotoUpload.css';

interface FileWithPreview {
  file: File;
  preview: string;
  id: string;
  error?: string;
  uploaded?: boolean;
  progress?: number;
}

interface MultiPhotoUploadProps {
  materialId: string;
  maxFiles?: number;
  maxFileSize?: number; // in MB
  acceptedTypes?: string[];
  existingPhotos?: MaterialPhoto[];
  onUploadSuccess?: (uploadedUrls: string[]) => void;
  onUploadError?: (error: string) => void;
  disabled?: boolean;
}

const MultiPhotoUpload: React.FC<MultiPhotoUploadProps> = ({
  materialId,
  maxFiles = 10,
  maxFileSize = 5, // 5MB default
  acceptedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'],
  existingPhotos = [],
  onUploadSuccess,
  onUploadError,
  disabled = false,
}) => {
  const [files, setFiles] = useState<FileWithPreview[]>([]);
  const [dragActive, setDragActive] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const validateFile = (file: File): string | null => {
    if (!acceptedTypes.includes(file.type)) {
      return `File type ${file.type} is not supported. Supported types: ${acceptedTypes.join(', ')}`;
    }

    if (file.size > maxFileSize * 1024 * 1024) {
      return `File size exceeds ${maxFileSize}MB limit`;
    }

    return null;
  };

  const createFilePreview = (file: File): Promise<FileWithPreview> => {
    return new Promise((resolve) => {
      const reader = new FileReader();
      reader.onload = (e) => {
        resolve({
          file,
          preview: e.target?.result as string,
          id: `${file.name}-${Date.now()}-${Math.random()}`,
        });
      };
      reader.readAsDataURL(file);
    });
  };

  const handleFiles = useCallback(async (selectedFiles: FileList | File[]) => {
    const fileArray = Array.from(selectedFiles);
    const validFiles: FileWithPreview[] = [];
    const errors: string[] = [];

    // Check total file count (including existing photos)
    const totalPhotos = files.length + existingPhotos.length + fileArray.length;
    if (totalPhotos > maxFiles) {
      setError(`Maximum ${maxFiles} files allowed. You can only add ${maxFiles - files.length - existingPhotos.length} more files.`);
      return;
    }

    for (const file of fileArray) {
      const validationError = validateFile(file);
      if (validationError) {
        errors.push(`${file.name}: ${validationError}`);
      } else {
        const fileWithPreview = await createFilePreview(file);
        validFiles.push(fileWithPreview);
      }
    }

    if (errors.length > 0) {
      setError(errors.join('\n'));
    }

    if (validFiles.length > 0) {
      setFiles(prev => [...prev, ...validFiles]);
      setError(null);
    }
  }, [files.length, maxFiles, acceptedTypes, maxFileSize]);

  const handleDrag = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === 'dragenter' || e.type === 'dragover') {
      setDragActive(true);
    } else if (e.type === 'dragleave') {
      setDragActive(false);
    }
  }, []);

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);

    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      handleFiles(e.dataTransfer.files);
    }
  }, [handleFiles]);

  const handleFileInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      handleFiles(e.target.files);
    }
  };

  const removeFile = (fileId: string) => {
    setFiles(prev => prev.filter(f => f.id !== fileId));
  };

  const uploadFiles = async () => {
    if (files.length === 0) return;

    setUploading(true);
    setError(null);
    setSuccess(null);

    // Reset progress for all files
    setFiles(prev => prev.map(f => ({ ...f, progress: 0, error: undefined })));

    try {
      // Optimize images before uploading for better mobile performance
      console.log('📸 Optimizing images for mobile upload...');
      const optimizationResults = await optimizeImages(
        files.map(f => f.file),
        MOBILE_OPTIMIZATION_OPTIONS,
        (completed, total) => {
          console.log(`📸 Optimized ${completed}/${total} images`);
        }
      );

      // Update files with optimized versions
      const optimizedFiles = files.map((fileWithPreview, index) => ({
        ...fileWithPreview,
        file: optimizationResults[index].file,
        optimizedSize: optimizationResults[index].optimizedSize,
        compressionRatio: optimizationResults[index].compressionRatio
      }));

      setFiles(optimizedFiles);

      // Log optimization results
      const totalOriginalSize = optimizationResults.reduce((sum, result) => sum + result.originalSize, 0);
      const totalOptimizedSize = optimizationResults.reduce((sum, result) => sum + result.optimizedSize, 0);
      const averageCompression = optimizationResults.reduce((sum, result) => sum + result.compressionRatio, 0) / optimizationResults.length;

      console.log('✅ Image optimization complete:', {
        originalSize: `${(totalOriginalSize / 1024 / 1024).toFixed(2)}MB`,
        optimizedSize: `${(totalOptimizedSize / 1024 / 1024).toFixed(2)}MB`,
        compressionRatio: `${(averageCompression * 100).toFixed(1)}%`,
        spaceSaved: `${((totalOriginalSize - totalOptimizedSize) / 1024 / 1024).toFixed(2)}MB`
      });

      const filesToUpload = optimizedFiles.map(f => f.file);
      const response = await materialsAPI.uploadMaterialPhotos(materialId, filesToUpload);

      if (response.data.success) {
        const uploadedUrls = response.data.data.photoUrls || [];
        setSuccess(`Successfully uploaded ${uploadedUrls.length} optimized photo(s)`);

        // Mark files as uploaded
        setFiles(prev => prev.map(f => ({ ...f, uploaded: true, progress: 100 })));

        onUploadSuccess?.(uploadedUrls);
      } else {
        throw new Error(response.data.message || 'Upload failed');
      }
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || err.message || 'Upload failed';
      setError(errorMessage);
      onUploadError?.(errorMessage);

      // Mark files with error
      setFiles(prev => prev.map(f => ({ ...f, error: errorMessage })));
    } finally {
      setUploading(false);
    }
  };

  const clearAll = () => {
    setFiles([]);
    setError(null);
    setSuccess(null);
  };

  return (
    <Box sx={{ width: '100%' }}>
      {/* Upload Area */}
      <Paper
        sx={{
          p: 3,
          border: '2px dashed',
          borderColor: dragActive ? 'primary.main' : 'grey.300',
          backgroundColor: dragActive ? 'action.hover' : 'background.paper',
          cursor: disabled ? 'not-allowed' : 'pointer',
          transition: 'all 0.2s ease-in-out',
          '&:hover': {
            borderColor: disabled ? 'grey.300' : 'primary.main',
            backgroundColor: disabled ? 'background.paper' : 'action.hover',
          },
        }}
        onDragEnter={handleDrag}
        onDragLeave={handleDrag}
        onDragOver={handleDrag}
        onDrop={handleDrop}
        onClick={() => !disabled && fileInputRef.current?.click()}
      >
        <Box
          sx={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            gap: 2,
            py: 2,
          }}
        >
          <CloudUploadIcon sx={{ fontSize: 48, color: 'text.secondary' }} />
          <Typography variant="h6" color="text.secondary">
            Drag and drop photos here
          </Typography>
          <Typography variant="body2" color="text.secondary">
            or click to browse files
          </Typography>
          <Button
            variant="outlined"
            startIcon={<PhotoCameraIcon />}
            disabled={disabled}
            onClick={(e) => {
              e.stopPropagation();
              fileInputRef.current?.click();
            }}
          >
            Choose Files
          </Button>
          <Typography variant="caption" color="text.secondary">
            Maximum {maxFiles} files, up to {maxFileSize}MB each
          </Typography>
          <Typography variant="caption" color="text.secondary">
            Supported formats: {acceptedTypes.map(type => type.split('/')[1]).join(', ')}
          </Typography>
        </Box>
        <input
          ref={fileInputRef}
          type="file"
          multiple
          accept={acceptedTypes.join(',')}
          onChange={handleFileInputChange}
          style={{ display: 'none' }}
          disabled={disabled}
          data-testid="file-input"
          aria-label="Upload photos"
        />
      </Paper>

      {/* Error/Success Messages */}
      {error && (
        <Alert severity="error" sx={{ mt: 2 }}>
          {error}
        </Alert>
      )}

      {success && (
        <Alert severity="success" sx={{ mt: 2 }}>
          {success}
        </Alert>
      )}

      {/* Existing Photos */}
      {existingPhotos.length > 0 && (
        <Box sx={{ mt: 3 }}>
          <Typography variant="h6" sx={{ mb: 2 }}>
            Existing Photos ({existingPhotos.length})
          </Typography>
          <Grid container spacing={2} data-testid="photos-grid">
            {existingPhotos.map((photo) => (
              <Grid key={photo.id} size={{ xs: 12, sm: 6, md: 4 }}>
                <Card sx={{ position: 'relative' }}>
                  <Box sx={{ height: 140, overflow: 'hidden' }}>
                    <LazyLoadImage
                      src={photo.url}
                      alt={photo.filename}
                      height={140}
                      width="100%"
                      effect="blur"
                      style={{ objectFit: 'cover', height: '140px', width: '100%' }}
                      onError={(e) => {
                        const target = e.target as HTMLImageElement;
                        target.style.display = 'none';
                        const parent = target.parentElement;
                        if (parent) {
                          parent.innerHTML = '<div style="height: 100%; display: flex; align-items: center; justify-content: center; color: grey;"><svg style="font-size: 48px;" viewBox="0 0 24 24"><path d="M21 19V5c0-1.1-.9-2-2-2H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2zM8.5 13.5l2.5 3.01L14.5 12l4.5 6H5l3.5-4.5z"/></svg></div>';
                        }
                      }}
                    />
                  </Box>
                  <CardContent sx={{ pb: 1 }}>
                    <Typography variant="body2" noWrap title={photo.filename}>
                      {photo.filename}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      Uploaded: {new Date(photo.uploadedAt).toLocaleDateString()}
                    </Typography>
                  </CardContent>
                  <CardActions sx={{ pt: 0, px: 2, pb: 2 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, width: '100%' }}>
                      <CheckCircleIcon color="success" />
                      <Typography variant="caption" color="success.main">
                        Uploaded
                      </Typography>
                      <Box sx={{ flexGrow: 1 }} />
                      <IconButton
                        size="small"
                        onClick={() => {
                          if (window.confirm('Are you sure you want to delete this photo?')) {
                            materialsAPI.deleteMaterialPhoto(materialId, photo.id)
                              .then(() => {
                                // Remove from existing photos
                                // This would need to be handled by parent component
                                window.location.reload(); // Temporary solution
                              })
                              .catch((error) => {
                                console.error('Error deleting photo:', error);
                                setError('Failed to delete photo');
                              });
                          }
                        }}
                        color="error"
                        data-testid="delete-photo"
                      >
                        <DeleteIcon />
                      </IconButton>
                    </Box>
                  </CardActions>
                </Card>
              </Grid>
            ))}
          </Grid>
        </Box>
      )}

      {/* File Previews */}
      {files.length > 0 && (
        <Box sx={{ mt: 3 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6">
              Selected Files ({files.length}/{maxFiles})
            </Typography>
            <Button
              variant="outlined"
              size="small"
              onClick={clearAll}
              disabled={uploading}
            >
              Clear All
            </Button>
          </Box>

          <Grid container spacing={2}>
            {files.map((fileWithPreview) => (
              <Grid key={fileWithPreview.id} size={{ xs: 12, sm: 6, md: 4 }}>
                <Card sx={{ position: 'relative' }}>
                  <Box sx={{ height: 140, overflow: 'hidden' }}>
                    <LazyLoadImage
                      src={fileWithPreview.preview}
                      alt={fileWithPreview.file.name}
                      height={140}
                      width="100%"
                      effect="blur"
                      style={{ objectFit: 'cover', height: '140px', width: '100%' }}
                      onError={(e) => {
                        const target = e.target as HTMLImageElement;
                        target.style.display = 'none';
                        const parent = target.parentElement;
                        if (parent) {
                          parent.innerHTML = '<div style="height: 100%; display: flex; align-items: center; justify-content: center; color: grey;"><svg style="font-size: 48px;" viewBox="0 0 24 24"><path d="M21 19V5c0-1.1-.9-2-2-2H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2zM8.5 13.5l2.5 3.01L14.5 12l4.5 6H5l3.5-4.5z"/></svg></div>';
                        }
                      }}
                    />
                  </Box>
                  <CardContent sx={{ pb: 1 }}>
                    <Typography variant="body2" noWrap title={fileWithPreview.file.name}>
                      {fileWithPreview.file.name}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {(fileWithPreview.file.size / 1024 / 1024).toFixed(2)} MB
                    </Typography>
                  </CardContent>
                  <CardActions sx={{ pt: 0, px: 2, pb: 2 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, width: '100%' }}>
                      {fileWithPreview.uploaded ? (
                        <CheckCircleIcon color="success" />
                      ) : fileWithPreview.error ? (
                        <ErrorIcon color="error" />
                      ) : (
                        <FileIcon color="action" />
                      )}
                      <Box sx={{ flexGrow: 1 }}>
                        {fileWithPreview.progress !== undefined && (
                          <LinearProgress
                            variant="determinate"
                            value={fileWithPreview.progress}
                            sx={{ height: 4, borderRadius: 2 }}
                          />
                        )}
                      </Box>
                      <IconButton
                        size="small"
                        onClick={() => removeFile(fileWithPreview.id)}
                        disabled={uploading}
                        color="error"
                        data-testid="delete-file"
                      >
                        <DeleteIcon />
                      </IconButton>
                    </Box>
                  </CardActions>
                  {fileWithPreview.error && (
                    <Box sx={{ px: 2, pb: 1 }}>
                      <Typography variant="caption" color="error">
                        {fileWithPreview.error}
                      </Typography>
                    </Box>
                  )}
                </Card>
              </Grid>
            ))}
          </Grid>

          {/* Upload Button */}
          <Box sx={{ mt: 3, display: 'flex', justifyContent: 'center' }}>
            <Button
              variant="contained"
              size="large"
              onClick={uploadFiles}
              disabled={uploading || files.length === 0}
              startIcon={uploading ? <CloudUploadIcon /> : <PhotoCameraIcon />}
            >
              {uploading ? 'Uploading...' : `Upload ${files.length} Photo${files.length > 1 ? 's' : ''}`}
            </Button>
          </Box>
        </Box>
      )}
    </Box>
  );
};

export default MultiPhotoUpload;