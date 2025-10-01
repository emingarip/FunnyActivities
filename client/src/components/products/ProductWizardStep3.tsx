import React from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Chip,
  Card,
  CardContent,
  Divider,
  Alert,
  List,
  ListItem,
  ListItemText,
  Avatar,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import {
  Photo as PhotoIcon,
  Inventory as InventoryIcon,
  Category as CategoryIcon,
  Palette as PaletteIcon,
  Straighten as SizeIcon,
} from '@mui/icons-material';
import { ProductWizardFormData, ProductCategoryDto, UnitOfMeasureDto } from '../../services/api.types';

interface ProductWizardStep3Props {
  formData: ProductWizardFormData;
  categories: ProductCategoryDto[];
  unitsOfMeasure: UnitOfMeasureDto[];
}

/**
 * Step 3: Review & Submit
 * Shows a comprehensive summary of all entered data before final submission
 */
const ProductWizardStep3: React.FC<ProductWizardStep3Props> = ({
  formData,
  categories,
  unitsOfMeasure
}) => {
  // Responsive breakpoints
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const isTablet = useMediaQuery(theme.breakpoints.between('sm', 'md'));
  const getCategoryName = (categoryId: string) => {
    // Add type checking to prevent errors
    if (!Array.isArray(categories)) {
      console.error('❌ categories is not an array:', categories, 'Type:', typeof categories);
      return 'Unknown Category';
    }

    const category = categories.find(c => c.id === categoryId);
    return category?.name || 'Unknown Category';
  };

  const getUnitName = (unitId: string) => {
    // Add type checking to prevent errors
    if (!Array.isArray(unitsOfMeasure)) {
      console.error('❌ unitsOfMeasure is not an array:', unitsOfMeasure, 'Type:', typeof unitsOfMeasure);
      return 'Unknown Unit';
    }

    const unit = unitsOfMeasure.find(u => u.id === unitId);
    return unit ? `${unit.name} (${unit.symbol})` : 'Unknown Unit';
  };

  const getDynamicPropertiesList = (properties: Record<string, any>) => {
    return Object.entries(properties).map(([key, value]) => ({
      key,
      value: typeof value === 'boolean' ? (value ? 'Yes' : 'No') : String(value)
    }));
  };

  const totalStock = formData.variants.reduce((sum, variant) => sum + variant.stockQuantity, 0);
  const totalPhotos = formData.baseProduct.photos.length +
    formData.variants.reduce((sum, variant) => sum + variant.photos.length, 0);

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Review Your Product
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Please review all the information below before submitting your product. You can go back to previous steps to make changes.
      </Typography>

      {/* Summary Cards */}
      <Box sx={{
        display: 'flex',
        gap: { xs: 1.5, sm: 2, md: 2 },
        mb: { xs: 2, sm: 3, md: 3 },
        flexWrap: 'wrap',
        flexDirection: { xs: 'column', sm: 'row' }
      }}>
        <Paper sx={{
          p: { xs: 1.5, sm: 2, md: 2 },
          flex: 1,
          minWidth: { xs: '100%', sm: '200px', md: '200px' }
        }}>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
            <InventoryIcon color="primary" sx={{ mr: 1, fontSize: { xs: '1.25rem', sm: '1.5rem', md: '1.5rem' } }} />
            <Typography variant={isMobile ? "h6" : "h6"} sx={{
              fontSize: { xs: '1.1rem', sm: '1.25rem', md: '1.5rem' }
            }}>
              {formData.variants.length}
            </Typography>
          </Box>
          <Typography variant="body2" color="text.secondary" sx={{
            fontSize: { xs: '0.8rem', sm: '0.875rem', md: '0.875rem' }
          }}>
            Product Variants
          </Typography>
        </Paper>

        <Paper sx={{
          p: { xs: 1.5, sm: 2, md: 2 },
          flex: 1,
          minWidth: { xs: '100%', sm: '200px', md: '200px' }
        }}>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
            <PhotoIcon color="primary" sx={{ mr: 1, fontSize: { xs: '1.25rem', sm: '1.5rem', md: '1.5rem' } }} />
            <Typography variant={isMobile ? "h6" : "h6"} sx={{
              fontSize: { xs: '1.1rem', sm: '1.25rem', md: '1.5rem' }
            }}>
              {totalPhotos}
            </Typography>
          </Box>
          <Typography variant="body2" color="text.secondary" sx={{
            fontSize: { xs: '0.8rem', sm: '0.875rem', md: '0.875rem' }
          }}>
            Total Photos
          </Typography>
        </Paper>

        <Paper sx={{
          p: { xs: 1.5, sm: 2, md: 2 },
          flex: 1,
          minWidth: { xs: '100%', sm: '200px', md: '200px' }
        }}>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
            <CategoryIcon color="primary" sx={{ mr: 1, fontSize: { xs: '1.25rem', sm: '1.5rem', md: '1.5rem' } }} />
            <Typography variant={isMobile ? "h6" : "h6"} sx={{
              fontSize: { xs: '1.1rem', sm: '1.25rem', md: '1.5rem' }
            }}>
              {totalStock}
            </Typography>
          </Box>
          <Typography variant="body2" color="text.secondary" sx={{
            fontSize: { xs: '0.8rem', sm: '0.875rem', md: '0.875rem' }
          }}>
            Total Stock Quantity
          </Typography>
        </Paper>
      </Box>

      {/* Base Product Details */}
      <Paper sx={{
        p: { xs: 2, sm: 3, md: 3 },
        mb: { xs: 2, sm: 3, md: 3 },
        borderRadius: { xs: 1, sm: 1, md: 1 }
      }}>
        <Typography variant={isMobile ? "h6" : "h6"} gutterBottom sx={{
          display: 'flex',
          alignItems: 'center',
          fontSize: { xs: '1.1rem', sm: '1.25rem', md: '1.25rem' }
        }}>
          <CategoryIcon sx={{ mr: 1, fontSize: { xs: '1.25rem', sm: '1.5rem', md: '1.5rem' } }} />
          Base Product Information
        </Typography>
        <Divider sx={{ mb: 2 }} />

        <Box sx={{
          display: 'flex',
          flexDirection: 'column',
          gap: { xs: 1.5, sm: 2, md: 2 }
        }}>
          <Box>
            <Typography variant="subtitle2" color="text.secondary">Product Name</Typography>
            <Typography variant="body1">{formData.baseProduct.name || 'Not specified'}</Typography>
          </Box>

          <Box>
            <Typography variant="subtitle2" color="text.secondary">Category</Typography>
            <Typography variant="body1">
              {formData.baseProduct.categoryId ? getCategoryName(formData.baseProduct.categoryId) : 'Not specified'}
            </Typography>
          </Box>

          <Box>
            <Typography variant="subtitle2" color="text.secondary">Description</Typography>
            <Typography variant="body1">
              {formData.baseProduct.description || 'No description provided'}
            </Typography>
          </Box>

          {formData.baseProduct.photos.length > 0 && (
            <Box>
              <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>
                Product Photos ({formData.baseProduct.photos.length})
              </Typography>
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                {formData.baseProduct.photos.map((photo, index) => (
                  <Box
                    key={index}
                    sx={{
                      width: 80,
                      height: 80,
                      border: '1px solid #ddd',
                      borderRadius: 1,
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      bgcolor: 'grey.100'
                    }}
                  >
                    <PhotoIcon color="action" />
                  </Box>
                ))}
              </Box>
            </Box>
          )}

          {Object.keys(formData.baseProduct.dynamicProperties).length > 0 && (
            <Box>
              <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>
                Additional Properties
              </Typography>
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                {getDynamicPropertiesList(formData.baseProduct.dynamicProperties).map((prop, index) => (
                  <Chip
                    key={index}
                    label={`${prop.key}: ${prop.value}`}
                    size="small"
                    variant="outlined"
                  />
                ))}
              </Box>
            </Box>
          )}
        </Box>
      </Paper>

      {/* Product Variants */}
      <Paper sx={{
        p: { xs: 2, sm: 3, md: 3 },
        mb: { xs: 2, sm: 3, md: 3 },
        borderRadius: { xs: 1, sm: 1, md: 1 }
      }}>
        <Typography variant={isMobile ? "h6" : "h6"} gutterBottom sx={{
          display: 'flex',
          alignItems: 'center',
          fontSize: { xs: '1.1rem', sm: '1.25rem', md: '1.25rem' }
        }}>
          <InventoryIcon sx={{ mr: 1, fontSize: { xs: '1.25rem', sm: '1.5rem', md: '1.5rem' } }} />
          Product Variants ({formData.variants.length})
        </Typography>
        <Divider sx={{ mb: 2 }} />

        <Box sx={{
          display: 'flex',
          flexDirection: 'column',
          gap: { xs: 1.5, sm: 2, md: 2 }
        }}>
          {formData.variants.map((variant, index) => (
            <Card key={index} variant="outlined">
              <CardContent>
                <Typography variant="h6" sx={{ mb: 2 }}>
                  Variant {index + 1}: {variant.name || 'Unnamed'}
                </Typography>

                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                  {/* Basic Info */}
                  <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                    {variant.size && (
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <SizeIcon fontSize="small" color="action" />
                        <Typography variant="body2">Size: {variant.size}</Typography>
                      </Box>
                    )}
                    {variant.color && (
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <PaletteIcon fontSize="small" color="action" />
                        <Typography variant="body2">Color: {variant.color}</Typography>
                      </Box>
                    )}
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <InventoryIcon fontSize="small" color="action" />
                      <Typography variant="body2">Stock: {variant.stockQuantity}</Typography>
                    </Box>
                    {variant.unitOfMeasureId && (
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography variant="body2">Unit: {getUnitName(variant.unitOfMeasureId)}</Typography>
                      </Box>
                    )}
                  </Box>

                  {/* Usage Notes */}
                  {variant.usageNotes && (
                    <Box>
                      <Typography variant="body2" color="text.secondary">
                        Notes: {variant.usageNotes}
                      </Typography>
                    </Box>
                  )}

                  {/* Photos */}
                  {variant.photos.length > 0 && (
                    <Box>
                      <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                        Photos: {variant.photos.length} uploaded
                      </Typography>
                      <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                        {variant.photos.map((_, photoIndex) => (
                          <Box
                            key={photoIndex}
                            sx={{
                              width: 60,
                              height: 60,
                              border: '1px solid #ddd',
                              borderRadius: 1,
                              display: 'flex',
                              alignItems: 'center',
                              justifyContent: 'center',
                              bgcolor: 'grey.100'
                            }}
                          >
                            <PhotoIcon fontSize="small" color="action" />
                          </Box>
                        ))}
                      </Box>
                    </Box>
                  )}

                  {/* Dynamic Properties */}
                  {Object.keys(variant.dynamicProperties).length > 0 && (
                    <Box>
                      <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                        Additional Properties:
                      </Typography>
                      <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                        {getDynamicPropertiesList(variant.dynamicProperties).map((prop, propIndex) => (
                          <Chip
                            key={propIndex}
                            label={`${prop.key}: ${prop.value}`}
                            size="small"
                            variant="outlined"
                          />
                        ))}
                      </Box>
                    </Box>
                  )}
                </Box>
              </CardContent>
            </Card>
          ))}
        </Box>
      </Paper>

      {/* Final Checklist */}
      <Alert severity="info" sx={{
        mb: { xs: 2, sm: 3, md: 3 },
        '& .MuiAlert-message': {
          fontSize: { xs: '0.875rem', sm: '1rem', md: '1rem' }
        }
      }}>
        <Typography variant="subtitle2" gutterBottom sx={{
          fontSize: { xs: '0.875rem', sm: '1rem', md: '1rem' },
          fontWeight: 600
        }}>
          Before submitting, please ensure:
        </Typography>
        <List dense sx={{
          '& .MuiListItem-root': {
            px: { xs: 0, sm: 1, md: 1 },
            py: { xs: 0.5, sm: 0.5, md: 0.5 }
          },
          '& .MuiListItemText-primary': {
            fontSize: { xs: '0.8rem', sm: '0.875rem', md: '0.875rem' }
          }
        }}>
          <ListItem>
            <ListItemText primary="✓ Product name and description are complete" />
          </ListItem>
          <ListItem>
            <ListItemText primary="✓ Category is selected" />
          </ListItem>
          <ListItem>
            <ListItemText primary="✓ All variants have names and stock quantities" />
          </ListItem>
          <ListItem>
            <ListItemText primary="✓ Units of measure are specified for all variants" />
          </ListItem>
          <ListItem>
            <ListItemText primary="✓ Photos are uploaded (optional but recommended)" />
          </ListItem>
        </List>
      </Alert>

      {/* Submission Notice */}
      <Alert severity="warning" sx={{
        '& .MuiAlert-message': {
          fontSize: { xs: '0.875rem', sm: '1rem', md: '1rem' }
        }
      }}>
        <Typography variant="body2" sx={{
          fontSize: { xs: '0.8rem', sm: '0.875rem', md: '0.875rem' }
        }}>
          <strong>Important:</strong> Once submitted, you can still edit the product and its variants,
          but some changes may affect existing orders or inventory tracking.
        </Typography>
      </Alert>
    </Box>
  );
};

export default ProductWizardStep3;