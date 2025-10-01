import React, { useState, useEffect, Suspense, lazy, useCallback, useMemo, useRef } from 'react';
import {
  Box,
  Stepper,
  Step,
  StepLabel,
  Button,
  Typography,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  CircularProgress,
  Snackbar,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import { ArrowBack, ArrowForward, Send } from '@mui/icons-material';
import {
  ProductWizardFormData,
  ProductWizardStep,
  FormValidationErrors,
  ProductCategoryDto,
  UnitOfMeasureDto
} from '../../services/api.types';
import { productsAPI } from '../../services/api';
import './ProductWizard.css';

// Performance optimization utilities
const debounce = (func: Function, wait: number) => {
  let timeout: NodeJS.Timeout;
  return (...args: any[]) => {
    clearTimeout(timeout);
    timeout = setTimeout(() => func.apply(null, args), wait);
  };
};

const cache = new Map<string, { data: any; timestamp: number; ttl: number }>();

const getCachedData = (key: string) => {
  const cached = cache.get(key);
  if (cached && Date.now() - cached.timestamp < cached.ttl) {
    return cached.data;
  }
  cache.delete(key);
  return null;
};

const setCachedData = (key: string, data: any, ttl: number = 5 * 60 * 1000) => {
  cache.set(key, { data, timestamp: Date.now(), ttl });
};

// Lazy load step components for better performance
const ProductWizardStep1 = lazy(() => import('./ProductWizardStep1'));
const ProductWizardStep2 = lazy(() => import('./ProductWizardStep2'));
const ProductWizardStep3 = lazy(() => import('./ProductWizardStep3'));

// Loading component for lazy-loaded steps
const StepLoadingFallback = () => (
  <Box
    display="flex"
    flexDirection="column"
    justifyContent="center"
    alignItems="center"
    p={4}
    minHeight={200}
  >
    <CircularProgress size={32} />
    <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
      Loading step...
    </Typography>
  </Box>
);

interface ProductWizardProps {
  /** Callback when the wizard is completed successfully */
  onComplete?: (productId: string) => void;
  /** Callback when the wizard is cancelled */
  onCancel?: () => void;
  /** Whether the wizard is open */
  open: boolean;
  /** Pre-filled form data for editing */
  initialData?: Partial<ProductWizardFormData>;
  /** Whether this is an edit operation */
  isEdit?: boolean;
  /** Product ID for editing */
  productId?: string;
}

/**
 * Main ProductWizard component for creating/editing products with variants
 */
const ProductWizard: React.FC<ProductWizardProps> = ({
  onComplete,
  onCancel,
  open,
  initialData,
  isEdit = false,
  productId
}) => {
  // Responsive breakpoints
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  // Refs for focus management
  const dialogRef = useRef<HTMLDivElement>(null);
  const stepperRef = useRef<HTMLDivElement>(null);
  const mainContentRef = useRef<HTMLDivElement>(null);
  const skipLinkRef = useRef<HTMLAnchorElement>(null);

  // Form state
  const [formData, setFormData] = useState<ProductWizardFormData>({
    baseProduct: {
      name: '',
      description: '',
      categoryId: '',
      photos: [],
      dynamicProperties: {}
    },
    variants: [{
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
    }],
    isDraft: false,
    currentStep: 0,
    lastSavedAt: undefined
  });

  // UI state
  const [activeStep, setActiveStep] = useState(0);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [errors, setErrors] = useState<FormValidationErrors>({});
  const [showExitDialog, setShowExitDialog] = useState(false);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' | 'info' }>({
    open: false,
    message: '',
    severity: 'info'
  });

  // Data state
  const [categories, setCategories] = useState<ProductCategoryDto[]>([]);
  const [unitsOfMeasure, setUnitsOfMeasure] = useState<UnitOfMeasureDto[]>([]);

  // Live region for announcements
  const [liveRegionMessage, setLiveRegionMessage] = useState<string>('');

  // Define wizard steps (memoized since it's static)
  const steps: ProductWizardStep[] = useMemo(() => [
    {
      id: 0,
      title: 'Base Product Details',
      description: 'Enter basic product information, photos, and properties',
      isValid: false,
      isCompleted: false
    },
    {
      id: 1,
      title: 'Product Variants',
      description: 'Add product variants with sizes, colors, and stock information',
      isValid: false,
      isCompleted: false
    },
    {
      id: 2,
      title: 'Review & Submit',
      description: 'Review all information and submit the product',
      isValid: false,
      isCompleted: false
    }
  ], []);


  // Optimized loadDependencies with caching and debouncing
  const loadDependencies = useCallback(async () => {
    try {
      console.log('🔄 Loading dependencies...');

      // Check cache first
      const cachedCategories = getCachedData('categories');
      const cachedUnits = getCachedData('units');

      let categoriesData: ProductCategoryDto[] = [];
      let unitsData: UnitOfMeasureDto[] = [];

      // Load categories if not cached
      if (cachedCategories) {
        categoriesData = cachedCategories;
        console.log('✅ Using cached categories');
      } else {
        const categoriesResponse = await productsAPI.getProductCategories();
        if (categoriesResponse.data.success) {
          categoriesData = categoriesResponse.data.data?.items || categoriesResponse.data.items || [];
          setCachedData('categories', categoriesData);
          console.log('✅ Loaded and cached categories:', categoriesData.length);
        }
      }

      // Load units if not cached
      if (cachedUnits) {
        unitsData = cachedUnits;
        console.log('✅ Using cached units');
      } else {
        const unitsResponse = await productsAPI.getUnitsOfMeasure();
        if (unitsResponse.data.success) {
          unitsData = unitsResponse.data.data || unitsResponse.data.items || [];
          setCachedData('units', unitsData);
          console.log('✅ Loaded and cached units:', unitsData.length);
        }
      }

      setCategories(categoriesData);
      setUnitsOfMeasure(unitsData);
    } catch (error) {
      console.error('❌ Error loading dependencies:', error);
      // Set empty arrays on error to prevent undefined state
      setCategories([]);
      setUnitsOfMeasure([]);
      showSnackbar('Failed to load form data', 'error');
    }
  }, []);

  // Debounced version of loadDependencies
  const debouncedLoadDependencies = useMemo(
    () => debounce(loadDependencies, 300),
    [loadDependencies]
  );

  // Optimized loadProductData with caching
  const loadProductData = useCallback(async (id: string) => {
    const cacheKey = `product-${id}`;
    const cachedProduct = getCachedData(cacheKey);

    if (cachedProduct) {
      console.log('✅ Using cached product data for:', id);
      setFormData(cachedProduct);
      setLoading(false);
      return;
    }

    setLoading(true);
    try {
      const [productResponse, variantsResponse] = await Promise.all([
        productsAPI.getProduct(id),
        productsAPI.getProductVariants(id)
      ]);

      if (productResponse.data.success && variantsResponse.data.success) {
        const product = productResponse.data.data;
        const variants = variantsResponse.data.data || [];

        const productData = {
          baseProduct: {
            name: product.name,
            description: product.description || '',
            categoryId: product.categoryId || '',
            photos: [], // Will be loaded separately
            dynamicProperties: product.dynamicProperties || {}
          },
          variants: variants.map((variant: any) => ({
            id: variant.id,
            name: variant.name,
            size: variant.dynamicProperties?.size || '',
            color: variant.dynamicProperties?.color || '',
            stockQuantity: variant.stockQuantity,
            unitOfMeasureId: variant.unitOfMeasureId,
            unitValue: variant.unitValue,
            usageNotes: variant.usageNotes || '',
            photos: [], // Will be loaded separately
            dynamicProperties: variant.dynamicProperties || {},
            isNew: false
          })),
          isDraft: false,
          currentStep: 0
        };

        setCachedData(cacheKey, productData, 10 * 60 * 1000); // Cache for 10 minutes
        setFormData(productData);
        console.log('✅ Loaded and cached product data for:', id);
      }
    } catch (error) {
      console.error('Error loading product data:', error);
      showSnackbar('Failed to load product data', 'error');
    } finally {
      setLoading(false);
    }
  }, []);

  // Debounced version of loadProductData
  const debouncedLoadProductData = useMemo(
    () => debounce(loadProductData, 500),
    [loadProductData]
  );

  // Load initial data and dependencies with debouncing
  useEffect(() => {
    if (open) {
      console.log('🔍 ProductWizard: Dialog opened, checking aria-hidden and focus states');
      // Log aria-hidden state on modal root
      const modalRoot = document.querySelector('[role="presentation"].MuiModal-root');
      if (modalRoot) {
        console.log('🔍 Modal root aria-hidden:', modalRoot.getAttribute('aria-hidden'));
        console.log('🔍 Modal root element:', modalRoot);
      }

      // Log current active element
      console.log('🔍 Active element on dialog open:', document.activeElement);

      // Add focus event listeners
      const handleFocusIn = (e: FocusEvent) => {
        const target = e.target as Element;
        console.log('🔍 Focus moved to:', target, 'aria-hidden ancestor:', target?.closest('[aria-hidden="true"]'));
      };

      const handleFocusOut = (e: FocusEvent) => {
        console.log('🔍 Focus moved from:', e.target, 'to:', e.relatedTarget);
      };

      document.addEventListener('focusin', handleFocusIn);
      document.addEventListener('focusout', handleFocusOut);

      // Use debounced functions to prevent excessive API calls
      debouncedLoadDependencies();
      if (initialData) {
        setFormData(prev => ({ ...prev, ...initialData }));
      }
      if (isEdit && productId) {
        debouncedLoadProductData(productId);
      }

      // Cleanup listeners
      return () => {
        document.removeEventListener('focusin', handleFocusIn);
        document.removeEventListener('focusout', handleFocusOut);
      };
    }
  }, [open, initialData, isEdit, productId, debouncedLoadDependencies, debouncedLoadProductData]);

  const showSnackbar = useCallback((message: string, severity: 'success' | 'error' | 'info' = 'info') => {
    setSnackbar({ open: true, message, severity });
  }, []);

  const handleCloseSnackbar = useCallback(() => {
    setSnackbar(prev => ({ ...prev, open: false }));
  }, []);

  // Focus management functions
  const focusFirstFocusableElement = useCallback((container: HTMLElement) => {
    const focusableElements = container.querySelectorAll(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );
    const firstElement = focusableElements[0] as HTMLElement;
    if (firstElement) {
      firstElement.focus();
    }
  }, []);

  const announceToScreenReader = useCallback((message: string) => {
    setLiveRegionMessage(message);
    // Clear the message after it's been announced
    setTimeout(() => setLiveRegionMessage(''), 1000);
  }, []);

  const handleStepChange = useCallback((newStep: number) => {
    setActiveStep(newStep);
    setFormData(prev => ({ ...prev, currentStep: newStep }));

    // Announce step change to screen readers
    const stepTitle = steps[newStep]?.title || `Step ${newStep + 1}`;
    announceToScreenReader(`Navigated to ${stepTitle}`);

    // Focus management for step changes
    setTimeout(() => {
      if (mainContentRef.current) {
        focusFirstFocusableElement(mainContentRef.current);
      }
    }, 100);
  }, [steps, announceToScreenReader, focusFirstFocusableElement]);

  // Keyboard navigation handler
  const handleKeyDown = useCallback((event: React.KeyboardEvent) => {
    if (event.key === 'Escape') {
      handleExit();
    }
  }, []);

  // Stepper keyboard navigation
  const handleStepperKeyDown = useCallback((event: React.KeyboardEvent) => {
    const { key } = event;

    switch (key) {
      case 'ArrowLeft':
        event.preventDefault();
        if (activeStep > 0) {
          handleStepChange(activeStep - 1);
        }
        break;
      case 'ArrowRight':
        event.preventDefault();
        if (activeStep < steps.length - 1) {
          handleStepChange(activeStep + 1);
        }
        break;
      case 'Home':
        event.preventDefault();
        handleStepChange(0);
        break;
      case 'End':
        event.preventDefault();
        handleStepChange(steps.length - 1);
        break;
      case 'Enter':
      case ' ':
        event.preventDefault();
        // Allow default button behavior
        break;
      default:
        break;
    }
  }, [activeStep, steps.length, handleStepChange]);

  const validateStep = useCallback((step: number): boolean => {
    const newErrors: FormValidationErrors = {};

    switch (step) {
      case 0: // Base Product
        if (!formData.baseProduct.name.trim()) {
          newErrors.baseProduct = { name: 'Product name is required' };
        }
        break;

      case 1: // Variants
        formData.variants.forEach((variant, index) => {
          const variantErrors: FormValidationErrors = {};
          if (!variant.name.trim()) {
            variantErrors.name = 'Variant name is required';
          }
          if (variant.stockQuantity < 0) {
            variantErrors.stockQuantity = 'Stock quantity must be non-negative';
          }
          if (!variant.unitOfMeasureId) {
            variantErrors.unitOfMeasureId = 'Unit of measure is required';
          }
          if (Object.keys(variantErrors).length > 0) {
            newErrors[`variants[${index}]`] = variantErrors;
          }
        });
        break;

      case 2: // Review
        // All validation should be done in previous steps
        break;
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  }, [formData.baseProduct.name, formData.variants]);

  const handleNext = useCallback(() => {
    if (validateStep(activeStep)) {
      handleStepChange(activeStep + 1);
    }
  }, [validateStep, activeStep, handleStepChange]);

  const handleBack = useCallback(() => {
    handleStepChange(activeStep - 1);
  }, [activeStep, handleStepChange]);


  const handleSubmit = async () => {
    if (!validateStep(activeStep)) return;

    setSubmitting(true);
    try {
      // Create base product first
      const baseProductResponse = await productsAPI.createBaseProduct({
        name: formData.baseProduct.name,
        description: formData.baseProduct.description,
        categoryId: formData.baseProduct.categoryId || undefined
      });

      if (baseProductResponse.data.success) {
        const baseProductId = baseProductResponse.data.data.id;

        // Upload base product photos if any
        if (formData.baseProduct.photos.length > 0) {
          await productsAPI.uploadProductVariantPhotos(baseProductId, formData.baseProduct.photos);
        }

        // Create variants
        for (const variant of formData.variants) {
          const variantData = {
            baseProductId,
            name: variant.name,
            stockQuantity: variant.stockQuantity,
            unitOfMeasureId: variant.unitOfMeasureId,
            unitValue: variant.unitValue,
            usageNotes: variant.usageNotes,
            dynamicProperties: {
              ...variant.dynamicProperties,
              ...(variant.size && { size: variant.size }),
              ...(variant.color && { color: variant.color })
            }
          };

          const variantResponse = await productsAPI.createProductVariant(variantData);

          if (variantResponse.data.success && variant.photos.length > 0) {
            const variantId = variantResponse.data.data.id;
            await productsAPI.uploadProductVariantPhotos(variantId, variant.photos);
          }
        }

        showSnackbar('Product created successfully', 'success');
        onComplete?.(baseProductId);
      }
    } catch (error) {
      console.error('Error creating product:', error);
      showSnackbar('Failed to create product', 'error');
    } finally {
      setSubmitting(false);
    }
  };

  const handleFormDataChange = (updates: Partial<ProductWizardFormData>) => {
    setFormData(prev => ({ ...prev, ...updates }));
  };

  const handleExit = () => {
    if (formData.baseProduct.name || formData.variants.some(v => v.name)) {
      setShowExitDialog(true);
    } else {
      handleCancel();
    }
  };

  const handleCancel = () => {
    onCancel?.();
  };

  const handleConfirmExit = () => {
    setShowExitDialog(false);
    handleCancel();
  };

  const renderStepContent = () => {
    return (
      <Suspense fallback={<StepLoadingFallback />}>
        {(() => {
          switch (activeStep) {
            case 0:
              return (
                <ProductWizardStep1
                  formData={formData}
                  onChange={handleFormDataChange}
                  categories={categories}
                  errors={errors}
                />
              );
            case 1:
              return (
                <ProductWizardStep2
                  formData={formData}
                  onChange={handleFormDataChange}
                  unitsOfMeasure={unitsOfMeasure}
                  errors={errors}
                />
              );
            case 2:
              return (
                <ProductWizardStep3
                  formData={formData}
                  categories={categories}
                  unitsOfMeasure={unitsOfMeasure}
                />
              );
            default:
              return null;
          }
        })()}
      </Suspense>
    );
  };

  if (loading) {
    return (
      <Dialog
        open={open}
        maxWidth={isMobile ? false : "md"}
        fullWidth={!isMobile}
        fullScreen={isMobile}
        sx={{
          '& .MuiDialog-paper': {
            width: isMobile ? '100%' : 'auto',
            maxWidth: isMobile ? 'none' : '600px',
            margin: isMobile ? 0 : '16px',
            borderRadius: isMobile ? 0 : '8px',
          }
        }}
      >
        <DialogContent>
          <Box
            display="flex"
            flexDirection="column"
            justifyContent="center"
            alignItems="center"
            p={{ xs: 2, sm: 3, md: 4 }}
            textAlign="center"
          >
            <CircularProgress size={isMobile ? 32 : 40} />
            <Typography
              variant={isMobile ? "body2" : "body1"}
              sx={{
                ml: 2,
                mt: 1,
                fontSize: {
                  xs: '0.875rem',
                  sm: '1rem',
                  md: '1rem'
                }
              }}
            >
              Loading product data...
            </Typography>
          </Box>
        </DialogContent>
      </Dialog>
    );
  }

  return (
    <>
      {/* Skip Links for Accessibility */}
      <a
        ref={skipLinkRef}
        href="#main-content"
        className="skip-link"
        onFocus={() => announceToScreenReader('Skip to main content')}
      >
        Skip to main content
      </a>
      <a
        href="#stepper-navigation"
        className="skip-link"
        onFocus={() => announceToScreenReader('Skip to step navigation')}
      >
        Skip to step navigation
      </a>

      {/* Live Region for Screen Reader Announcements */}
      <div
        aria-live="polite"
        aria-atomic="true"
        className="sr-only"
        role="status"
      >
        {liveRegionMessage}
      </div>

      <Dialog
        ref={dialogRef}
        open={open}
        maxWidth={isMobile ? false : "lg"}
        fullWidth={!isMobile}
        fullScreen={isMobile}
        onClose={handleExit}
        onKeyDown={handleKeyDown}
        className="product-wizard-dialog"
        aria-labelledby="product-wizard-title"
        aria-describedby="product-wizard-description"
        role="dialog"
        aria-modal="true"
        sx={{
          '& .MuiDialog-paper': {
            width: isMobile ? '100%' : '95vw',
            maxWidth: isMobile ? 'none' : '1200px',
            maxHeight: isMobile ? '100%' : '90vh',
            margin: isMobile ? 0 : '16px',
            borderRadius: isMobile ? 0 : '8px',
          }
        }}
      >
        <DialogTitle className="product-wizard-header" id="product-wizard-title">
          <Typography
            variant={isMobile ? "h6" : "h5"}
            component="h1"
            sx={{
              fontSize: {
                xs: '1.1rem',
                sm: '1.25rem',
                md: '1.5rem'
              }
            }}
          >
            {isEdit ? 'Edit Product' : 'Create New Product'}
          </Typography>
          <Typography
            variant={isMobile ? "caption" : "body2"}
            color="text.secondary"
            id="product-wizard-description"
            component="p"
            sx={{
              fontSize: {
                xs: '0.75rem',
                sm: '0.875rem',
                md: '0.875rem'
              }
            }}
          >
            Follow the steps to {isEdit ? 'update' : 'create'} your product and its variants
          </Typography>
        </DialogTitle>

        <DialogContent
          className="product-wizard-content"
          id="main-content"
          ref={mainContentRef}
          role="main"
          aria-labelledby="product-wizard-title"
        >
          {/* Mobile Progress Indicator */}
          {isMobile && (
            <Box sx={{ mb: 2, px: 1 }}>
              <Typography
                variant="body2"
                color="text.secondary"
                sx={{
                  textAlign: 'center',
                  mb: 1,
                  fontSize: '0.8rem',
                  fontWeight: 500
                }}
                aria-live="polite"
              >
                Step {activeStep + 1} of {steps.length}
              </Typography>
              <Box
                sx={{
                  width: '100%',
                  height: '4px',
                  backgroundColor: 'rgba(0, 0, 0, 0.1)',
                  borderRadius: '2px',
                  overflow: 'hidden'
                }}
                role="progressbar"
                aria-valuenow={activeStep + 1}
                aria-valuemin={1}
                aria-valuemax={steps.length}
                aria-label={`Step ${activeStep + 1} of ${steps.length}`}
              >
                <Box
                  sx={{
                    width: `${((activeStep + 1) / steps.length) * 100}%`,
                    height: '100%',
                    backgroundColor: '#1976d2',
                    borderRadius: '2px',
                    transition: 'width 0.3s ease-in-out'
                  }}
                />
              </Box>
            </Box>
          )}

          <Box sx={{ width: '100%', mb: { xs: 2, sm: 3, md: 4 } }}>
            <nav aria-label="Product creation steps" id="stepper-navigation">
              <Stepper
                ref={stepperRef}
                activeStep={activeStep}
                orientation={isMobile ? 'vertical' : 'horizontal'}
                alternativeLabel={!isMobile}
                onKeyDown={handleStepperKeyDown}
                tabIndex={0}
                role="tablist"
                aria-label="Product creation steps"
                sx={{
                  '& .MuiStep-root': {
                    padding: {
                      xs: '8px 0',
                      sm: '16px 0',
                      md: '16px 0'
                    }
                  },
                  '& .MuiStepConnector-root': {
                    marginLeft: isMobile ? '20px' : 'auto',
                    marginRight: isMobile ? '20px' : 'auto'
                  },
                  '& .MuiStepConnector-line': {
                    minHeight: isMobile ? '32px' : 'auto'
                  },
                  '& .MuiStepLabel-root': {
                    padding: {
                      xs: '8px 0',
                      sm: '16px 0',
                      md: '16px 0'
                    }
                  },
                  '& .MuiStepLabel-iconContainer': {
                    paddingRight: {
                      xs: '8px',
                      sm: '16px',
                      md: '16px'
                    }
                  },
                  '& .MuiStepIcon-root': {
                    width: {
                      xs: '32px',
                      sm: '24px',
                      md: '24px'
                    },
                    height: {
                      xs: '32px',
                      sm: '24px',
                      md: '24px'
                    },
                    '&.Mui-active': {
                      width: {
                        xs: '36px',
                        sm: '28px',
                        md: '28px'
                      },
                      height: {
                        xs: '36px',
                        sm: '28px',
                        md: '28px'
                      }
                    }
                  },
                  '& .MuiStepLabel-label': {
                    fontSize: {
                      xs: '0.9rem',
                      sm: '0.875rem',
                      md: '0.875rem'
                    },
                    fontWeight: {
                      xs: 500,
                      sm: 500,
                      md: 500
                    },
                    lineHeight: {
                      xs: 1.3,
                      sm: 1.2,
                      md: 1.2
                    },
                    marginTop: {
                      xs: '4px',
                      sm: '8px',
                      md: '8px'
                    }
                  },
                  '& .MuiStepLabel-label.Mui-active': {
                    fontWeight: {
                      xs: 600,
                      sm: 600,
                      md: 600
                    }
                  },
                  '& .MuiStepLabel-labelContainer': {
                    padding: {
                      xs: '4px 0',
                      sm: '8px 0',
                      md: '8px 0'
                    }
                  }
                }}
              >
                {steps.map((step, index) => (
                  <Step key={step.id}>
                    <StepLabel
                      aria-current={index === activeStep ? 'step' : undefined}
                    >
                      <Typography
                        variant={isMobile ? "body2" : "subtitle2"}
                        sx={{
                          fontSize: {
                            xs: '0.8rem',
                            sm: '0.875rem',
                            md: '0.875rem'
                          }
                        }}
                      >
                        {step.title}
                      </Typography>
                      {!isMobile && (
                        <Typography
                          variant="caption"
                          color="text.secondary"
                          sx={{
                            fontSize: {
                              xs: '0.7rem',
                              sm: '0.75rem',
                              md: '0.75rem'
                            }
                          }}
                        >
                          {step.description}
                        </Typography>
                      )}
                    </StepLabel>
                  </Step>
                ))}
              </Stepper>
            </nav>
          </Box>

          <Box sx={{ mt: 2, mb: 2 }} role="region" aria-label="Current step content">
            {renderStepContent()}
          </Box>
        </DialogContent>

        <DialogActions
          className="product-wizard-actions"
          sx={{
            flexDirection: isMobile ? 'column' : 'row',
            gap: isMobile ? 1.5 : 1,
            padding: {
              xs: '16px',
              sm: '20px 24px',
              md: '20px 24px'
            },
            '& .MuiButton-root': {
              width: isMobile ? '100%' : 'auto',
              minWidth: isMobile ? 'auto' : '120px',
              minHeight: {
                xs: '48px', // Increased from 44px for better touch targets
                sm: '44px',
                md: '44px'
              },
              fontSize: {
                xs: '0.95rem',
                sm: '0.9rem',
                md: '0.9rem'
              },
              padding: {
                xs: '12px 16px',
                sm: '10px 16px',
                md: '10px 16px'
              },
              marginBottom: isMobile ? '8px' : 0,
              borderRadius: '8px',
              boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
              '&:last-child': {
                marginBottom: 0
              },
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
            },
            '& .MuiIconButton-root': {
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
            }
          }}
        >
          <Button
            onClick={handleExit}
            disabled={submitting}
            color="inherit"
            sx={{
              order: isMobile ? 4 : 1,
              marginRight: isMobile ? 0 : 'auto'
            }}
          >
            {isMobile ? 'Exit' : 'Cancel'}
          </Button>

          {!isMobile && <Box sx={{ flex: '1 1 auto' }} />}

          <Button
            disabled={activeStep === 0 || submitting}
            onClick={handleBack}
            startIcon={<ArrowBack />}
            sx={{
              order: isMobile ? 2 : 3
            }}
          >
            {isMobile ? 'Previous' : 'Back'}
          </Button>

          {activeStep === steps.length - 1 ? (
            <Button
              variant="contained"
              onClick={handleSubmit}
              disabled={submitting}
              endIcon={submitting ? <CircularProgress size={16} /> : <Send />}
              sx={{
                order: isMobile ? 1 : 4
              }}
            >
              {submitting ? 'Creating...' : (isMobile ? 'Finish' : 'Create Product')}
            </Button>
          ) : (
            <Button
              variant="contained"
              onClick={handleNext}
              endIcon={<ArrowForward />}
              sx={{
                order: isMobile ? 1 : 4
              }}
            >
              {isMobile ? 'Continue' : 'Next'}
            </Button>
          )}
        </DialogActions>
      </Dialog>

      {/* Exit Confirmation Dialog */}
      <Dialog
        open={showExitDialog}
        onClose={() => setShowExitDialog(false)}
        maxWidth={isMobile ? false : "sm"}
        fullWidth={!isMobile}
        fullScreen={isMobile}
        sx={{
          '& .MuiDialog-paper': {
            width: isMobile ? '100%' : 'auto',
            maxWidth: isMobile ? 'none' : '400px',
            margin: isMobile ? 0 : '16px',
            borderRadius: isMobile ? 0 : '8px',
          }
        }}
      >
        <DialogTitle
          sx={{
            fontSize: {
              xs: '1.1rem',
              sm: '1.25rem',
              md: '1.25rem'
            }
          }}
        >
          Discard Changes?
        </DialogTitle>
        <DialogContent>
          <Typography
            sx={{
              fontSize: {
                xs: '0.875rem',
                sm: '1rem',
                md: '1rem'
              }
            }}
          >
            You have unsaved changes. Are you sure you want to exit without saving?
          </Typography>
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
            onClick={() => setShowExitDialog(false)}
            sx={{ order: isMobile ? 2 : 1 }}
          >
            Continue Editing
          </Button>
          <Button
            onClick={handleConfirmExit}
            color="error"
            sx={{ order: isMobile ? 1 : 2 }}
          >
            Discard Changes
          </Button>
        </DialogActions>
      </Dialog>

      {/* Snackbar for notifications */}
      <Snackbar
        open={snackbar.open}
        autoHideDuration={6000}
        onClose={handleCloseSnackbar}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert onClose={handleCloseSnackbar} severity={snackbar.severity}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </>
  );
};

export default ProductWizard;