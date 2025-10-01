import axios, { AxiosInstance, AxiosResponse, AxiosError, InternalAxiosRequestConfig } from 'axios';
import { BulkUpdateProductVariantsRequest } from './api.types';

// API Base URL - pointing to .NET WebAPI
const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:8080/api';

// Logout callback - will be set by AuthContext
let logoutCallback: (() => void) | null = null;

// Function to set logout callback from AuthContext
export const setLogoutCallback = (callback: () => void) => {
  logoutCallback = callback;
};

// Create axios instance with optimized settings
const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
  // Optimize for performance
  maxRedirects: 5,
  maxContentLength: 50 * 1024 * 1024, // 50MB for video uploads
});

// Request interceptor for adding auth token
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig): InternalAxiosRequestConfig => {
    // Show loading state
    if (typeof window !== 'undefined') {
      // You can implement a global loading state here
      // For now, we'll use a simple approach
      const loadingElement = document.getElementById('global-loading');
      if (loadingElement) {
        loadingElement.style.display = 'block';
      }
    }

    // DEBUG: Log the actual request being made
    console.log('🚀 API Request:', {
      method: config.method?.toUpperCase(),
      url: config.url,
      fullUrl: `${config.baseURL}${config.url}`,
      headers: config.headers,
      params: config.params,
      origin: window.location.origin,
      timestamp: new Date().toISOString()
    });

    // Add auth token if available, but skip for anonymous endpoints
    const token = localStorage.getItem('accessToken');
    if (token && config.headers) {
      // Skip adding Authorization header for anonymous endpoints
      const anonymousEndpoints = [
        '/auth/register',
        '/auth/login',
        '/auth/refresh',
        '/users/request-password-reset',
        '/activities/public',
        '/surveys/public',
        '/surveys/activities',
        '/surveys/status',
        '/surveys/share'
      ];

      const isAnonymousEndpoint = anonymousEndpoints.some(endpoint =>
        config.url?.includes(endpoint)
      );

      if (!isAnonymousEndpoint) {
        config.headers.Authorization = `Bearer ${token}`;
        console.log('🔐 Auth token added to request');
      } else {
        console.log('🔓 Anonymous endpoint - no auth token added');
      }
    } else {
      console.log('⚠️  No auth token available');
    }

    return config;
  },
  (error: AxiosError) => {
    // Hide loading state on error
    if (typeof window !== 'undefined') {
      const loadingElement = document.getElementById('global-loading');
      if (loadingElement) {
        loadingElement.style.display = 'none';
      }
    }
    return Promise.reject(error);
  }
);

// Helper function to detect if response data is HTML
const isHtmlResponse = (data: any): boolean => {
  if (typeof data !== 'string') return false;
  const trimmed = data.trim().toLowerCase();
  return trimmed.startsWith('<!doctype html') ||
         trimmed.startsWith('<html') ||
         (trimmed.includes('<html') && trimmed.includes('</html>'));
};

// Response interceptor for handling errors and token refresh
api.interceptors.response.use(
  (response: AxiosResponse) => {
    // DEBUG: Log successful response
    console.log('✅ API Response:', {
      status: response.status,
      url: response.config.url,
      method: response.config.method?.toUpperCase(),
      data: response.data,
      contentType: response.headers['content-type']
    });

    // Hide loading state on success
    if (typeof window !== 'undefined') {
      const loadingElement = document.getElementById('global-loading');
      if (loadingElement) {
        loadingElement.style.display = 'none';
      }
    }

    // Check if response is HTML when JSON was expected
    const contentType = response.headers['content-type']?.toLowerCase();
    if (contentType && !contentType.includes('application/json') && isHtmlResponse(response.data)) {
      console.error('🚨 HTML response received when JSON expected:', {
        url: response.config.url,
        status: response.status,
        contentType,
        data: response.data.substring(0, 200) + '...'
      });

      // Create an error for HTML responses
      const error = new Error('Server returned HTML instead of JSON. This may indicate a server error or misconfiguration.');
      (error as any).response = response;
      (error as any).isHtmlResponse = true;
      (error as any).type = 'ServerError';
      throw error;
    }

    // Check for standardized API response format
    if (response.data && typeof response.data.success === 'boolean') {
      if (!response.data.success) {
        // Create an error for unsuccessful responses
        const error = new Error(response.data.message || 'API request failed');
        (error as any).response = response;
        (error as any).isApiError = true;
        throw error;
      }
    }

    return response;
  },
  async (error: AxiosError) => {
    // DEBUG: Log error details
    console.error('❌ API Error:', {
      message: error.message,
      status: error.response?.status,
      statusText: error.response?.statusText,
      url: error.config?.url,
      method: error.config?.method?.toUpperCase(),
      headers: error.config?.headers,
      isNetworkError: !error.response,
      isCorsError: error.message?.includes('CORS') || error.message?.includes('Network Error')
    });

    // Hide loading state on error
    if (typeof window !== 'undefined') {
      const loadingElement = document.getElementById('global-loading');
      if (loadingElement) {
        loadingElement.style.display = 'none';
      }
    }

    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    // Handle 401 Unauthorized
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        // Try to refresh token
        const refreshToken = localStorage.getItem('refreshToken');
        const accessToken = localStorage.getItem('accessToken');
        if (refreshToken && accessToken) {
          const response = await axios.post(`${API_BASE_URL}/auth/refresh`, {
            refreshToken,
            accessToken
          });

          // Handle new response format from AuthController
          const responseData = response.data;
          if (responseData.success && responseData.data) {
            const { accessToken, refreshToken: newRefreshToken, user } = responseData.data;

            // Update tokens in localStorage
            localStorage.setItem('accessToken', accessToken);
            localStorage.setItem('refreshToken', newRefreshToken);

            // Update user data in AuthContext if available
            if (user && logoutCallback) {
              // We need to update the user state, but since we don't have direct access
              // to the AuthContext here, we'll trigger a re-check by calling logoutCallback
              // and then re-setting the auth state. This is a bit of a hack, but works.
              // TODO: Consider using a more elegant solution like a global auth state manager
            }

            // Retry original request with new token
            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${accessToken}`;
            }

            return api(originalRequest);
          }
        }
      } catch (refreshError) {
        // Refresh failed, clear tokens and logout
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');

        // Use logout callback if available, otherwise redirect
        if (logoutCallback) {
          logoutCallback();
        } else {
          window.location.href = '/login';
        }

        return Promise.reject(refreshError);
      }
    }

    // Handle other errors
    let errorMessage = 'An unexpected error occurred';
    let errorType = 'UnknownError';

    if (error.response?.data) {
      const errorData = error.response.data as any;

      // Check if response data is HTML (server error pages)
      if (isHtmlResponse(errorData)) {
        console.error('🚨 HTML error response received:', {
          status: error.response.status,
          url: error.config?.url,
          htmlSnippet: errorData.substring(0, 200) + '...'
        });

        errorMessage = `Server error: The server returned an HTML error page instead of JSON. Status: ${error.response.status}`;
        errorType = 'ServerError';
      } else {
        // Handle standardized API error format
        if (typeof errorData.success === 'boolean' && !errorData.success) {
          errorMessage = errorData.message || errorMessage;
          errorType = errorData.error || 'ApiError';
        } else if (errorData.message) {
          // Handle legacy error format or other error structures
          errorMessage = errorData.message || errorMessage;
          errorType = errorData.error || errorType;
        }
      }
    } else if (error.message) {
      errorMessage = error.message;
    }

    // Handle specific HTTP status codes
    if (error.response?.status) {
      switch (error.response.status) {
        case 400:
          errorType = 'BadRequest';
          break;
        case 401:
          errorType = 'Unauthorized';
          errorMessage = 'Authentication required or token expired';
          break;
        case 403:
          errorType = 'Forbidden';
          errorMessage = 'Access denied';
          break;
        case 404:
          errorType = 'NotFound';
          errorMessage = 'Resource not found';
          break;
        case 409:
          errorType = 'Conflict';
          errorMessage = 'Resource conflict';
          break;
        case 422:
          errorType = 'ValidationError';
          break;
        case 429:
          errorType = 'RateLimitExceeded';
          errorMessage = 'Too many requests, please try again later';
          break;
        case 500:
          errorType = 'InternalServerError';
          errorMessage = 'Server error, please try again later';
          break;
        case 503:
          errorType = 'ServiceUnavailable';
          errorMessage = 'Service temporarily unavailable';
          break;
        default:
          errorType = 'HttpError';
      }
    }

    // Log error for debugging
    console.error('API Error:', {
      message: errorMessage,
      type: errorType,
      status: error.response?.status,
      url: error.config?.url,
      method: error.config?.method
    });

    // Create enhanced error object
    const status = error.response?.status;
    const enhancedError = {
      ...error,
      message: errorMessage,
      type: errorType,
      status,
      isAuthError: status === 401,
      isNetworkError: !error.response,
      isServerError: status ? status >= 500 : false
    };

    return Promise.reject(enhancedError);
  }
);

// API methods - Updated for .NET WebAPI endpoints
export const authAPI = {
  register: (data: { email: string; password: string; firstName: string; lastName: string; role?: string }) =>
    api.post('/auth/register', data),

  login: (data: { email: string; password: string }) =>
    api.post('/auth/login', data),

  refreshToken: (data: { refreshToken: string }) =>
    api.post('/auth/refresh', data),

  logout: () =>
    api.post('/auth/logout'),

  requestPasswordReset: (data: { email: string }) =>
    api.post('/users/request-password-reset', data),
};

export const userAPI = {
  getProfile: () => api.get('/users/profile'),

  updateProfile: (data: { firstName: string; lastName: string }) =>
    api.put('/users/profile', data),

  // Note: .NET API doesn't have a dedicated dashboard endpoint
  // We'll use profile data for now
  getDashboard: () => api.get('/users/profile'),
};

export const adminAPI = {
  // Note: .NET API doesn't have admin dashboard endpoint in the controllers I saw
  // getDashboard: () => api.get('/admin/dashboard'),

  getAllUsers: (params?: { searchTerm?: string; page?: number; pageSize?: number; sortBy?: string; sortOrder?: string }) =>
    api.get('/users/search', { params }),

  getUserById: (id: string) => api.get(`/users/${id}`),

  // Note: .NET API doesn't have direct user update endpoint for admins
  // Admin functionality is through role management
  // updateUser: (id: string, data: { firstName?: string; lastName?: string; role?: string; isActive?: boolean }) =>
  //   api.put(`/admin/users/${id}`, data),

  // deleteUser: (id: string) => api.delete(`/admin/users/${id}`),

  // Role management endpoints
  assignRole: (data: { userId: string; role: string }) =>
    api.post('/roles/assign', data),

  getUserRole: (userId: string) => api.get(`/roles/user/${userId}`),

  // User growth endpoint
  getUserGrowth: (params?: { period?: string; days?: number }) =>
    api.get('/users/admin/growth', { params }),
};

export const materialsAPI = {
  // Get paginated list of materials with filtering and sorting
  getMaterials: (params?: {
    page?: number;
    pageSize?: number;
    name?: string;
    category?: string;
    minStock?: number;
    maxStock?: number;
    search?: string;
    sortBy?: 'name' | 'stockQuantity';
    sortOrder?: 'asc' | 'desc';
  }) => api.get('/materials', { params }),

  // Get single material by ID
  getMaterial: (id: string) => api.get(`/materials/${id}`),

  // Create new material
  createMaterial: (data: {
    name: string;
    description?: string;
    stockQuantity: number;
    category?: string;
    unit: string;
    usageNotes?: string;
    dynamicProperties?: Record<string, any>;
  }) => {
    // Ensure unit is properly formatted as string
    const formattedData = {
      ...data,
      unit: String(data.unit).trim(),
      stockQuantity: Number(data.stockQuantity)
    };

    console.log('🔧 Frontend: Creating material with payload:', formattedData);
    return api.post('/materials', formattedData);
  },

  // Create new material with corrected payload structure
  createMaterialCorrected: (data: {
    name: string;
    description?: string;
    stockQuantity: number;
    category?: string;
    unit: string;
    usageNotes?: string;
    dynamicProperties?: Record<string, any>;
  }) => {
    // Ensure unit is properly formatted as string
    const formattedData = {
      ...data,
      unit: String(data.unit).trim(),
      stockQuantity: Number(data.stockQuantity)
    };

    console.log('🔧 Frontend: Creating corrected material with payload:', formattedData);
    return api.post('/materials', formattedData);
  },

  // Update existing material
  updateMaterial: (id: string, data: {
    name?: string;
    description?: string;
    stockQuantity?: number;
    category?: string;
    unit?: string;
    usageNotes?: string;
    dynamicProperties?: Record<string, any>;
  }) => {
    // Ensure unit is properly formatted if provided
    const formattedData = {
      ...data,
      ...(data.unit && { unit: String(data.unit).trim() }),
      ...(data.stockQuantity && { stockQuantity: Number(data.stockQuantity) })
    };

    console.log('🔧 Frontend: Updating material with payload:', formattedData);
    return api.put(`/materials/${id}`, formattedData);
  },

  // Delete material
  deleteMaterial: (id: string) => api.delete(`/materials/${id}`),

  // Bulk create materials
  bulkCreateMaterials: (data: { materials: any[] }) =>
    api.post('/materials/bulk', data),

  // Bulk update materials
  bulkUpdateMaterials: (data: { updates: { id: string; updateData: any }[] }) =>
    api.put('/materials/bulk', data),

  // Get materials statistics for dashboard
  getMaterialsStats: () => api.get('/materials/stats'),

  // Get chart data for visualizations
  getStockLevelChartData: () => api.get('/materials/charts/stock-levels'),
  getCategoryChartData: () => api.get('/materials/charts/categories'),
  getActivityChartData: (params?: { days?: number }) => api.get('/materials/charts/activity', { params }),
  getTopMaterialsChartData: (params?: { limit?: number }) => api.get('/materials/charts/top-materials', { params }),

  // Upload photos for a material
  uploadMaterialPhotos: (id: string, photos: File[]) => {
    const formData = new FormData();
    photos.forEach((photo, index) => {
      formData.append('photos', photo);
    });
    return api.post(`/materials/${id}/photos`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },

  // Get all photos for a material
  getMaterialPhotos: (materialId: string) =>
    api.get(`/materials/${materialId}/photos`),

  // Download a specific photo for a material
  downloadMaterialPhoto: (materialId: string, photoId: string) =>
    api.get(`/materials/${materialId}/photos/${photoId}`, {
      responseType: 'blob'
    }),

  // Delete a specific photo for a material
  deleteMaterialPhoto: (materialId: string, photoId: string) =>
    api.delete(`/materials/${materialId}/photos/${photoId}`),
};

export const productsAPI = {
  // Get paginated list of products with filtering and sorting
  getProducts: (params?: {
    page?: number;
    pageSize?: number;
    searchTerm?: string;
    categoryId?: string;
    sortBy?: 'name' | 'createdAt' | 'updatedAt';
    sortOrder?: 'asc' | 'desc';
    minPrice?: number;
    maxPrice?: number;
    stockStatus?: 'in-stock' | 'low-stock' | 'out-of-stock';
    createdFrom?: string;
    createdTo?: string;
    updatedFrom?: string;
    updatedTo?: string;
  }) => api.get('/products', { params }),

  // Get single product by ID
  getProduct: (id: string) => api.get(`/products/${id}`),

  // Get single base product by ID
  getBaseProduct: (id: string) => api.get(`/products/base/${id}`),

  // Create new base product
  createBaseProduct: (data: {
    name: string;
    description?: string;
    categoryId?: string;
  }) => api.post('/products/base', data),

  // Create new product (as per task requirement)
  createProduct: (data: {
    name: string;
    description?: string;
    price?: number;
    categoryId?: string;
    image?: File;
  }) => {
    const formData = new FormData();
    formData.append('name', data.name);
    if (data.description) formData.append('description', data.description);
    if (data.price) formData.append('price', data.price.toString());
    if (data.categoryId) formData.append('categoryId', data.categoryId);
    if (data.image) formData.append('image', data.image);

    return api.post('/products', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },

  // Update base product
  updateBaseProduct: (id: string, data: {
    name?: string;
    description?: string;
    categoryId?: string;
  }) => api.put(`/products/base/${id}`, data),

  // Delete base product
  deleteBaseProduct: (id: string) => {
    console.log('🗑️ Frontend: Calling deleteBaseProduct with ID:', id);
    console.log('🗑️ Frontend: Sending cascadeDeleteVariants=true as query parameter');
    return api.delete(`/products/base/${id}?cascadeDeleteVariants=true`);
  },

  // Get product variants for a base product or all variants if no baseProductId
  getProductVariants: (baseProductId?: string, params?: {
    page?: number;
    pageSize?: number;
  }) => {
    if (baseProductId) {
      return api.get(`/products/${baseProductId}/variants`, { params });
    } else {
      return api.get('/products/variants', { params });
    }
  },

  // Create product variant
  createProductVariant: (data: {
    baseProductId: string;
    name: string;
    stockQuantity: number;
    unitOfMeasureId: string;
    unitValue: number;
    usageNotes?: string;
    dynamicProperties?: Record<string, any>;
  }) => api.post('/products/variants', data),

  // Update product variant
  updateProductVariant: (id: string, data: {
    name?: string;
    stockQuantity?: number;
    unitOfMeasureId?: string;
    unitValue?: number;
    usageNotes?: string;
    dynamicProperties?: Record<string, any>;
  }) => api.put(`/products/variants/${id}`, data),

  // Delete product variant
  deleteProductVariant: (id: string) => api.delete(`/products/variants/${id}`),

  // Get categories for products
  getProductCategories: () => api.get('/categories'),

  // Upload photos for a product variant
  uploadProductVariantPhotos: (variantId: string, photos: File[]) => {
    const formData = new FormData();
    photos.forEach((photo, index) => {
      formData.append('photos', photo);
    });
    return api.post(`/products/variants/${variantId}/photos`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },

  // Get all photos for a product variant
  getProductVariantPhotos: (variantId: string) =>
    api.get(`/products/variants/${variantId}/photos`),

  // Delete a specific photo for a product variant
  deleteProductVariantPhoto: (variantId: string, photoId: string) =>
    api.delete(`/products/variants/${variantId}/photos/${photoId}`),

  // Get all units of measure
  getUnitsOfMeasure: () => api.get('/units-of-measure'),

  // Get single unit of measure by ID
  getUnitOfMeasure: (id: string) => api.get(`/units-of-measure/${id}`),

  // Get single product variant by ID
  getProductVariant: (id: string) => api.get(`/products/variants/${id}`),

  // Create new unit of measure
  createUnitOfMeasure: (data: { name: string; symbol: string; type?: string }) =>
    api.post('/units-of-measure', data),

  // Create product category
  createProductCategory: (data: { name: string; description?: string }) =>
    api.post('/products/categories', data),

  // Create category (as per task requirement)
  createCategory: (data: { name: string; description?: string }) =>
    api.post('/categories', data),

  // Draft management
  saveProductDraft: (data: { formData: any; draftId?: string }) =>
    api.post('/products/drafts', data),

  getProductDrafts: () => api.get('/products/drafts'),

  getProductDraft: (id: string) => api.get(`/products/drafts/${id}`),

  deleteProductDraft: (id: string) => api.delete(`/products/drafts/${id}`),
  // Bulk update product variants
  bulkUpdateProductVariants: (data: BulkUpdateProductVariantsRequest) =>
    api.put('/products/variants/bulk', data),
};

export const activitiesAPI = {
  // Get paginated list of activities with optional filtering
  getActivities: (params?: {
    pageNumber?: number;
    pageSize?: number;
    searchTerm?: string;
    activityCategoryId?: string;
    sortBy?: string;
    sortOrder?: string;
  }) => api.get('/activities', { params }),

  // Get single activity by ID
  getActivity: (id: string) => api.get(`/activities/${id}`),

  // Get activity with all details (steps, product variants)
  getActivityWithDetails: (id: string) => api.get(`/activities/${id}/with-details`),

  // Create new activity
  createActivity: (data: {
    name: string;
    description?: string;
    videoUrl?: string;
    durationHours?: number;
    durationMinutes?: number;
    durationSeconds?: number;
    activityCategoryId?: string;
  }) => api.post('/activities', data),

  // Update existing activity
  updateActivity: (id: string, data: {
    name?: string;
    description?: string;
    videoUrl?: string;
    durationHours?: number;
    durationMinutes?: number;
    durationSeconds?: number;
    isPublic?: boolean;
  }) => api.put(`/activities/${id}`, data),

  // Delete activity
  deleteActivity: (id: string) => api.delete(`/activities/${id}`),

  // Upload video for activity
  uploadActivityVideo: (activityId: string, videoData: File) => {
    const formData = new FormData();
    formData.append('videoData', videoData);
    formData.append('fileName', videoData.name);
    formData.append('contentType', videoData.type);
    formData.append('activityId', activityId);

    return api.post(`/activities/${activityId}/upload-video`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },

  // Get signed URL for activity video (authenticated)
  getActivityVideoUrl: (activityId: string, videoObjectKey: string, expirySeconds?: number) =>
    api.get(`/activities/${activityId}/video-url`, {
      params: { videoObjectKey, expirySeconds }
    }),

  // Get public video URL for public activities (no authentication required)
  getPublicActivityVideoUrl: (activityId: string, videoObjectKey: string, expirySeconds?: number) =>
    api.get(`/activities/public/${activityId}/video-url`, {
      params: { videoObjectKey, expirySeconds }
    }),

  // Delete activity video
  deleteActivityVideo: (activityId: string, videoObjectKey: string) =>
    api.delete(`/activities/${activityId}/video`, { params: { videoObjectKey } }),

  // Get public activities (no authentication required)
  getPublicActivities: (params?: {
    pageNumber?: number;
    pageSize?: number;
    searchTerm?: string;
    activityCategoryId?: string;
    sortBy?: string;
    sortOrder?: string;
  }) => api.get('/activities/public', { params }),

  // Get single public activity by ID (no authentication required)
  getPublicActivity: (id: string) => {
    // Use the configured api instance but skip authentication for public endpoints
    return api.get(`/activities/public/${id}`);
  },

  // Get video metadata using GET-based methods (no authentication required)
  getVideoMetadata: (videoObjectKey: string) =>
    api.get('/activities/video-metadata', { params: { videoObjectKey } }),

  // Get object metadata using GET-based methods (no authentication required)
  getObjectMetadata: (objectKey: string) =>
    api.get('/activities/object-metadata', { params: { objectKey } }),
};

export const activityCategoriesAPI = {
  // Get paginated list of activity categories
  getActivityCategories: (params?: {
    pageNumber?: number;
    pageSize?: number;
    searchTerm?: string;
    sortBy?: string;
    sortOrder?: string;
  }) => api.get('/activity-categories', { params }),

  // Get single activity category by ID
  getActivityCategory: (id: string) => api.get(`/activity-categories/${id}`),

  // Create new activity category
  createActivityCategory: (data: {
    name: string;
    description?: string;
  }) => api.post('/activity-categories', data),

  // Update existing activity category
  updateActivityCategory: (id: string, data: {
    name?: string;
    description?: string;
  }) => api.put(`/activity-categories/${id}`, data),

  // Delete activity category
  deleteActivityCategory: (id: string) => api.delete(`/activity-categories/${id}`),
};

export const stepsAPI = {
  // Get single step by ID
  getStep: (id: string) => api.get(`/steps/${id}`),

  // Get all steps for an activity
  getStepsByActivityId: (activityId: string) => api.get(`/steps/by-activity/${activityId}`),

  // Create new step
  createStep: (data: {
    activityId: string;
    order: number;
    description: string;
    mediaAttachments?: string[];
  }) => {
    const payload = {
      ...data,
      mediaAttachments: data.mediaAttachments || []
    };

    console.log('🔧 Frontend: Creating step with payload:', payload);
    return api.post('/steps', payload);
  },

  // Update existing step
  updateStep: (id: string, data: {
    order?: number;
    description?: string;
  }) => api.put(`/steps/${id}`, data),

  // Delete step
  deleteStep: (id: string) => api.delete(`/steps/${id}`),

  // Enhanced step methods with timestamps and media attachments
  createEnhancedStep: (data: {
    activityId: string;
    order: number;
    description: string;
    timestampSeconds?: number;
    durationSeconds?: number;
    pauseTimeSeconds?: number;
    mediaAttachments?: string[];
  }) => {
    const payload = {
      ...data,
      mediaAttachments: data.mediaAttachments || []
    };

    console.log('🔧 Frontend: Creating enhanced step with payload:', payload);
    return api.post('/steps', payload);
  },

  updateEnhancedStep: (id: string, data: {
    order?: number;
    description?: string;
    timestampSeconds?: number;
    durationSeconds?: number;
    pauseTimeSeconds?: number;
    mediaAttachments?: any[];
  }) => {
    const payload = data;
    return api.put(`/steps/${id}`, payload);
  },

  // Upload media attachments for steps
  uploadStepMedia: (stepId: string, mediaFiles: File[]) => {
    const formData = new FormData();
    mediaFiles.forEach((file, index) => {
      formData.append('mediaFiles', file);
    });
    return api.post(`/steps/${stepId}/media`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },

  // Delete media attachment from step
  deleteStepMedia: (stepId: string, mediaId: string) =>
    api.delete(`/steps/${stepId}/media/${mediaId}`),
};

export const activityProductVariantsAPI = {
  // Get single activity product variant by ID
  getActivityProductVariant: (id: string) => api.get(`/activity-product-variants/${id}`),

  // Get all activity product variants for an activity
  getActivityProductVariantsByActivityId: (activityId: string) =>
    api.get(`/activity-product-variants/by-activity/${activityId}`),

  // Create new activity product variant
  createActivityProductVariant: (data: {
    activityId: string;
    productVariantId: string;
    quantity: number;
    unitOfMeasureId: string;
  }) => api.post('/activity-product-variants', data),

  // Update existing activity product variant
  updateActivityProductVariant: (id: string, data: {
    quantity?: number;
    unitOfMeasureId?: string;
  }) => api.put(`/activity-product-variants/${id}`, data),

  // Delete activity product variant
  deleteActivityProductVariant: (id: string) => api.delete(`/activity-product-variants/${id}`),
};

export const auditAPI = {
  // Get audit logs with filtering and pagination
  getAuditLogs: (params?: {
    page?: number;
    pageSize?: number;
    userId?: string;
    action?: string;
    entityType?: string;
    startDate?: string;
    endDate?: string;
    sortBy?: string;
    sortOrder?: 'asc' | 'desc';
  }) => api.get('/audit/logs', { params }),

  // Get audit log details by ID
  getAuditLogDetails: (id: string) => api.get(`/audit/logs/${id}`),

  // Export audit logs
  exportAuditLogs: (params?: {
    userId?: string;
    action?: string;
    entityType?: string;
    startDate?: string;
    endDate?: string;
    format?: 'csv' | 'pdf';
  }) => api.get('/audit/logs/export', { params, responseType: 'blob' }),

  // Get audit statistics
  getAuditStats: () => api.get('/audit/stats'),
};

export const favoritesAPI = {
  // Add activity to user's favorites
  addToFavorites: (activityId: string) =>
    api.post(`/favorites/${activityId}`),

  // Remove activity from user's favorites
  removeFromFavorites: (activityId: string) =>
    api.delete(`/favorites/${activityId}`),

  // Get all user's favorite activities
  getUserFavorites: () =>
    api.get('/favorites'),

  // Check if activity is in user's favorites
  checkFavorite: (activityId: string) =>
    api.get(`/favorites/${activityId}/check`),
};

export const surveyAPI = {
  // Admin operations (require authentication)

  // Get paginated list of surveys with filtering and sorting
  getSurveys: (params?: {
    pageNumber?: number;
    pageSize?: number;
    searchTerm?: string;
    status?: string;
    sortBy?: string;
    sortOrder?: 'asc' | 'desc';
  }) => api.get('/surveys', { params }),

  // Get single survey by ID
  getSurvey: (id: string) => api.get(`/surveys/${id}`),

  // Create new survey
  createSurvey: (data: {
    title: string;
    description?: string;
    startDate: string;
    endDate?: string;
    maxParticipants?: number;
    activityIds: string[];
  }) => api.post('/surveys', data),

  // Update existing survey
  updateSurvey: (id: string, data: {
    title?: string;
    description?: string;
    startDate?: string;
    endDate?: string;
    maxParticipants?: number;
    activityIds?: string[];
  }) => api.put(`/surveys/${id}`, data),

  // Delete survey
  deleteSurvey: (id: string) => api.delete(`/surveys/${id}`),

  // Get survey results with detailed statistics
  getSurveyResults: (id: string) => api.get(`/surveys/${id}/results`),


  // Public operations (no authentication required)

  // Get public survey by ID
  getPublicSurvey: (surveyId: string) => {
    // Use configured api instance (interceptor will skip auth for public endpoints)
    return api.get(`/surveys/${surveyId}/public`);
  },

  // Get survey activities for public voting
  getSurveyActivities: (surveyId: string) => {
    // Use configured api instance (interceptor will skip auth for public endpoints)
    return api.get(`/surveys/${surveyId}/activities`);
  },

  // Get survey status
  getSurveyStatus: (surveyId: string) => {
    // Use configured api instance (interceptor will skip auth for public endpoints)
    return api.get(`/surveys/${surveyId}/status`);
  },

  // Submit vote for survey activity
  submitVote: (surveyId: string, data: {
    surveyActivityId: string;
    voteValue: number;
  }) => {
    // Use direct axios call to avoid authentication for public endpoints
    const axios = require('axios');
    const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:8080/api';
    return axios.post(`${API_BASE_URL}/surveys/${surveyId}/vote`, data);
  },

  // Get public survey by share token
  getPublicSurveyByShareToken: (shareToken: string) => {
    // Use configured api instance (interceptor will skip auth for public endpoints)
    return api.get(`/surveys/share/${shareToken}`);
  },

  // Get share URL for a survey (authenticated endpoint)
  getShareUrl: (surveyId: string) => api.get(`/surveys/${surveyId}/share-url`),
};

export default api;