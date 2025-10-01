import React, { useState, useCallback } from 'react';
import {
  Box,
  TextField,
  Typography,
  Button,
  IconButton,
  Paper,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  Card,
  CardContent,
  CardActions,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  CircularProgress,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
  PhotoCamera as PhotoIcon,
  Close as CloseIcon,
} from '@mui/icons-material';
import { ProductWizardFormData, ProductVariantFormData, UnitOfMeasureDto, FormValidationErrors } from '../../services/api.types';
import { productsAPI } from '../../services/api';
import DynamicPropertiesInput from '../materials/DynamicPropertiesInput';
import MultiPhotoUpload from '../materials/MultiPhotoUpload';

interface ProductWizardStep2Props {
  formData: ProductWizardFormData;
  onChange: (updates: Partial<ProductWizardFormData>) => void;
  unitsOfMeasure: UnitOfMeasureDto[];
  errors: FormValidationErrors;
  onUnitOfMeasureCreated?: () => void; // Callback to refresh units of measure after creation
}

/**
 * Step 2: Product Variants Form
 * Handles multiple product variants with size, color, unit, stock, and photos
 */
const ProductWizardStep2: React.FC<ProductWizardStep2Props> = ({
  formData,
  onChange,
  unitsOfMeasure,
  errors,
  onUnitOfMeasureCreated
}) => {
  const [editingVariantIndex, setEditingVariantIndex] = useState<number | null>(null);

  // Modal state for adding new unit of measure
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [newUnitName, setNewUnitName] = useState('');
  const [newUnitSymbol, setNewUnitSymbol] = useState('');
  const [newUnitType, setNewUnitType] = useState('');
  const [isCreatingUnit, setIsCreatingUnit] = useState(false);
  const [unitError, setUnitError] = useState<string | null>(null);

  const handleVariantChange = useCallback((index: number, field: string, value: any) => {
    const updatedVariants = [...formData.variants];
    updatedVariants[index] = {
      ...updatedVariants[index],
      [field]: value
    };
    onChange({ variants: updatedVariants });
  }, [formData.variants, onChange]);

  const handleVariantDynamicPropertiesChange = useCallback((index: number, properties: Record<string, any>) => {
    handleVariantChange(index, 'dynamicProperties', properties);
  }, [handleVariantChange]);

  const handleVariantPhotosChange = useCallback((index: number, photos: File[]) => {
    handleVariantChange(index, 'photos', photos);
  }, [handleVariantChange]);

  const addNewVariant = useCallback(() => {
    const newVariant: ProductVariantFormData = {
      name: '',
      size: '',
      color: '',
      stockQuantity: 0,
      unitOfMeasureId: '',
      unitValue: 1,
      usageNotes: '',
      photos: [],
      dynamicProperties: {},
      isNew: true
    };
    onChange({ variants: [...formData.variants, newVariant] });
  }, [formData.variants, onChange]);

  const removeVariant = useCallback((index: number) => {
    if (formData.variants.length > 1) {
      const updatedVariants = formData.variants.filter((_, i) => i !== index);
      onChange({ variants: updatedVariants });
    }
  }, [formData.variants, onChange]);

  const getVariantError = (index: number, field: string) => {
    const variantErrors = errors[`variants[${index}]`];
    if (variantErrors && typeof variantErrors === 'object' && !Array.isArray(variantErrors)) {
      return (variantErrors as any)[field] as string;
    }
    return undefined;
  };

  // Modal handlers
  const handleOpenModal = useCallback(() => {
    console.log('🔍 ProductWizardStep2: Opening nested unit modal');
    console.log('🔍 Parent modal root aria-hidden:', document.querySelector('[role="presentation"].MuiModal-root')?.getAttribute('aria-hidden'));
    setIsModalOpen(true);
    setNewUnitName('');
    setNewUnitSymbol('');
    setNewUnitType('');
    setUnitError(null);
  }, []);

  const handleCloseModal = useCallback(() => {
    setIsModalOpen(false);
    setNewUnitName('');
    setNewUnitSymbol('');
    setNewUnitType('');
    setUnitError(null);
  }, []);

  // Unit creation handler
  const handleCreateUnitOfMeasure = useCallback(async () => {
    if (!newUnitName.trim()) {
      setUnitError('Unit name is required');
      return;
    }

    if (!newUnitSymbol.trim()) {
      setUnitError('Unit symbol is required');
      return;
    }

    setIsCreatingUnit(true);
    setUnitError(null);

    try {
      const response = await productsAPI.createUnitOfMeasure({
        name: newUnitName.trim(),
        symbol: newUnitSymbol.trim(),
        type: newUnitType.trim() || undefined,
      });

      if (response.data?.success) {
        // Refresh units of measure by calling the parent callback
        if (onUnitOfMeasureCreated) {
          onUnitOfMeasureCreated();
        }
        handleCloseModal();
      } else {
        setUnitError(response.data?.message || 'Failed to create unit of measure');
      }
    } catch (error: any) {
      console.error('Error creating unit of measure:', error);
      setUnitError(error.response?.data?.message || error.message || 'Failed to create unit of measure');
    } finally {
      setIsCreatingUnit(false);
    }
  }, [newUnitName, newUnitSymbol, newUnitType, handleCloseModal, onUnitOfMeasureCreated]);

  const renderVariantCard = (variant: ProductVariantFormData, index: number) => {
    const isEditing = editingVariantIndex === index;
    const hasErrors = Object.keys(errors).some(key => key.startsWith(`variants[${index}]`));

    return (
      <Card
        key={index}
        variant="outlined"
        sx={{
          mb: 2,
          borderColor: hasErrors ? 'error.main' : 'divider',
          position: 'relative'
        }}
      >
        <CardContent>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6">
              Variant {index + 1}
              {hasErrors && <Chip label="Has Errors" color="error" size="small" sx={{ ml: 1 }} />}
            </Typography>
            <Box>
              <IconButton
                size="medium"
                onClick={() => setEditingVariantIndex(isEditing ? null : index)}
                color={isEditing ? 'primary' : 'default'}
                sx={{
                  minWidth: {
                    xs: '48px',
                    sm: '44px',
                    md: '44px'
                  },
                  minHeight: {
                    xs: '48px',
                    sm: '44px',
                    md: '44px'
                  },
                  width: {
                    xs: '48px',
                    sm: '44px',
                    md: '44px'
                  },
                  height: {
                    xs: '48px',
                    sm: '44px',
                    md: '44px'
                  },
                  marginRight: '4px'
                }}
              >
                <EditIcon />
              </IconButton>
              {formData.variants.length > 1 && (
                <IconButton
                  size="medium"
                  onClick={() => removeVariant(index)}
                  color="error"
                  sx={{
                    minWidth: {
                      xs: '48px',
                      sm: '44px',
                      md: '44px'
                    },
                    minHeight: {
                      xs: '48px',
                      sm: '44px',
                      md: '44px'
                    },
                    width: {
                      xs: '48px',
                      sm: '44px',
                      md: '44px'
                    },
                    height: {
                      xs: '48px',
                      sm: '44px',
                      md: '44px'
                    }
                  }}
                >
                  <DeleteIcon />
                </IconButton>
              )}
            </Box>
          </Box>

          {isEditing ? (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              {/* Variant Name */}
              <TextField
                fullWidth
                label="Variant Name"
                value={variant.name}
                onChange={(e) => handleVariantChange(index, 'name', e.target.value)}
                error={!!getVariantError(index, 'name')}
                helperText={getVariantError(index, 'name')}
                placeholder="e.g., Red Large"
                size="medium"
                sx={{
                  '& .MuiInputBase-root': {
                    minHeight: {
                      xs: '48px',
                      sm: '44px',
                      md: '44px'
                    }
                  },
                  '& .MuiInputBase-input': {
                    fontSize: '1rem',
                    padding: {
                      xs: '14px 16px',
                      sm: '12px 16px',
                      md: '12px 16px'
                    }
                  },
                  '& .MuiInputLabel-root': {
                    fontSize: '1rem',
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

              {/* Size and Color Row */}
              <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                <TextField
                  sx={{
                    flex: 1,
                    minWidth: '150px',
                    '& .MuiInputBase-root': {
                      minHeight: {
                        xs: '48px',
                        sm: '44px',
                        md: '44px'
                      }
                    },
                    '& .MuiInputBase-input': {
                      fontSize: '1rem',
                      padding: {
                        xs: '14px 16px',
                        sm: '12px 16px',
                        md: '12px 16px'
                      }
                    },
                    '& .MuiInputLabel-root': {
                      fontSize: '1rem',
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
                  label="Size"
                  value={variant.size || ''}
                  onChange={(e) => handleVariantChange(index, 'size', e.target.value)}
                  placeholder="e.g., Large, XL, 10oz"
                  size="medium"
                />
                <TextField
                  sx={{
                    flex: 1,
                    minWidth: '150px',
                    '& .MuiInputBase-root': {
                      minHeight: {
                        xs: '48px',
                        sm: '44px',
                        md: '44px'
                      }
                    },
                    '& .MuiInputBase-input': {
                      fontSize: '1rem',
                      padding: {
                        xs: '14px 16px',
                        sm: '12px 16px',
                        md: '12px 16px'
                      }
                    },
                    '& .MuiInputLabel-root': {
                      fontSize: '1rem',
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
                  label="Color"
                  value={variant.color || ''}
                  onChange={(e) => handleVariantChange(index, 'color', e.target.value)}
                  placeholder="e.g., Red, Blue, Black"
                  size="medium"
                />
              </Box>

              {/* Stock and Unit Row */}
              <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                <TextField
                  sx={{
                    flex: 1,
                    minWidth: '150px',
                    '& .MuiInputBase-root': {
                      minHeight: {
                        xs: '48px',
                        sm: '44px',
                        md: '44px'
                      }
                    },
                    '& .MuiInputBase-input': {
                      fontSize: '1rem',
                      padding: {
                        xs: '14px 16px',
                        sm: '12px 16px',
                        md: '12px 16px'
                      }
                    },
                    '& .MuiInputLabel-root': {
                      fontSize: '1rem',
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
                  label="Stock Quantity"
                  type="number"
                  value={variant.stockQuantity}
                  onChange={(e) => handleVariantChange(index, 'stockQuantity', parseInt(e.target.value) || 0)}
                  error={!!getVariantError(index, 'stockQuantity')}
                  helperText={getVariantError(index, 'stockQuantity')}
                  size="medium"
                  inputProps={{ min: 0 }}
                />

                <FormControl sx={{
                  flex: 1,
                  minWidth: '150px',
                  '& .MuiInputBase-root': {
                    minHeight: {
                      xs: '48px',
                      sm: '44px',
                      md: '44px'
                    }
                  },
                  '& .MuiSelect-select': {
                    fontSize: '1rem',
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
                    fontSize: '1rem',
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
                }} size="medium">
                  <InputLabel>Unit of Measure</InputLabel>
                  <Select
                    value={variant.unitOfMeasureId}
                    label="Unit of Measure"
                    onChange={(e) => {
                      if (e.target.value === 'add-new-unit') {
                        handleOpenModal();
                      } else {
                        handleVariantChange(index, 'unitOfMeasureId', e.target.value);
                      }
                    }}
                    error={!!getVariantError(index, 'unitOfMeasureId')}
                  >
                    {unitsOfMeasure.map((unit) => (
                      <MenuItem key={unit.id} value={unit.id}>
                        {unit.name} ({unit.symbol})
                      </MenuItem>
                    ))}
                    <MenuItem
                      value="add-new-unit"
                      sx={{
                        borderTop: '1px solid',
                        borderColor: 'divider',
                        fontStyle: 'italic',
                        color: 'primary.main',
                        minHeight: {
                          xs: '48px',
                          sm: '44px',
                          md: '44px'
                        },
                        padding: {
                          xs: '14px 16px',
                          sm: '12px 16px',
                          md: '12px 16px'
                        }
                      }}
                    >
                      + Add New Unit of Measure
                    </MenuItem>
                  </Select>
                </FormControl>

                <TextField
                  sx={{
                    flex: 1,
                    minWidth: '150px',
                    '& .MuiInputBase-root': {
                      minHeight: {
                        xs: '48px',
                        sm: '44px',
                        md: '44px'
                      }
                    },
                    '& .MuiInputBase-input': {
                      fontSize: '1rem',
                      padding: {
                        xs: '14px 16px',
                        sm: '12px 16px',
                        md: '12px 16px'
                      }
                    },
                    '& .MuiInputLabel-root': {
                      fontSize: '1rem',
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
                  label="Unit Value"
                  type="number"
                  value={variant.unitValue}
                  onChange={(e) => handleVariantChange(index, 'unitValue', parseFloat(e.target.value) || 1)}
                  size="medium"
                  inputProps={{ min: 0, step: 0.1 }}
                />
              </Box>

              {/* Usage Notes */}
              <TextField
                fullWidth
                label="Usage Notes"
                value={variant.usageNotes}
                onChange={(e) => handleVariantChange(index, 'usageNotes', e.target.value)}
                multiline
                rows={3}
                placeholder="Optional notes about this variant..."
                size="medium"
                sx={{
                  '& .MuiInputBase-root': {
                    minHeight: {
                      xs: '80px', // 3 rows * 24px line height + padding
                      sm: '72px',
                      md: '72px'
                    }
                  },
                  '& .MuiInputBase-input': {
                    fontSize: '1rem',
                    padding: {
                      xs: '14px 16px',
                      sm: '12px 16px',
                      md: '12px 16px'
                    },
                    lineHeight: 1.4
                  },
                  '& .MuiInputLabel-root': {
                    fontSize: '1rem',
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

              {/* Photos */}
              <Box>
                <Typography variant="subtitle2" gutterBottom>
                  Variant Photos
                </Typography>
                <MultiPhotoUpload
                  materialId={`temp-variant-${index}`}
                  maxFiles={5}
                  maxFileSize={5}
                  acceptedTypes={['image/jpeg', 'image/png', 'image/gif', 'image/webp']}
                  existingPhotos={[]}
                  onUploadSuccess={(urls) => console.log('Variant photos uploaded:', urls)}
                  onUploadError={(error) => console.error('Variant photo upload error:', error)}
                  disabled={false}
                />
              </Box>

              {/* Dynamic Properties */}
              <Box>
                <Typography variant="subtitle2" gutterBottom>
                  Additional Properties
                </Typography>
                <DynamicPropertiesInput
                  properties={variant.dynamicProperties}
                  onChange={(properties) => handleVariantDynamicPropertiesChange(index, properties)}
                  disabled={false}
                />
              </Box>
            </Box>
          ) : (
            <Box>
              <Typography variant="body1" sx={{ fontWeight: 'bold', mb: 1 }}>
                {variant.name || 'Unnamed Variant'}
              </Typography>

              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 1 }}>
                {variant.size && <Chip label={`Size: ${variant.size}`} size="small" variant="outlined" />}
                {variant.color && <Chip label={`Color: ${variant.color}`} size="small" variant="outlined" />}
                <Chip label={`Stock: ${variant.stockQuantity}`} size="small" variant="outlined" />
                {variant.unitOfMeasureId && (
                  <Chip
                    label={`Unit: ${unitsOfMeasure.find(u => u.id === variant.unitOfMeasureId)?.name || 'Unknown'}`}
                    size="small"
                    variant="outlined"
                  />
                )}
              </Box>

              {variant.usageNotes && (
                <Typography variant="body2" color="text.secondary">
                  {variant.usageNotes}
                </Typography>
              )}

              <Typography variant="caption" color="text.secondary">
                Click edit to modify this variant
              </Typography>
            </Box>
          )}
        </CardContent>

        {isEditing && (
          <CardActions>
            <Button
              size="medium"
              onClick={() => setEditingVariantIndex(null)}
              sx={{
                minHeight: {
                  xs: '48px',
                  sm: '44px',
                  md: '44px'
                },
                fontSize: '1rem',
                padding: {
                  xs: '14px 16px',
                  sm: '10px 16px',
                  md: '10px 16px'
                },
                borderRadius: '8px',
                boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
                '&:hover': {
                  boxShadow: '0 4px 8px rgba(0,0,0,0.15)'
                }
              }}
            >
              Done Editing
            </Button>
          </CardActions>
        )}
      </Card>
    );
  };

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Product Variants
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Add different variations of your product (sizes, colors, etc.). Each variant can have its own stock level and photos.
      </Typography>

      {/* Variants List */}
      <Box sx={{ mb: 3 }}>
        {formData.variants.map((variant, index) => renderVariantCard(variant, index))}
      </Box>

      {/* Add New Variant Button */}
      <Box sx={{ textAlign: 'center', mb: 3 }}>
        <Button
          variant="outlined"
          startIcon={<AddIcon />}
          onClick={addNewVariant}
          size="large"
          sx={{
            minHeight: {
              xs: '52px',
              sm: '48px',
              md: '48px'
            },
            fontSize: '1rem',
            padding: {
              xs: '14px 24px',
              sm: '12px 24px',
              md: '12px 24px'
            },
            borderRadius: '8px',
            boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
            '&:hover': {
              boxShadow: '0 4px 8px rgba(0,0,0,0.15)'
            }
          }}
        >
          Add Another Variant
        </Button>
      </Box>

      {/* Help Text */}
      <Alert severity="info" sx={{ mb: 2 }}>
        <Typography variant="body2" component="div">
          <strong>Tips:</strong>
          <ul style={{ margin: '8px 0', paddingLeft: '20px' }}>
            <li>Each variant should have a unique combination of attributes (size, color, etc.)</li>
            <li>Stock quantities are managed per variant</li>
            <li>You can upload specific photos for each variant</li>
            <li>Use dynamic properties to add custom attributes like material, style, etc.</li>
          </ul>
        </Typography>
      </Alert>

      {/* Error Summary */}
      {Object.keys(errors).some(key => key.startsWith('variants[')) && (
        <Alert severity="error">
          <Typography variant="subtitle2" gutterBottom>
            Please fix the following errors in your variants:
          </Typography>
          <Typography variant="body2" component="div">
            <ul style={{ margin: 0, paddingLeft: '20px' }}>
              {Object.entries(errors).map(([key, value]) => {
                if (key.startsWith('variants[')) {
                  return (
                    <li key={key}>
                      {key}: {typeof value === 'string' ? value : JSON.stringify(value)}
                    </li>
                  );
                }
                return null;
              }).filter(Boolean)}
            </ul>
          </Typography>
        </Alert>
      )}

      {/* Add New Unit of Measure Modal */}
      <Dialog
        open={isModalOpen}
        onClose={handleCloseModal}
        maxWidth="sm"
        fullWidth
        aria-labelledby="add-unit-dialog-title"
        aria-describedby="add-unit-dialog-description"
      >
        <DialogTitle id="add-unit-dialog-title">
          Add New Unit of Measure
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
        <DialogContent id="add-unit-dialog-description">
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Create a new unit of measure that will be available for all product variants.
          </Typography>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <TextField
              autoFocus
              fullWidth
              label="Unit Name"
              value={newUnitName}
              onChange={(e) => setNewUnitName(e.target.value)}
              error={!!unitError && unitError.includes('name')}
              helperText={unitError && unitError.includes('name') ? unitError : ''}
              disabled={isCreatingUnit}
              placeholder="e.g., Kilogram, Liter, Piece"
              size="medium"
              onKeyPress={(e) => {
                if (e.key === 'Enter' && !isCreatingUnit) {
                  handleCreateUnitOfMeasure();
                }
              }}
              inputProps={{
                'aria-label': 'New unit name',
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
                  fontSize: '1rem',
                  padding: {
                    xs: '14px 16px',
                    sm: '12px 16px',
                    md: '12px 16px'
                  }
                },
                '& .MuiInputLabel-root': {
                  fontSize: '1rem',
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
            <TextField
              fullWidth
              label="Unit Symbol"
              value={newUnitSymbol}
              onChange={(e) => setNewUnitSymbol(e.target.value)}
              error={!!unitError && unitError.includes('symbol')}
              helperText={unitError && unitError.includes('symbol') ? unitError : 'e.g., kg, L, pcs'}
              disabled={isCreatingUnit}
              placeholder="e.g., kg, L, pcs"
              size="medium"
              onKeyPress={(e) => {
                if (e.key === 'Enter' && !isCreatingUnit) {
                  handleCreateUnitOfMeasure();
                }
              }}
              inputProps={{
                'aria-label': 'New unit symbol',
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
                  fontSize: '1rem',
                  padding: {
                    xs: '14px 16px',
                    sm: '12px 16px',
                    md: '12px 16px'
                  }
                },
                '& .MuiInputLabel-root': {
                  fontSize: '1rem',
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
            <TextField
              fullWidth
              label="Unit Type (Optional)"
              value={newUnitType}
              onChange={(e) => setNewUnitType(e.target.value)}
              disabled={isCreatingUnit}
              placeholder="e.g., Weight, Volume, Count"
              size="medium"
              onKeyPress={(e) => {
                if (e.key === 'Enter' && !isCreatingUnit) {
                  handleCreateUnitOfMeasure();
                }
              }}
              inputProps={{
                'aria-label': 'New unit type',
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
                  fontSize: '1rem',
                  padding: {
                    xs: '14px 16px',
                    sm: '12px 16px',
                    md: '12px 16px'
                  }
                },
                '& .MuiInputLabel-root': {
                  fontSize: '1rem',
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
          {unitError && !unitError.includes('name') && !unitError.includes('symbol') && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {unitError}
            </Alert>
          )}
        </DialogContent>
        <DialogActions
          sx={{
            flexDirection: 'column',
            gap: 1.5,
            padding: '16px',
            '& .MuiButton-root': {
              width: '100%',
              minHeight: '48px',
              fontSize: '1rem',
              padding: '12px 16px',
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
            disabled={isCreatingUnit}
            aria-label="Cancel adding new unit of measure"
            sx={{ order: 2 }}
          >
            Cancel
          </Button>
          <Button
            onClick={handleCreateUnitOfMeasure}
            variant="contained"
            disabled={isCreatingUnit || !newUnitName.trim() || !newUnitSymbol.trim()}
            startIcon={isCreatingUnit ? <CircularProgress size={16} /> : null}
            aria-label="Create new unit of measure"
            sx={{ order: 1 }}
          >
            {isCreatingUnit ? 'Creating...' : 'Create Unit'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ProductWizardStep2;