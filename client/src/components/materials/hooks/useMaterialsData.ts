import { useState, useEffect, useCallback } from 'react';
import { materialsAPI } from '../../../services/api';
import { MaterialListDto, PagedResult, GetMaterialsParams } from '../../../services/api.types';

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

  const fetchMaterials = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const response = await materialsAPI.getMaterials(filters);
      const result: PagedResult<MaterialListDto> = response.data.data;

      setMaterials(result.items);
      setPagination({
        page: result.page,
        pageSize: result.pageSize,
        totalCount: result.totalCount,
        totalPages: result.totalPages,
        hasPreviousPage: result.hasPreviousPage,
        hasNextPage: result.hasNextPage
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