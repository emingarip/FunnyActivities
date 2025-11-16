import { useState, useEffect, useCallback } from 'react';
import { productsAPI } from '../../../services/api';
import {
  MaterialListDto,
  PagedResult,
  GetMaterialsParams,
  ProductListDto,
  GetProductsParams
} from '../../../services/api.types';

/**
 * Custom hook for managing materials data and filtering
 * @param initialFilters - Initial filter parameters
 * @returns Materials data and filter management functions
 */
export const useMaterialsData = (initialFilters: GetMaterialsParams = {}) => {
  const [materials, setMaterials] = useState<MaterialListDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pagination, setPagination] = useState({
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
    hasPreviousPage: false,
    hasNextPage: false
  });

  const [filters, setFilters] = useState<GetMaterialsParams>({
    page: 1,
    pageSize: 10,
    sortBy: 'name',
    sortOrder: 'asc',
    ...initialFilters
  });

  const mapProductToMaterial = useCallback((product: ProductListDto): MaterialListDto => {
    const primaryVariant = product.variants?.[0];
    const stockQuantity = product.variants?.reduce((sum, variant) => sum + (variant.stockQuantity || 0), 0) ?? 0;

    return {
      id: product.id,
      name: product.name,
      category: product.categoryName,
      stockQuantity,
      unit: primaryVariant?.unitSymbol || primaryVariant?.unitOfMeasureName || '',
      photoCount: primaryVariant?.photos?.length ?? 0,
      thumbnailUrl: primaryVariant?.photos?.[0]
    };
  }, []);

  const fetchMaterials = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      // Map materials filters to product filters
      const productFilters: GetProductsParams = {
        page: filters.page,
        pageSize: filters.pageSize,
        searchTerm: filters.search ?? filters.name,
        sortBy: filters.sortBy === 'name' ? 'name' : 'updatedAt',
        sortOrder: filters.sortOrder,
        // Category and date filters can be added here when backend supports them
      };

      const response = await productsAPI.getProducts(productFilters);
      const responseData = response.data;

      if (!responseData?.data) {
        throw new Error('Products endpoint returned no data');
      }

      const productResult = responseData.data as PagedResult<ProductListDto>;
      const mappedItems = (productResult.items || []).map(mapProductToMaterial);

      setMaterials(mappedItems);
      setPagination({
        page: productResult.page,
        pageSize: productResult.pageSize,
        totalCount: productResult.totalCount,
        totalPages: productResult.totalPages,
        hasPreviousPage: productResult.hasPreviousPage,
        hasNextPage: productResult.hasNextPage
      });
    } catch (err: any) {
      console.error('Error fetching materials:', err);
      // Handle standardized API error format
      if (err.response?.data && typeof err.response.data.success === 'boolean' && !err.response.data.success) {
        setError(err.response.data.message || 'Failed to load materials');
      } else {
        setError(err.message || 'Failed to load materials');
      }
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    fetchMaterials();
  }, [fetchMaterials]);

  const updateFilters = useCallback((newFilters: Partial<GetMaterialsParams>) => {
    setFilters(prev => ({
      ...prev,
      ...newFilters,
      page: newFilters.page !== undefined ? newFilters.page : 1 // Reset to first page when filters change
    }));
  }, []);

  const handlePageChange = useCallback((newPage: number) => {
    updateFilters({ page: newPage });
  }, [updateFilters]);

  const handlePageSizeChange = useCallback((newPageSize: number) => {
    updateFilters({ page: 1, pageSize: newPageSize });
  }, [updateFilters]);

  const handleSort = useCallback((sortBy: 'name' | 'stockQuantity', sortOrder: 'asc' | 'desc') => {
    updateFilters({ sortBy, sortOrder, page: 1 });
  }, [updateFilters]);

  return {
    materials,
    loading,
    error,
    pagination,
    filters,
    fetchMaterials,
    updateFilters,
    handlePageChange,
    handlePageSizeChange,
    handleSort
  };
};
