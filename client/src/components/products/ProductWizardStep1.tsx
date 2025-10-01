import React, { useState, useCallback } from 'react';
import {
  Box,
  TextField,
  Typography,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Paper,
  Grid,
  Alert,
  Modal,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  CircularProgress,
  IconButton,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import { Close as CloseIcon } from '@mui/icons-material';
import { ProductWizardFormData, ProductCategoryDto, FormValidationErrors } from '../../services/api.types';
import { productsAPI } from '../../services/api';
import DynamicPropertiesInput from '../materials/DynamicPropertiesInput';
import MultiPhotoUpload from '../materials/MultiPhotoUpload';

interface ProductWizardStep1Props {
  formData: ProductWizardFormData;
  onChange: (updates: Partial<ProductWizardFormData>) => void;
  categories: ProductCategoryDto[];
  errors: FormValidationErrors;
  onCategoryCreated?: () => void; // Callback to refresh categories after creation
}

/**
 * Step 1: Base Product Details Form
 * Handles name, description, category, photos, and dynamic properties
 */
const ProductWizardStep1: React.FC<ProductWizardStep1Props> = ({
  formData,
  onChange,
  categories,
  errors,
  onCategoryCreated
}) => {
  // Responsive breakpoints
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const isTablet = useMediaQuery(theme.breakpoints.between('sm', 'md'));

  console.log('🔍 ProductWizardStep1 rendering with categories:', categories);
  console.log('🔍 Categories type:', typeof categories);
  console.log('🔍 Categories isArray:', Array.isArray(categories));
  const categoriesArray = (categories as any) || [];
  console.log('🔍 Categories.items isArray:', Array.isArray(categoriesArray));
  console.log('🔍 Categories length:', categoriesArray.length);
  console.log('🔍 Current categoryId:', formData.baseProduct.categoryId);
  if (Array.isArray(categoriesArray)) {
    categoriesArray.forEach((cat: any, index: number) => {
      console.log(`🔍 Category ${index}:`, { id: cat.id, name: cat.name });
    });
  }

  const [photoUploadKey, setPhotoUploadKey] = useState(0);

  // Modal state for adding new category
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [newCategoryName, setNewCategoryName] = useState('');
  const [isCreatingCategory, setIsCreatingCategory] = useState(false);
  const [categoryError, setCategoryError] = useState<string | null>(null);

  const handleBaseProductChange = useCallback((field: string, value: any) => {
    onChange({
      baseProduct: {
        ...formData.baseProduct,
        [field]: value
      }
    });
  }, [formData.baseProduct, onChange]);

  const handlePhotosChange = useCallback((photos: File[]) => {
    handleBaseProductChange('photos', photos);
  }, [handleBaseProductChange]);

  const handleDynamicPropertiesChange = useCallback((properties: Record<string, any>) => {
    handleBaseProductChange('dynamicProperties', properties);
  }, [handleBaseProductChange]);

  const handlePhotoUploadSuccess = useCallback((uploadedUrls: string[]) => {
    // For now, we'll just log success. In a real implementation,
    // you might want to update the form data with the uploaded URLs
    console.log('Photos uploaded successfully:', uploadedUrls);
  }, []);

  const handlePhotoUploadError = useCallback((error: string) => {
    console.error('Photo upload error:', error);
  }, []);

  // Modal handlers
  const handleOpenModal = useCallback(() => {
    console.log('🔍 ProductWizardStep1: Opening nested category modal');
    console.log('🔍 Parent modal root aria-hidden:', document.querySelector('[role="presentation"].MuiModal-root')?.getAttribute('aria-hidden'));
    setIsModalOpen(true);
    setNewCategoryName('');
    setCategoryError(null);
  }, []);

  const handleCloseModal = useCallback(() => {
    setIsModalOpen(false);
    setNewCategoryName('');
    setCategoryError(null);
  }, []);

  // Category creation handler
  const handleCreateCategory = useCallback(async () => {
    if (!newCategoryName.trim()) {
      setCategoryError('Category name is required');
      return;
    }

    setIsCreatingCategory(true);
    setCategoryError(null);

    try {
      const response = await productsAPI.createCategory({
        name: newCategoryName.trim(),
      });

      if (response.data?.success) {
        // Refresh categories by calling the parent callback
        if (onCategoryCreated) {
          onCategoryCreated();
        }
        handleCloseModal();
      } else {
        setCategoryError(response.data?.message || 'Failed to create category');
      }
    } catch (error: any) {
      console.error('Error creating category:', error);
      setCategoryError(error.response?.data?.message || error.message || 'Failed to create category');
    } finally {
      setIsCreatingCategory(false);
    }
  }, [newCategoryName, handleCloseModal, onCategoryCreated]);

  const getFieldError = (field: string) => {
    if (errors.baseProduct && typeof errors.baseProduct === 'object' && !Array.isArray(errors.baseProduct)) {
      return (errors.baseProduct as any)[field] as string;
    }
    return undefined;
  };

  // Additional debug log for rendering
  if (Array.isArray(categoriesArray)) {
    console.log('🔍 About to render Select with', categoriesArray.length, 'categories');
  }

  return (
    <Box>
      <Typography
        variant={isMobile ? "h6" : "h6"}
        component="h2"
        gutterBottom
        sx={{
          fontSize: {
            xs: '1.1rem',
            sm: '1.25rem',
            md: '1.25rem'
          }
        }}
      >
        Base Product Information
      </Typography>
      <Typography
        variant={isMobile ? "body2" : "body2"}
        color="text.secondary"
        component="p"
        sx={{
          mb: { xs: 2, sm: 2.5, md: 3 },
          fontSize: {
            xs: '0.875rem',
            sm: '0.875rem',
            md: '0.875rem'
          }
        }}
      >
        Enter the basic details for your product. This information will be shared across all variants.
      </Typography>

      <Box sx={{
        display: 'flex',
        flexDirection: 'column',
        gap: { xs: 2, sm: 2.5, md: 3 }
      }}>
        {/* Product Name and Category Row */}
        <Box sx={{
          display: 'flex',
          gap: { xs: 1.5, sm: 2, md: 2 },
          flexDirection: { xs: 'column', sm: 'row' },
          flexWrap: 'wrap'
        }}>
          <Box sx={{
            flex: 1,
            minWidth: { xs: '100%', sm: '250px', md: '300px' }
          }}>
            <TextField
              fullWidth
              label="Product Name"
              value={formData.baseProduct.name}
              onChange={(e) => handleBaseProductChange('name', e.target.value)}
              error={!!getFieldError('name')}
              helperText={getFieldError('name')}
              required
              placeholder="e.g., Wireless Headphones"
              size={isMobile ? "medium" : "medium"}
              inputProps={{
                'aria-describedby': getFieldError('name') ? 'product-name-error' : 'product-name-help',
                'aria-required': 'true',
                'aria-invalid': !!getFieldError('name')
              }}
              FormHelperTextProps={{
                id: getFieldError('name') ? 'product-name-error' : 'product-name-help'
              }}
              sx={{
                '& .MuiInputBase-root': {
                  minHeight: {
                    xs: '48px',
                    sm: '44px',
                    md: '44px'
                  }
                },
                '& .MuiInputBase-input': {
                  fontSize: {
                    xs: '1rem',
                    sm: '1rem',
                    md: '1rem'
                  },
                  padding: {
                    xs: '14px 16px',
                    sm: '12px 16px',
                    md: '12px 16px'
                  }
                },
                '& .MuiInputLabel-root': {
                  fontSize: {
                    xs: '1rem',
                    sm: '1rem',
                    md: '1rem'
                  },
                  transform: {
                    xs: 'translate(16px, 14px) scale(1)',
                    sm: 'translate(16px, 12px) scale(1)',
                    md: 'translate(16px, 12px) scale(1)'
                  },
                  '&.MuiInputLabel-shrink': {
                    transform: {
                      xs: 'translate(16px, -6px) scale(0.85)',
                      sm: 'translate(16px, -6px) scale(0.85)',
                      md: 'translate(16px, -6px) scale(0.85)'
                    }
                  }
                }
              }}
            />
          </Box>

          <Box sx={{
            flex: 1,
            minWidth: { xs: '100%', sm: '250px', md: '300px' }
          }}>
            <FormControl fullWidth size={isMobile ? "medium" : "medium"}>
              <InputLabel>Category</InputLabel>
              <Select
                value={formData.baseProduct.categoryId}
                label="Category"
                onChange={(e) => {
                  if (e.target.value === 'add-new-category') {
                    handleOpenModal();
                  } else {
                    handleBaseProductChange('categoryId', e.target.value);
                  }
                }}
                sx={{
                  '& .MuiInputBase-root': {
                    minHeight: {
                      xs: '48px',
                      sm: '44px',
                      md: '44px'
                    }
                  },
                  '& .MuiSelect-select': {
                    fontSize: {
                      xs: '1rem',
                      sm: '1rem',
                      md: '1rem'
                    },
                    padding: {
                      xs: '14px 16px',
                      sm: '12px 16px',
                      md: '12px 16px'
                    },
                    minHeight: {
                      xs: '48px',
                      sm: '44px',
                      md: '44px'
                    }
                  },
                  '& .MuiInputLabel-root': {
                    fontSize: {
                      xs: '1rem',
                      sm: '1rem',
                      md: '1rem'
                    },
                    transform: {
                      xs: 'translate(16px, 14px) scale(1)',
                      sm: 'translate(16px, 12px) scale(1)',
                      md: 'translate(16px, 12px) scale(1)'
                    },
                    '&.MuiInputLabel-shrink': {
                      transform: {
                        xs: 'translate(16px, -6px) scale(0.85)',
                        sm: 'translate(16px, -6px) scale(0.85)',
                        md: 'translate(16px, -6px) scale(0.85)'
                      }
                    }
                  }
                }}
                MenuProps={{ PaperProps: { sx: { maxHeight: '200px' } } }}
              >
                <MenuItem value="">
                  <em>Select a category</em>
                </MenuItem>
                {Array.isArray(categoriesArray) && categoriesArray.map((category: any) => {
                  console.log('🔍 Rendering MenuItem for category:', category.name, 'id:', category.id);
                  return (
                    <MenuItem key={category.id} value={category.id}>
                      {category.name}
                    </MenuItem>
                  );
                })}
                {Array.isArray(categories) && categories.length > 0 && null}
                <MenuItem
                  value="add-new-category"
                  sx={{
                    borderTop: '1px solid',
                    borderColor: 'divider',
                    fontStyle: 'italic',
                    color: 'primary.main',
                    minHeight: '48px',
                    padding: '12px 16px',
                    fontSize: '1rem'
                  }}
                >
                  + Add New Category
                </MenuItem>
              </Select>
            </FormControl>
          </Box>
        </Box>

        {/* Product Description */}
        <TextField
          fullWidth
          label="Description"
          value={formData.baseProduct.description}
          onChange={(e) => handleBaseProductChange('description', e.target.value)}
          multiline
          rows={isMobile ? 3 : 3}
          placeholder="Describe your product in detail..."
          size={isMobile ? "medium" : "medium"}
          sx={{
            '& .MuiInputBase-root': {
              minHeight: {
                xs: '72px', // 3 rows * 24px line height
                sm: '72px',
                md: '72px'
              }
            },
            '& .MuiInputBase-input': {
              fontSize: {
                xs: '1rem',
                sm: '1rem',
                md: '1rem'
              },
              padding: {
                xs: '14px 16px',
                sm: '12px 16px',
                md: '12px 16px'
              },
              lineHeight: {
                xs: 1.4,
                sm: 1.4,
                md: 1.4
              }
            },
            '& .MuiInputLabel-root': {
              fontSize: {
                xs: '1rem',
                sm: '1rem',
                md: '1rem'
              },
              transform: {
                xs: 'translate(16px, 14px) scale(1)',
                sm: 'translate(16px, 12px) scale(1)',
                md: 'translate(16px, 12px) scale(1)'
              },
              '&.MuiInputLabel-shrink': {
                transform: {
                  xs: 'translate(16px, -6px) scale(0.85)',
                  sm: 'translate(16px, -6px) scale(0.85)',
                  md: 'translate(16px, -6px) scale(0.85)'
                }
              }
            }
          }}
        />

        {/* Product Photos */}
        <Paper
          variant="outlined"
          sx={{
            p: { xs: 1.5, sm: 2, md: 2 },
            borderRadius: { xs: 1, sm: 1, md: 1 }
          }}
        >
          <Typography
            variant={isMobile ? "subtitle1" : "h6"}
            gutterBottom
            sx={{
              fontSize: {
                xs: '1rem',
                sm: '1.25rem',
                md: '1.25rem'
              }
            }}
          >
            Product Photos
          </Typography>
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              mb: { xs: 1.5, sm: 2, md: 2 },
              fontSize: {
                xs: '0.8rem',
                sm: '0.875rem',
                md: '0.875rem'
              }
            }}
          >
            Upload high-quality photos of your product. The first photo will be used as the main image.
          </Typography>

          <MultiPhotoUpload
            key={photoUploadKey}
            materialId="temp-base-product" // Temporary ID for base product photos
            maxFiles={10}
            maxFileSize={5}
            acceptedTypes={['image/jpeg', 'image/png', 'image/gif', 'image/webp']}
            existingPhotos={[]} // No existing photos for new products
            onUploadSuccess={handlePhotoUploadSuccess}
            onUploadError={handlePhotoUploadError}
            disabled={false}
          />
        </Paper>

        {/* Dynamic Properties */}
        <Paper
          variant="outlined"
          sx={{
            p: { xs: 1.5, sm: 2, md: 2 },
            borderRadius: { xs: 1, sm: 1, md: 1 }
          }}
        >
          <DynamicPropertiesInput
            properties={formData.baseProduct.dynamicProperties}
            onChange={handleDynamicPropertiesChange}
            disabled={false}
          />
        </Paper>
      </Box>

      {/* Error Summary */}
      {Object.keys(errors).length > 0 && (
        <Alert
          severity="error"
          sx={{
            mt: { xs: 2, sm: 2.5, md: 3 },
            '& .MuiAlert-message': {
              fontSize: {
                xs: '0.875rem',
                sm: '1rem',
                md: '1rem'
              }
            }
          }}
        >
          <Typography
            variant={isMobile ? "body2" : "subtitle2"}
            gutterBottom
            sx={{
              fontSize: {
                xs: '0.875rem',
                sm: '1rem',
                md: '1rem'
              },
              fontWeight: 600
            }}
          >
            Please fix the following errors:
          </Typography>
          <ul style={{
            margin: 0,
            paddingLeft: '20px',
            fontSize: isMobile ? '0.8rem' : '0.875rem'
          }}>
            {Object.entries(errors).map(([key, value]) => (
              <li key={key}>
                {typeof value === 'string' ? value : `${key}: ${JSON.stringify(value)}`}
              </li>
            ))}
          </ul>
        </Alert>
      )}

      {/* Add New Category Modal */}
      <Dialog
        open={isModalOpen}
        onClose={handleCloseModal}
        maxWidth={isMobile ? false : "sm"}
        fullWidth={!isMobile}
        fullScreen={isMobile}
        aria-labelledby="add-category-dialog-title"
        aria-describedby="add-category-dialog-description"
        sx={{
          '& .MuiDialog-paper': {
            width: isMobile ? '100%' : 'auto',
            maxWidth: isMobile ? 'none' : '500px',
            margin: isMobile ? 0 : '16px',
            borderRadius: isMobile ? 0 : '8px',
          }
        }}
      >
        <DialogTitle
          id="add-category-dialog-title"
          sx={{
            fontSize: {
              xs: '1.1rem',
              sm: '1.25rem',
              md: '1.25rem'
            }
          }}
        >
          Add New Category
          <IconButton
            aria-label="close"
            onClick={handleCloseModal}
            sx={{
              position: 'absolute',
              right: 8,
              top: 8,
              color: (theme) => theme.palette.grey[500],
            }}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent id="add-category-dialog-description">
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              mb: { xs: 1.5, sm: 2, md: 2 },
              fontSize: {
                xs: '0.8rem',
                sm: '0.875rem',
                md: '0.875rem'
              }
            }}
          >
            Create a new product category that will be available for all products.
          </Typography>
          <TextField
            autoFocus
            fullWidth
            label="Category Name"
            value={newCategoryName}
            onChange={(e) => setNewCategoryName(e.target.value)}
            error={!!categoryError}
            helperText={categoryError}
            disabled={isCreatingCategory}
            size={isMobile ? "medium" : "medium"}
            onKeyPress={(e) => {
              if (e.key === 'Enter' && !isCreatingCategory) {
                handleCreateCategory();
              }
            }}
            inputProps={{
              'aria-label': 'New category name',
            }}
            sx={{
              '& .MuiInputBase-root': {
                minHeight: {
                  xs: '48px',
                  sm: '44px',
                  md: '44px'
                }
              },
              '& .MuiInputBase-input': {
                fontSize: {
                  xs: '1rem',
                  sm: '1rem',
                  md: '1rem'
                },
                padding: {
                  xs: '14px 16px',
                  sm: '12px 16px',
                  md: '12px 16px'
                }
              },
              '& .MuiInputLabel-root': {
                fontSize: {
                  xs: '1rem',
                  sm: '1rem',
                  md: '1rem'
                },
                transform: {
                  xs: 'translate(16px, 14px) scale(1)',
                  sm: 'translate(16px, 12px) scale(1)',
                  md: 'translate(16px, 12px) scale(1)'
                },
                '&.MuiInputLabel-shrink': {
                  transform: {
                    xs: 'translate(16px, -6px) scale(0.85)',
                    sm: 'translate(16px, -6px) scale(0.85)',
                    md: 'translate(16px, -6px) scale(0.85)'
                  }
                }
              }
            }}
          />
        </DialogContent>
        <DialogActions
          sx={{
            flexDirection: isMobile ? 'column' : 'row',
            gap: isMobile ? 1 : 0,
            padding: {
              xs: '12px 16px',
              sm: '16px 24px',
              md: '16px 24px'
            },
            '& .MuiButton-root': {
              width: isMobile ? '100%' : 'auto',
              minWidth: isMobile ? 'auto' : '120px',
              minHeight: '44px',
              borderRadius: '8px',
              boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
              '&.MuiButton-contained': {
                backgroundColor: '#007BFF',
                '&:hover': {
                  backgroundColor: '#0056b3'
                }
              },
              '&.MuiButton-text': {
                backgroundColor: '#6C757D',
                color: 'white',
                '&:hover': {
                  backgroundColor: '#5a6268'
                }
              },
              '&.MuiButton-outlined': {
                backgroundColor: '#6C757D',
                color: 'white',
                border: 'none',
                '&:hover': {
                  backgroundColor: '#5a6268'
                }
              }
            }
          }}
        >
          <Button
            onClick={handleCloseModal}
            disabled={isCreatingCategory}
            aria-label="Cancel adding new category"
            sx={{ order: isMobile ? 2 : 1 }}
          >
            Cancel
          </Button>
          <Button
            onClick={handleCreateCategory}
            variant="contained"
            disabled={isCreatingCategory || !newCategoryName.trim()}
            startIcon={isCreatingCategory ? <CircularProgress size={16} /> : null}
            aria-label="Create new category"
            sx={{ order: isMobile ? 1 : 2 }}
          >
            {isCreatingCategory ? 'Creating...' : 'Create Category'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ProductWizardStep1;