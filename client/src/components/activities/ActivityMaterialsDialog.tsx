import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Alert,
  useTheme,
  useMediaQuery,
  IconButton,
} from '@mui/material';
import {
  Close as CloseIcon,
  Image as ImageIcon,
} from '@mui/icons-material';
import { LazyLoadImage } from 'react-lazy-load-image-component';
import 'react-lazy-load-image-component/src/effects/blur.css';
import { productsAPI } from '../../services/api';

interface ActivityMaterial {
  id: string;
  productVariantId: string;
  quantity: number;
  unitOfMeasureId: string;
  productVariant?: {
    id: string;
    name: string;
    baseProduct?: {
      id: string;
      name: string;
    };
  };
  unitOfMeasure?: {
    id: string;
    name: string;
    symbol: string;
  };
}

interface ProductVariantDetails {
  id: string;
  name: string;
  baseProductName: string;
  baseProductDescription?: string;
  photos: string[];
  quantity: number;
  unitOfMeasureName: string;
  unitSymbol: string;
}

interface ActivityMaterialsDialogProps {
  open: boolean;
  onClose: () => void;
  activityName: string;
  materials: ActivityMaterial[];
}

const ActivityMaterialsDialog: React.FC<ActivityMaterialsDialogProps> = ({
  open,
  onClose,
  activityName,
  materials,
}) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [materialsDetails, setMaterialsDetails] = useState<ProductVariantDetails[]>([]);

  useEffect(() => {
    if (open && materials.length > 0) {
      loadMaterialsDetails();
    }
  }, [open, materials]);

  const loadMaterialsDetails = async () => {
    try {
      setLoading(true);
      setError(null);

      const detailsPromises = materials.map(async (material) => {
        try {
          // Fetch product variant details
          const variantResponse = await productsAPI.getProductVariant(material.productVariantId);
          if (variantResponse.data.success) {
            const variant = variantResponse.data.data;

            // Fetch photos for the variant
            let photos: string[] = [];
            try {
              const photosResponse = await productsAPI.getProductVariantPhotos(material.productVariantId);
              if (photosResponse.data.success) {
                photos = photosResponse.data.data || [];
              }
            } catch (photosError) {
              console.warn('Failed to load photos for variant:', material.productVariantId, photosError);
            }

            return {
              id: material.id,
              name: variant.name,
              baseProductName: variant.baseProductName,
              baseProductDescription: variant.baseProductDescription,
              photos,
              quantity: material.quantity,
              unitOfMeasureName: material.unitOfMeasure?.name || '',
              unitSymbol: material.unitOfMeasure?.symbol || '',
            };
          }
        } catch (variantError) {
          console.error('Failed to load variant details:', material.productVariantId, variantError);
        }

        // Fallback to basic info if detailed fetch fails
        return {
          id: material.id,
          name: material.productVariant?.name || 'Unknown',
          baseProductName: material.productVariant?.baseProduct?.name || 'Unknown',
          baseProductDescription: undefined,
          photos: [],
          quantity: material.quantity,
          unitOfMeasureName: material.unitOfMeasure?.name || '',
          unitSymbol: material.unitOfMeasure?.symbol || '',
        };
      });

      const details = await Promise.all(detailsPromises);
      setMaterialsDetails(details);
    } catch (err: any) {
      console.error('Error loading materials details:', err);
      setError('Failed to load materials details');
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setMaterialsDetails([]);
    setError(null);
    onClose();
  };

  return (
    <Dialog
      data-cy="materials-dialog"
      open={open}
      onClose={handleClose}
      maxWidth="md"
      fullWidth
      fullScreen={isMobile}
      sx={{
        '& .MuiDialog-paper': {
          height: isMobile ? '100%' : 'auto',
          maxHeight: isMobile ? '100%' : '80vh',
        },
      }}
    >
      <DialogTitle sx={{ pb: 1 }}>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Typography variant="h6" component="div">
            Materials for {activityName}
          </Typography>
          <IconButton data-cy="close-materials-dialog" onClick={handleClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent dividers sx={{ p: 2 }}>
        {loading && (
          <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
            <CircularProgress />
          </Box>
        )}

        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        {!loading && !error && materialsDetails.length === 0 && (
          <Box sx={{ py: 4, textAlign: 'center' }}>
            <Typography color="text.secondary">
              No materials required for this activity
            </Typography>
          </Box>
        )}

        {!loading && !error && materialsDetails.length > 0 && (
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, justifyContent: isMobile ? 'center' : 'flex-start' }}>
            {materialsDetails.map((material) => (
              <Card data-cy="material-item" key={material.id} sx={{ width: isMobile ? '100%' : '300px', display: 'flex', flexDirection: 'column' }}>
                {/* Image */}
                <Box sx={{ position: 'relative', height: 140, bgcolor: 'grey.100' }}>
                  {material.photos.length > 0 ? (
                    <LazyLoadImage
                      src={material.photos[0]}
                      alt={material.name}
                      height={140}
                      width="100%"
                      effect="blur"
                      placeholderSrc=""
                      style={{ objectFit: 'cover', height: '140px', width: '100%' }}
                      onError={(e: React.SyntheticEvent<HTMLImageElement>) => {
                        // Fallback to placeholder on error
                        const target = e.target as HTMLImageElement;
                        target.style.display = 'none';
                        const parent = target.parentElement;
                        if (parent) {
                          parent.innerHTML = '<div style="height: 100%; display: flex; align-items: center; justify-content: center; color: grey;"><svg style="font-size: 48px;" viewBox="0 0 24 24"><path d="M21 19V5c0-1.1-.9-2-2-2H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2zM8.5 13.5l2.5 3.01L14.5 12l4.5 6H5l3.5-4.5z"/></svg></div>';
                        }
                      }}
                    />
                  ) : (
                    <Box
                      sx={{
                        height: '100%',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        color: 'grey.400',
                      }}
                    >
                      <ImageIcon sx={{ fontSize: 48 }} />
                    </Box>
                  )}
                </Box>

                <CardContent sx={{ flexGrow: 1, p: 2 }}>
                  {/* Name */}
                  <Typography variant="h6" component="div" gutterBottom sx={{ fontSize: '1rem' }}>
                    {material.baseProductName}
                  </Typography>

                  {/* Variant Name */}
                  {material.name !== material.baseProductName && (
                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                      {material.name}
                    </Typography>
                  )}

                  {/* Description */}
                  {material.baseProductDescription && (
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 2, display: '-webkit-box', WebkitLineClamp: 3, WebkitBoxOrient: 'vertical', overflow: 'hidden' }}>
                      {material.baseProductDescription}
                    </Typography>
                  )}

                  {/* Quantity */}
                  <Box sx={{ mt: 'auto', pt: 1 }}>
                    <Chip
                      label={`${material.quantity} ${material.unitSymbol || material.unitOfMeasureName}`}
                      size="small"
                      color="primary"
                      variant="outlined"
                    />
                  </Box>
                </CardContent>
              </Card>
            ))}
          </Box>
        )}
      </DialogContent>

      <DialogActions sx={{ p: 2 }}>
        <Button onClick={handleClose} variant="outlined" fullWidth={isMobile} sx={{ minHeight: 44 }}>
          Close
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default ActivityMaterialsDialog;