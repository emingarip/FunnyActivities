import React, { useState, useEffect } from 'react';
import {
  Box,
  Grid,
  Card,
  CardActions,
  IconButton,
  Modal,
  Fade,
  Backdrop,
  Typography,
  CircularProgress,
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import {
  Close as CloseIcon,
  Delete as DeleteIcon,
  NavigateBefore as NavigateBeforeIcon,
  NavigateNext as NavigateNextIcon,
  ZoomIn as ZoomInIcon,
  DragIndicator as DragIndicatorIcon,
} from '@mui/icons-material';
import { LazyLoadImage } from 'react-lazy-load-image-component';
import 'react-lazy-load-image-component/src/effects/blur.css';
import { materialsAPI } from '../../services/api';

interface Photo {
  id: string;
  url: string;
  filename: string;
  uploadedAt: string;
  size?: number;
}

interface MaterialPhotoDisplayProps {
  materialId: string;
  photos?: Photo[];
  onPhotosChange?: (photos: Photo[]) => void;
  maxPhotos?: number;
  disabled?: boolean;
}

const MaterialPhotoDisplay: React.FC<MaterialPhotoDisplayProps> = ({
  materialId,
  photos: initialPhotos,
  onPhotosChange,
  maxPhotos = 20,
  disabled = false,
}) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const [photos, setPhotos] = useState<Photo[]>(initialPhotos || []);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedPhotoIndex, setSelectedPhotoIndex] = useState<number | null>(null);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [photoToDelete, setPhotoToDelete] = useState<string | null>(null);
  const [draggedIndex, setDraggedIndex] = useState<number | null>(null);

  // Fetch photos if not provided
  useEffect(() => {
    if (!initialPhotos) {
      fetchPhotos();
    }
  }, [materialId, initialPhotos]);

  const fetchPhotos = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await materialsAPI.getMaterial(materialId);
      if (response.data.success) {
        const materialPhotos = response.data.data.photos || [];
        setPhotos(materialPhotos);
        onPhotosChange?.(materialPhotos);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to fetch photos');
    } finally {
      setLoading(false);
    }
  };


  const handlePhotoClick = (index: number) => {
    setSelectedPhotoIndex(index);
  };

  const handleCloseModal = () => {
    setSelectedPhotoIndex(null);
  };

  const handlePrevPhoto = () => {
    if (selectedPhotoIndex !== null && selectedPhotoIndex > 0) {
      setSelectedPhotoIndex(selectedPhotoIndex - 1);
    }
  };

  const handleNextPhoto = () => {
    if (selectedPhotoIndex !== null && selectedPhotoIndex < photos.length - 1) {
      setSelectedPhotoIndex(selectedPhotoIndex + 1);
    }
  };

  const handleDeleteClick = (photoId: string) => {
    setPhotoToDelete(photoId);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!photoToDelete) return;

    try {
      // Assume delete endpoint exists
      await materialsAPI.deleteMaterialPhoto(materialId, photoToDelete);
      const updatedPhotos = photos.filter(p => p.id !== photoToDelete);
      setPhotos(updatedPhotos);
      onPhotosChange?.(updatedPhotos);
      setDeleteDialogOpen(false);
      setPhotoToDelete(null);
    } catch (err: any) {
      setError(err.message || 'Failed to delete photo');
    }
  };

  const handleDragStart = (e: React.DragEvent, index: number) => {
    setDraggedIndex(index);
    e.dataTransfer.effectAllowed = 'move';
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
  };

  const handleDrop = (e: React.DragEvent, dropIndex: number) => {
    e.preventDefault();
    if (draggedIndex === null || draggedIndex === dropIndex) return;

    const newPhotos = [...photos];
    const [draggedPhoto] = newPhotos.splice(draggedIndex, 1);
    newPhotos.splice(dropIndex, 0, draggedPhoto);

    setPhotos(newPhotos);
    onPhotosChange?.(newPhotos);
    setDraggedIndex(null);
  };

  const handleDragEnd = () => {
    setDraggedIndex(null);
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight={200}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mb: 2 }}>
        {error}
        <Button size="small" onClick={fetchPhotos} sx={{ ml: 2 }}>
          Retry
        </Button>
      </Alert>
    );
  }

  return (
    <Box>
      {photos.length === 0 ? (
        <Typography variant="body2" color="text.secondary" align="center" sx={{ py: 4 }}>
          No photos available for this material.
        </Typography>
      ) : (
        <Grid container spacing={2}>
          {photos.map((photo, index) => (
            <Grid key={photo.id} size={{ xs: 6, sm: 4, md: 3 }}>
              <Card
                sx={{
                  position: 'relative',
                  cursor: 'pointer',
                  '&:hover': {
                    boxShadow: theme.shadows[4],
                  },
                }}
                draggable={!disabled}
                onDragStart={(e) => handleDragStart(e, index)}
                onDragOver={handleDragOver}
                onDrop={(e) => handleDrop(e, index)}
                onDragEnd={handleDragEnd}
              >
                <Box sx={{ position: 'relative' }}>
                  <Box sx={{ height: 140, overflow: 'hidden' }}>
                    <LazyLoadImage
                      src={photo.url}
                      alt={photo.filename}
                      height={140}
                      width="100%"
                      effect="blur"
                      style={{ objectFit: 'cover', height: '140px', width: '100%' }}
                      onClick={() => handlePhotoClick(index)}
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
                  {!disabled && (
                    <Box
                      sx={{
                        position: 'absolute',
                        top: 8,
                        right: 8,
                        display: 'flex',
                        gap: 1,
                      }}
                    >
                      <IconButton
                        size="small"
                        onClick={(e) => {
                          e.stopPropagation();
                          handleDeleteClick(photo.id);
                        }}
                        sx={{
                          bgcolor: 'rgba(0,0,0,0.5)',
                          color: 'white',
                          '&:hover': { bgcolor: 'rgba(0,0,0,0.7)' },
                        }}
                      >
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </Box>
                  )}
                  {!disabled && draggedIndex === null && (
                    <Box
                      sx={{
                        position: 'absolute',
                        top: 8,
                        left: 8,
                        bgcolor: 'rgba(0,0,0,0.5)',
                        borderRadius: 1,
                        p: 0.5,
                      }}
                    >
                      <DragIndicatorIcon sx={{ color: 'white', fontSize: 16 }} />
                    </Box>
                  )}
                </Box>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}

      {/* Full-size Modal */}
      <Modal
        open={selectedPhotoIndex !== null}
        onClose={handleCloseModal}
        closeAfterTransition
        BackdropComponent={Backdrop}
        BackdropProps={{ timeout: 500 }}
      >
        <Fade in={selectedPhotoIndex !== null}>
          <Box
            sx={{
              position: 'absolute',
              top: '50%',
              left: '50%',
              transform: 'translate(-50%, -50%)',
              maxWidth: '90vw',
              maxHeight: '90vh',
              bgcolor: 'background.paper',
              boxShadow: 24,
              outline: 'none',
            }}
          >
            {selectedPhotoIndex !== null && (
              <Box sx={{ position: 'relative' }}>
                <IconButton
                  onClick={handleCloseModal}
                  sx={{
                    position: 'absolute',
                    top: 16,
                    right: 16,
                    zIndex: 1,
                    bgcolor: 'rgba(0,0,0,0.5)',
                    color: 'white',
                    '&:hover': { bgcolor: 'rgba(0,0,0,0.7)' },
                  }}
                >
                  <CloseIcon />
                </IconButton>

                {!isMobile && selectedPhotoIndex > 0 && (
                  <IconButton
                    onClick={handlePrevPhoto}
                    sx={{
                      position: 'absolute',
                      left: 16,
                      top: '50%',
                      transform: 'translateY(-50%)',
                      zIndex: 1,
                      bgcolor: 'rgba(0,0,0,0.5)',
                      color: 'white',
                      '&:hover': { bgcolor: 'rgba(0,0,0,0.7)' },
                    }}
                  >
                    <NavigateBeforeIcon />
                  </IconButton>
                )}

                {!isMobile && selectedPhotoIndex < photos.length - 1 && (
                  <IconButton
                    onClick={handleNextPhoto}
                    sx={{
                      position: 'absolute',
                      right: 16,
                      top: '50%',
                      transform: 'translateY(-50%)',
                      zIndex: 1,
                      bgcolor: 'rgba(0,0,0,0.5)',
                      color: 'white',
                      '&:hover': { bgcolor: 'rgba(0,0,0,0.7)' },
                    }}
                  >
                    <NavigateNextIcon />
                  </IconButton>
                )}

                <img
                  src={photos[selectedPhotoIndex].url}
                  alt={photos[selectedPhotoIndex].filename}
                  style={{
                    maxWidth: '100%',
                    maxHeight: '80vh',
                    objectFit: 'contain',
                    display: 'block',
                  }}
                />

                <Box sx={{ p: 2, bgcolor: 'background.paper' }}>
                  <Typography variant="body2" color="text.secondary">
                    {photos[selectedPhotoIndex].filename}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {new Date(photos[selectedPhotoIndex].uploadedAt).toLocaleDateString()}
                  </Typography>
                </Box>
              </Box>
            )}
          </Box>
        </Fade>
      </Modal>

      {/* Delete Confirmation Dialog */}
      <Dialog
        open={deleteDialogOpen}
        onClose={() => setDeleteDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Delete Photo</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete this photo? This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleDeleteConfirm} color="error" variant="contained">
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default MaterialPhotoDisplay;