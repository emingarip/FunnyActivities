import React, { useState, useEffect, useCallback, useRef } from 'react';
import { ProductListDto, GetProductsParams } from '../../services/api.types';
import { productsAPI } from '../../services/api';
import ProductTable from './ProductTable';
import ProductFilterPanel, { ProductFilters } from './ProductFilterPanel';
import MaterialsPagination from '../materials/MaterialsPagination';
import MaterialsLoading from '../materials/MaterialsLoading';
import MaterialsEmpty from '../materials/MaterialsEmpty';
import './ProductList.css';

/**
 * Props for the ProductList component
 */
interface ProductListProps {
  /** Callback when a product is selected */
  onProductSelect?: (product: ProductListDto) => void;
  /** Callback when a product is edited */
  onProductEdit?: (product: ProductListDto) => void;
  /** Callback when a product is deleted */
  onProductDelete?: (product: ProductListDto) => void;
  /** Callback when a product variant is viewed */
  onVariantView?: (variantId: string) => void;
  /** Whether to show action buttons */
  showActions?: boolean;
  /** Trigger to refresh the product list */
  refreshTrigger?: number;
}

/**
 * Main ProductList component for displaying products with their variants
 */
const ProductList: React.FC<ProductListProps> = React.memo(({
  onProductSelect,
  onProductEdit,
  onProductDelete,
  onVariantView,
  showActions = true,
  refreshTrigger
}) => {
  // State for products data
  const [products, setProducts] = useState<ProductListDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Mobile interaction state
  const [touchStart, setTouchStart] = useState<number | null>(null);
  const [touchEnd, setTouchEnd] = useState<number | null>(null);
  const [swipedProductId, setSwipedProductId] = useState<string | null>(null);

  // Pagination state
  const [pagination, setPagination] = useState({
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0
  });

  // Filters state
  const [filters, setFilters] = useState<GetProductsParams>({
    page: 1,
    pageSize: 10,
    sortBy: 'name',
    sortOrder: 'asc',
    searchTerm: '',
    categoryId: '',
    minPrice: undefined,
    maxPrice: undefined,
    stockStatus: undefined,
    createdFrom: undefined,
    createdTo: undefined,
    updatedFrom: undefined,
    updatedTo: undefined
  });

  // Filter panel state
  const [isFilterPanelOpen, setIsFilterPanelOpen] = useState(false);
  const [categories, setCategories] = useState<{ id: string; name: string }[]>([]);
  const [categoriesLoading, setCategoriesLoading] = useState(false);

  // Lazy loading state
  const [hasMore, setHasMore] = useState(true);
  const [isLoadingMore, setIsLoadingMore] = useState(false);
  const observerRef = useRef<IntersectionObserver | null>(null);
  const sentinelRef = useRef<HTMLDivElement | null>(null);

  // Load products function
  const loadProducts = useCallback(async () => {
    setLoading(true);
    setError(null);
    setHasMore(true); // Reset lazy loading state
    setIsLoadingMore(false);

    try {
      const response = await productsAPI.getProducts(filters);
      const responseData = response.data;

      if (responseData.success && responseData.data) {
        setProducts(responseData.data.items || []);
        setPagination({
          page: responseData.data.page || 1,
          pageSize: responseData.data.pageSize || 10,
          totalCount: responseData.data.totalCount || 0,
          totalPages: responseData.data.totalPages || 1
        });
        setHasMore(responseData.data.hasNextPage || false);
      } else {
        // Fallback to empty data if API response format is unexpected
        setProducts([]);
        setPagination({
          page: 1,
          pageSize: 10,
          totalCount: 0,
          totalPages: 1
        });
        setHasMore(false);
      }
    } catch (err: any) {
      console.error('Error loading products:', err);
      const errorMessage = getErrorMessage(err, 'load products');
      setError(errorMessage);

      // Fallback to empty state on error
      setProducts([]);
      setPagination({
        page: 1,
        pageSize: 10,
        totalCount: 0,
        totalPages: 1
      });
      setHasMore(false);

      // Show error notification
      showErrorMessage(errorMessage);
    } finally {
      setLoading(false);
    }
  }, [filters]);

  // Load categories on mount
  useEffect(() => {
    const loadCategories = async () => {
      setCategoriesLoading(true);
      try {
        const response = await productsAPI.getProductCategories();
        if (response.data.success && response.data.data) {
          setCategories(response.data.data);
        }
      } catch (err: any) {
        console.error('Error loading categories:', err);
        // Fallback to empty categories
        setCategories([]);
      } finally {
        setCategoriesLoading(false);
      }
    };

    loadCategories();
  }, []);

  // Load products on mount and when filters change
  useEffect(() => {
    loadProducts();
  }, [loadProducts, refreshTrigger]);

  // Handle page change
  const handlePageChange = useCallback((page: number) => {
    setFilters(prev => ({ ...prev, page }));
  }, []);

  // Handle page size change
  const handlePageSizeChange = useCallback((pageSize: number) => {
    setFilters(prev => ({ ...prev, pageSize, page: 1 }));
  }, []);

  // Handle sort
  const handleSort = useCallback((sortBy: 'name' | 'createdAt' | 'updatedAt', sortOrder: 'asc' | 'desc') => {
    setFilters(prev => ({ ...prev, sortBy, sortOrder, page: 1 }));
  }, []);

  // Handle search
  const handleSearch = useCallback((searchTerm: string) => {
    setFilters(prev => ({ ...prev, searchTerm, page: 1 }));
  }, []);

  // Handle filter panel
  const handleFiltersChange = useCallback((newFilters: Partial<ProductFilters>) => {
    setFilters(prev => ({
      ...prev,
      searchTerm: newFilters.searchTerm,
      categoryId: newFilters.categoryId,
      stockStatus: newFilters.stockStatus === 'all' ? undefined : newFilters.stockStatus,
      minPrice: newFilters.minPrice ? parseFloat(newFilters.minPrice) : undefined,
      maxPrice: newFilters.maxPrice ? parseFloat(newFilters.maxPrice) : undefined,
      createdFrom: newFilters.createdFrom || undefined,
      createdTo: newFilters.createdTo || undefined,
      updatedFrom: newFilters.updatedFrom || undefined,
      updatedTo: newFilters.updatedTo || undefined,
      page: 1
    }));
  }, []);

  const handleFilterApply = useCallback(() => {
    setIsFilterPanelOpen(false);
    loadProducts();
  }, [loadProducts]);

  const handleFilterReset = useCallback(() => {
    setFilters(prev => ({
      ...prev,
      searchTerm: '',
      categoryId: '',
      minPrice: undefined,
      maxPrice: undefined,
      stockStatus: undefined,
      createdFrom: undefined,
      createdTo: undefined,
      updatedFrom: undefined,
      updatedTo: undefined,
      page: 1
    }));
  }, []);

  // Handle product deletion
  const handleDeleteProduct = useCallback(async (product: ProductListDto) => {
    const confirmed = window.confirm(
      `Are you sure you want to delete "${product.name}"?\n\n` +
      `This will permanently remove the product and all its variants. This action cannot be undone.`
    );

    if (!confirmed) {
      return;
    }

    // Show loading state for the specific product
    setProducts(prev => prev.map(p =>
      p.id === product.id ? { ...p, isDeleting: true } : p
    ));

    try {
      await productsAPI.deleteBaseProduct(product.id);

      // Remove from local state
      setProducts(prev => prev.filter(p => p.id !== product.id));

      // Update pagination
      setPagination(prev => ({
        ...prev,
        totalCount: prev.totalCount - 1
      }));

      // Show success message
      showSuccessMessage(`Product "${product.name}" has been deleted successfully.`);
    } catch (err: any) {
      console.error('Error deleting product:', err);

      // Reset loading state
      setProducts(prev => prev.map(p =>
        p.id === product.id ? { ...p, isDeleting: false } : p
      ));

      // Show user-friendly error message
      const errorMessage = getErrorMessage(err, 'delete the product');
      showErrorMessage(errorMessage);
    }
  }, []);

  // Handle retry
  const handleRetry = useCallback(() => {
    loadProducts();
  }, [loadProducts]);

  // Helper functions for user-friendly messages
  const getErrorMessage = useCallback((error: any, action: string): string => {
    if (error.response) {
      const status = error.response.status;
      const data = error.response.data;

      switch (status) {
        case 400:
          return `Invalid request while trying to ${action}. Please check your input and try again.`;
        case 401:
          return `You are not authorized to ${action}. Please log in and try again.`;
        case 403:
          return `You don't have permission to ${action}. Please contact your administrator.`;
        case 404:
          return `The item you're trying to ${action.split(' ')[1]} could not be found. It may have been deleted.`;
        case 409:
          return `Cannot ${action} due to a conflict. The item may have been modified by someone else.`;
        case 422:
          return `The data provided is invalid. Please check your input and try again.`;
        case 429:
          return `Too many requests. Please wait a moment and try again.`;
        case 500:
          return `Server error occurred while trying to ${action}. Please try again later.`;
        case 503:
          return `Service is temporarily unavailable. Please try again later.`;
        default:
          return data?.message || `Failed to ${action}. Please try again.`;
      }
    }

    if (error.code === 'NETWORK_ERROR') {
      return `Network error occurred while trying to ${action}. Please check your internet connection and try again.`;
    }

    if (error.code === 'TIMEOUT') {
      return `Request timed out while trying to ${action}. Please try again.`;
    }

    return `An unexpected error occurred while trying to ${action}. Please try again.`;
  }, []);

  const showSuccessMessage = useCallback((message: string) => {
    // For now, use alert. In a real app, you'd use a toast notification system
    console.log('Success:', message);
    // You could integrate with a toast library here
    alert(`✅ ${message}`);
  }, []);

  const showErrorMessage = useCallback((message: string) => {
    console.error('Error:', message);
    alert(`❌ ${message}`);
  }, []);

  // Touch gesture handlers for mobile
  const handleTouchStart = useCallback((e: React.TouchEvent, productId: string) => {
    setTouchEnd(null);
    setTouchStart(e.targetTouches[0].clientX);
    setSwipedProductId(productId);
  }, []);

  const handleTouchMove = useCallback((e: React.TouchEvent) => {
    setTouchEnd(e.targetTouches[0].clientX);
  }, []);

  const handleTouchEnd = useCallback(() => {
    if (!touchStart || !touchEnd) return;

    const distance = touchStart - touchEnd;
    const isLeftSwipe = distance > 50;
    const isRightSwipe = distance < -50;

    if (isLeftSwipe || isRightSwipe) {
      // Handle swipe actions (could show action buttons, etc.)
      console.log('Swipe detected:', isLeftSwipe ? 'left' : 'right', 'on product:', swipedProductId);
    }

    setTouchStart(null);
    setTouchEnd(null);
    setSwipedProductId(null);
  }, [touchStart, touchEnd, swipedProductId]);

  // Keyboard navigation handlers
  const handleKeyDown = useCallback((e: React.KeyboardEvent, product: ProductListDto) => {
    switch (e.key) {
      case 'Enter':
      case ' ':
        e.preventDefault();
        onProductSelect?.(product);
        break;
      case 'Delete':
        if (e.ctrlKey || e.metaKey) {
          e.preventDefault();
          handleDeleteProduct(product);
        }
        break;
      case 'e':
        if (e.ctrlKey || e.metaKey) {
          e.preventDefault();
          onProductEdit?.(product);
        }
        break;
      default:
        break;
    }
  }, [onProductSelect, onProductEdit, handleDeleteProduct]);

  // Lazy loading function
  const loadMoreProducts = useCallback(async () => {
    if (isLoadingMore || !hasMore) return;

    setIsLoadingMore(true);
    try {
      const nextPage = pagination.page + 1;
      const response = await productsAPI.getProducts({ ...filters, page: nextPage });
      const responseData = response.data;

      if (responseData.success && responseData.data) {
        setProducts(prev => [...prev, ...(responseData.data.items || [])]);
        setPagination({
          page: responseData.data.page || nextPage,
          pageSize: responseData.data.pageSize || filters.pageSize || 10,
          totalCount: responseData.data.totalCount || 0,
          totalPages: responseData.data.totalPages || 1
        });
        setHasMore(responseData.data.hasNextPage || false);
      }
    } catch (err: any) {
      console.error('Error loading more products:', err);
      setHasMore(false); // Stop lazy loading on error
    } finally {
      setIsLoadingMore(false);
    }
  }, [isLoadingMore, hasMore, filters, pagination.page]);

  // Set up intersection observer for lazy loading
  useEffect(() => {
    if (observerRef.current) observerRef.current.disconnect();

    observerRef.current = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && hasMore && !isLoadingMore) {
          loadMoreProducts();
        }
      },
      { threshold: 0.1 }
    );

    if (sentinelRef.current) {
      observerRef.current.observe(sentinelRef.current);
    }

    return () => {
      if (observerRef.current) {
        observerRef.current.disconnect();
      }
    };
  }, [hasMore, isLoadingMore, loadMoreProducts]);

  // Loading state
  if (loading && products.length === 0) {
    return (
      <MaterialsLoading
        message="Loading Products..."
        description="Please wait while we fetch your product data."
        showProgress={true}
        progress={33}
        skeletonRows={8}
      />
    );
  }

  // Error state
  if (error) {
    return (
      <div className="products-error" role="alert" aria-live="assertive">
        <div className="error-message">
          <div className="error-icon" aria-hidden="true">⚠️</div>
          <h3>Unable to Load Products</h3>
          <p>{error}</p>
          <div className="error-actions">
            <button
              onClick={handleRetry}
              className="btn btn-primary"
              aria-label="Retry loading products"
            >
              🔄 Try Again
            </button>
            <button
              onClick={() => window.location.reload()}
              className="btn btn-outline-secondary"
              aria-label="Refresh the page"
            >
              🔄 Refresh Page
            </button>
          </div>
          <div className="error-suggestions">
            <p><strong>Suggestions:</strong></p>
            <ul>
              <li>Check your internet connection</li>
              <li>Try refreshing the page</li>
              <li>Contact support if the problem persists</li>
            </ul>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="products-list">

      {/* Filter Panel */}
      <ProductFilterPanel
        filters={{
          searchTerm: filters.searchTerm || '',
          categoryId: filters.categoryId || '',
          stockStatus: filters.stockStatus || 'all',
          minPrice: filters.minPrice?.toString() || '',
          maxPrice: filters.maxPrice?.toString() || '',
          createdFrom: filters.createdFrom || '',
          createdTo: filters.createdTo || '',
          updatedFrom: filters.updatedFrom || '',
          updatedTo: filters.updatedTo || ''
        }}
        onFiltersChange={handleFiltersChange}
        onApply={handleFilterApply}
        onReset={handleFilterReset}
        isOpen={isFilterPanelOpen}
        onToggle={() => setIsFilterPanelOpen(!isFilterPanelOpen)}
        categories={categories}
      />

      {/* Products Table */}
      {products.length === 0 ? (
        <MaterialsEmpty />
      ) : (
        <ProductTable
          products={products}
          sortBy={filters.sortBy}
          sortOrder={filters.sortOrder}
          onSort={handleSort}
          onProductSelect={onProductSelect}
          onProductDelete={showActions ? handleDeleteProduct : undefined}
          onProductEdit={onProductEdit}
          onVariantView={onVariantView}
          showActions={showActions}
          onTouchStart={handleTouchStart}
          onTouchMove={handleTouchMove}
          onTouchEnd={handleTouchEnd}
          onKeyDown={handleKeyDown}
        />
      )}

      {/* Lazy loading sentinel */}
      {hasMore && products.length > 0 && (
        <div ref={sentinelRef} className="lazy-loading-sentinel">
          {isLoadingMore && (
            <div className="lazy-loading-indicator">
              <MaterialsLoading />
              <span>Loading more products...</span>
            </div>
          )}
        </div>
      )}

      {/* Pagination (fallback for when lazy loading is disabled or reaches end) */}
      {!hasMore && pagination.totalPages > 1 && (
        <MaterialsPagination
          currentPage={pagination.page}
          totalPages={pagination.totalPages}
          pageSize={pagination.pageSize}
          totalCount={pagination.totalCount}
          onPageChange={handlePageChange}
          onPageSizeChange={handlePageSizeChange}
        />
      )}

      {/* Loading overlay for subsequent loads */}
      {loading && products.length > 0 && (
        <div className="loading-overlay">
          <div className="loading-spinner"></div>
        </div>
      )}
    </div>
  );
});

ProductList.displayName = 'ProductList';

export default ProductList;