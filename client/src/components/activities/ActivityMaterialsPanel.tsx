import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Alert,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import {
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

interface ActivityMaterialsPanelProps {
  activityName: string;
  materials: ActivityMaterial[];
}

const ActivityMaterialsPanel: React.FC<ActivityMaterialsPanelProps> = ({
  activityName,
  materials,
}) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [materialsDetails, setMaterialsDetails] = useState<ProductVariantDetails[]>([]);

  useEffect(() => {
    if (materials.length > 0) {
      loadMaterialsDetails();
    }
  }, [materials]);

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

  if (materials.length === 0) {
    return (
      <Box sx={{ py: 4, textAlign: 'center' }}>
        <Typography color="text.secondary">
          No materials required for this activity
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h6" gutterBottom sx={{ mb: 2 }}>
        Materials for {activityName}
      </Typography>

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

      {!loading && !error && materialsDetails.length > 0 && (
        <Box sx={{
          display: 'flex',
          flexWrap: 'wrap',
          gap: 2,
          justifyContent: isMobile ? 'center' : 'flex-start'
        }}>
          {materialsDetails.map((material) => (
            <Card
              data-cy="material-item"
              key={material.id}
              sx={{
                width: isMobile ? '100%' : '280px',
                display: 'flex',
                flexDirection: 'column',
                maxWidth: isMobile ? 'none' : '280px'
              }}
            >
              {/* Image */}
              <Box sx={{ position: 'relative', height: 120, bgcolor: 'grey.100' }}>
                {material.photos.length > 0 ? (
                  <LazyLoadImage
                    src={material.photos[0]}
                    alt={material.name}
                    height={120}
                    width="100%"
                    effect="blur"
                    placeholderSrc=""
                    style={{ objectFit: 'cover', height: '120px', width: '100%' }}
                    onError={(e: React.SyntheticEvent<HTMLImageElement>) => {
                      // Fallback to placeholder on error
                      const target = e.target as HTMLImageElement;
                      target.style.display = 'none';
                      const parent = target.parentElement;
                      if (parent) {
                        parent.innerHTML = '<div style="height: 100%; display: flex; align-items: center; justify-content: center; color: grey;"><svg style="font-size: 36px;" viewBox="0 0 24 24"><path d="M21 19V5c0-1.1-.9-2-2-2H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2zM8.5 13.5l2.5 3.01L14.5 12l4.5 6H5l3.5-4.5z"/></svg></div>';
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
                    <ImageIcon sx={{ fontSize: 36 }} />
                  </Box>
                )}
              </Box>

              <CardContent sx={{ flexGrow: 1, p: 1.5 }}>
                {/* Name */}
                <Typography variant="subtitle1" component="div" gutterBottom sx={{ fontSize: '0.9rem', fontWeight: 600 }}>
                  {material.baseProductName}
                </Typography>

                {/* Variant Name */}
                {material.name !== material.baseProductName && (
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    {material.name}
                  </Typography>
                )}

                {/* Description */}
                {material.baseProductDescription && (
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1, display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical', overflow: 'hidden', fontSize: '0.8rem' }}>
                    {material.baseProductDescription}
                  </Typography>
                )}

                {/* Quantity */}
                <Box sx={{ mt: 'auto', pt: 0.5 }}>
                  <Chip
                    label={`${material.quantity} ${material.unitSymbol || material.unitOfMeasureName}`}
                    size="small"
                    color="primary"
                    variant="outlined"
                    sx={{ fontSize: '0.75rem' }}
                  />
                </Box>
              </CardContent>
            </Card>
          ))}
        </Box>
      )}
    </Box>
  );
};

export default ActivityMaterialsPanel;