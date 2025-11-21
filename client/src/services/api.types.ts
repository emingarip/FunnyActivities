// API Request/Response Types

export {}; // Make this a module

export type ActivityVideoType = 'main' | 'intro';

// User related types
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  profileImageUrl?: string;
  role: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

// Authentication request types
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role?: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

// Authentication response types
export interface AuthResponse {
  success: boolean;
  message: string;
  data: {
    user: User;
    accessToken: string;
    refreshToken: string;
  };
}

export interface ApiError {
  success: boolean;
  message: string;
  error: string;
  status?: number;
}

// User management request types
export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  profileImage?: File;
}

export interface UpdateProfileWithImageRequest extends UpdateProfileRequest {
  profileImage: File;
}

// User management response types
export interface ProfileResponse {
  success: boolean;
  data: User;
}

export interface UpdateProfileResponse {
  success: boolean;
  message: string;
  data: User;
}

// Admin types
export interface UserSearchParams {
  searchTerm?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface UserSearchResponse {
  success: boolean;
  data: {
    users: User[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
  };
}

// Generic API response wrapper
export interface ApiResponse<T = any> {
  success: boolean;
  message?: string;
  data?: T;
  error?: string;
}

// Password reset types
export interface RequestPasswordResetRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

export interface PasswordResetResponse {
  success: boolean;
  message: string;
}

// Role management types
export interface AssignRoleRequest {
  userId: string;
  role: string;
}

export interface RoleResponse {
  success: boolean;
  message: string;
  data?: {
    userId: string;
    role: string;
  };
}

export interface LlmSettings {
  defaultProvider: string;
  defaultModel: string;
  ollamaBaseUrl: string;
  ollamaHealthCheckModel: string;
  ollamaPreferredModels: string[];
  openAiBaseUrl: string;
  openAiDefaultModel: string;
  openAiAllowedModels: string[];
  openAiOrganizationId: string;
  hasOpenAiApiKey: boolean;
  modelCacheSeconds: number;
}

export interface UpdateLlmSettingsPayload {
  defaultProvider: string;
  defaultModel: string;
  ollamaBaseUrl: string;
  ollamaHealthCheckModel: string;
  ollamaPreferredModels: string[];
  openAiBaseUrl: string;
  openAiDefaultModel: string;
  openAiAllowedModels: string[];
  openAiOrganizationId: string;
  openAiApiKey?: string;
  modelCacheSeconds: number;
}

export interface LlmModelInfo {
  name: string;
  displayName: string;
  provider: string;
  isDefault: boolean;
  isAvailable: boolean;
}

export interface ProviderModelsResponse {
  provider: string;
  models: LlmModelInfo[];
}

// Upload types
export interface UploadImageResponse {
  success: boolean;
  message: string;
  data: {
    profileImageUrl: string;
  };
}

export interface UploadActivityVideoResponse {
  success: boolean;
  message: string;
  data: {
    activityId: string;
    videoObjectKey: string;
    videoType: ActivityVideoType;
    signedVideoUrl: string;
    urlExpirySeconds: number;
    uploadedAt: string;
  };
}

// Material related types
export enum MeasurementUnit {
  Pieces = 0,
  Kilograms = 1,
  Liters = 2,
  Meters = 3,
  SquareMeters = 4
}

export interface MaterialListDto {
  id: string;
  name: string;
  category?: string;
  stockQuantity: number;
  unit: string;
  photoCount?: number;
  thumbnailUrl?: string;
}

export interface MaterialPhoto {
  id: string;
  url: string;
  filename: string;
  uploadedAt: string;
}

export interface MaterialDto {
  id: string;
  name: string;
  description?: string;
  stockQuantity: number;
  category?: string;
  unit: string;
  usageNotes?: string;
  photos?: MaterialPhoto[];
  dynamicProperties?: Record<string, any>;
  createdAt: string;
  updatedAt: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// Material request types
export interface CreateMaterialRequest {
  name: string;
  description?: string;
  stockQuantity: number;
  category?: string;
  unit: string;
  usageNotes?: string;
  dynamicProperties?: Record<string, any>;
}

export interface UpdateMaterialRequest {
  name?: string;
  description?: string;
  stockQuantity?: number;
  category?: string;
  unit?: string;
  usageNotes?: string;
  dynamicProperties?: Record<string, any>;
}

// Material query parameters
export interface GetMaterialsParams {
  page?: number;
  pageSize?: number;
  name?: string;
  category?: string;
  minStock?: number;
  maxStock?: number;
  search?: string;
  sortBy?: 'name' | 'stockQuantity';
  sortOrder?: 'asc' | 'desc';
  unit?: string;
  createdFrom?: string;
  createdTo?: string;
  updatedFrom?: string;
  updatedTo?: string;
}


// Materials Statistics and Dashboard Types
export interface MaterialsStats {
  totalMaterials: number;
  lowStockMaterials: number;
  outOfStockMaterials: number;
  totalValue: number;
  categoriesCount: number;
  recentActivities: number;
}

// Chart Data Types
export interface ChartDataPoint {
  label: string;
  value: number;
  color?: string;
}

export interface StockLevelChartData {
  inStock: number;
  lowStock: number;
  outOfStock: number;
  criticalStock: number;
}

export interface CategoryChartData {
  categories: ChartDataPoint[];
}

export interface ActivityChartData {
  labels: string[];
  data: number[];
  period: string;
}

export interface TopMaterialsChartData {
  materials: {
    id: string;
    name: string;
    usageCount: number;
    category?: string;
  }[];
}

// Audit Log Types
export interface AuditLogEntry {
  id: string;
  userId: string;
  userName: string;
  action: 'create' | 'update' | 'delete' | 'view' | 'export';
  entityType: 'material' | 'user' | 'category' | 'system';
  entityId: string;
  entityName: string;
  timestamp: string;
  ipAddress?: string;
  userAgent?: string;
  changes?: AuditLogChange[];
  details?: string;
}

export interface AuditLogChange {
  field: string;
  oldValue?: any;
  newValue?: any;
}

export interface AuditLogFilters {
  userId?: string;
  action?: string;
  entityType?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface AuditLogsResponse {
  items: AuditLogEntry[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface AuditStats {
  totalLogs: number;
  todayLogs: number;
  thisWeekLogs: number;
  thisMonthLogs: number;
  topUsers: {
    userId: string;
    userName: string;
    actionCount: number;
  }[];
  actionBreakdown: {
    action: string;
    count: number;
  }[];
}

// Product related types
export interface BaseProductDto {
  id: string;
  name: string;
  description?: string;
  categoryId?: string;
  categoryName?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ProductVariantDto {
  id: string;
  baseProductId: string;
  baseProductName: string;
  baseProductDescription?: string;
  baseProductCategoryId?: string;
  baseProductCategoryName?: string;
  name: string;
  stockQuantity: number;
  unitOfMeasureId: string;
  unitOfMeasureName: string;
  unitSymbol: string;
  unitValue: number;
  usageNotes?: string;
  photos: string[];
  dynamicProperties: Record<string, any>;
  createdAt: string;
  updatedAt: string;
}

export interface ProductListDto {
  id: string;
  name: string;
  description?: string;
  categoryName?: string;
  basePrice?: number;
  stockStatus: 'in-stock' | 'low-stock' | 'out-of-stock';
  totalVariants: number;
  variants: ProductVariantDto[];
  createdAt: string;
  updatedAt: string;
}

// Unit of Measure types
export interface UnitOfMeasureDto {
  id: string;
  name: string;
  symbol: string;
  type: string;
  createdAt: string;
  updatedAt: string;
}

// Product Wizard Form Types
export interface ProductWizardFormData {
  // Step 1: Base Product Details
  baseProduct: {
    name: string;
    description: string;
    categoryId: string;
    photos: File[];
    dynamicProperties: Record<string, any>;
  };

  // Step 2: Product Variants
  variants: ProductVariantFormData[];

  // Metadata
  isDraft: boolean;
  lastSavedAt?: string;
  currentStep: number;
}

export interface ProductVariantFormData {
  id?: string; // For editing existing variants
  name: string;
  size?: string;
  color?: string;
  stockQuantity: number;
  unitOfMeasureId: string;
  unitValue: number;
  usageNotes: string;
  photos: File[];
  dynamicProperties: Record<string, any>;
  isNew: boolean;
}

// Form validation types
export interface FormValidationErrors {
  [key: string]: string | FormValidationErrors;
}

export interface ProductWizardStep {
  id: number;
  title: string;
  description: string;
  isValid: boolean;
  isCompleted: boolean;
}

// File upload types for products
export interface ProductPhoto {
  id?: string;
  url: string;
  filename: string;
  uploadedAt?: string;
  isNew?: boolean;
  file?: File;
}

// Category types
export interface ProductCategoryDto {
  id: string;
  name: string;
  description?: string;
  createdAt: string;
  updatedAt: string;
}

// Draft management types
export interface ProductDraft {
  id: string;
  formData: ProductWizardFormData;
  createdAt: string;
  updatedAt: string;
  userId: string;
}

// Product query parameters
export interface GetProductsParams {
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
}

// Bulk update types
export interface BulkUpdateProductVariantsRequest {
  updates: ProductVariantUpdateRequest[];
}

export interface ProductVariantUpdateRequest {
  id: string;
  name?: string;
  stockQuantity?: number;
  unitOfMeasureId?: string;
  unitValue?: number;
  usageNotes?: string;
  dynamicProperties?: Record<string, any>;
}

export interface BulkUpdateProductVariantsResponse {
  updatedVariants: ProductVariantDto[];
  errors: BulkUpdateError[];
  totalUpdates: number;
  successfulUpdates: number;
  failedUpdates: number;
}

export interface BulkUpdateError {
  variantId: string;
  errorMessage: string;
  errorType: string;
}

export interface EnhancedStepDto {
  id?: string;
  activityId: string;
  order: number;
  description: string;
  timestampSeconds: number; // Video timestamp in seconds
  createdAt?: string;
  updatedAt?: string;
}

// Enhanced Step API Request Types
export interface CreateEnhancedStepRequest {
  activityId: string;
  order: number;
  description: string;
  timestampSeconds: number;
}

export interface UpdateEnhancedStepRequest {
  order?: number;
  description?: string;
  timestampSeconds?: number;
}

// Video Player and Timeline Types
export interface VideoPlayerState {
  currentTime: number;
  duration: number;
  isPlaying: boolean;
  isPaused: boolean;
  volume: number;
  playbackRate: number;
  buffered: TimeRanges;
}

export interface TimelineMarker {
  id: string;
  time: number; // in seconds
  type: 'step' | 'pause' | 'highlight';
  label: string;
  color?: string;
  data?: any;
}

export interface VideoScrubberProps {
  duration: number;
  currentTime: number;
  markers: TimelineMarker[];
  onTimeChange: (time: number) => void;
  onMarkerClick: (marker: TimelineMarker) => void;
  onMarkerAdd: (time: number) => void;
}

// Undo/Redo Types
export interface StepHistoryAction {
  type: 'create' | 'update' | 'delete' | 'reorder';
  stepId?: string;
  previousState?: any;
  newState?: any;
  timestamp: number;
}

export interface StepHistoryState {
  past: StepHistoryAction[];
  present: EnhancedStepDto[];
  future: StepHistoryAction[];
}
