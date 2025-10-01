import { useState, useCallback, useEffect } from 'react';
import { GetMaterialsParams } from '../../../services/api.types';
import { validateFilters } from '../utils/validationUtils';

/**
 * Custom hook for managing materials filters
 * @param onFiltersChange - Callback when filters change
 * @returns Filter state and handlers
 */
export const useMaterialsFilters = (
  onFiltersChange: (filters: Partial<GetMaterialsParams>) => void
) => {
  // UI filter state
  const [uiFilters, setUiFilters] = useState<any>({
    search: '',
    category: '',
    minStock: '',
    maxStock: '',
    unit: '',
    createdFrom: '',
    createdTo: '',
    updatedFrom: '',
    updatedTo: ''
  });

  // Handle UI filter changes
  const handleUiFiltersChange = useCallback((newFilters: Partial<any>) => {
    const validatedFilters = validateFilters(newFilters);
    setUiFilters((prev: any) => ({ ...prev, ...validatedFilters }));

    // Update API filters
    onFiltersChange({
      search: validatedFilters.search !== undefined ? validatedFilters.search : undefined,
      category: validatedFilters.category !== undefined ? (validatedFilters.category || undefined) : undefined,
      minStock: validatedFilters.minStock !== undefined ? (validatedFilters.minStock ? parseFloat(validatedFilters.minStock) : undefined) : undefined,
      maxStock: validatedFilters.maxStock !== undefined ? (validatedFilters.maxStock ? parseFloat(validatedFilters.maxStock) : undefined) : undefined,
      unit: validatedFilters.unit !== undefined ? (validatedFilters.unit || undefined) : undefined,
      createdFrom: validatedFilters.createdFrom !== undefined ? (validatedFilters.createdFrom || undefined) : undefined,
      createdTo: validatedFilters.createdTo !== undefined ? (validatedFilters.createdTo || undefined) : undefined,
      updatedFrom: validatedFilters.updatedFrom !== undefined ? (validatedFilters.updatedFrom || undefined) : undefined,
      updatedTo: validatedFilters.updatedTo !== undefined ? (validatedFilters.updatedTo || undefined) : undefined,
      page: 1
    });
  }, [onFiltersChange]);

  // Clear all filters
  const handleClearAllFilters = useCallback(() => {
    const resetFilters: any = {
      search: '',
      category: '',
      minStock: '',
      maxStock: '',
      unit: '',
      createdFrom: '',
      createdTo: '',
      updatedFrom: '',
      updatedTo: ''
    };

    setUiFilters(resetFilters);

    onFiltersChange({
      search: undefined,
      category: undefined,
      minStock: undefined,
      maxStock: undefined,
      unit: undefined,
      createdFrom: undefined,
      createdTo: undefined,
      updatedFrom: undefined,
      updatedTo: undefined,
      page: 1
    });
  }, [onFiltersChange]);

  // URL parameter synchronization
  useEffect(() => {
    const urlParams = new URLSearchParams(window.location.search);

    // Load filters from URL
    const search = urlParams.get('search') || '';
    const category = urlParams.get('category') || '';
    const minStock = urlParams.get('minStock') || '';
    const maxStock = urlParams.get('maxStock') || '';
    const unit = urlParams.get('unit') || '';
    const createdFrom = urlParams.get('createdFrom') || '';
    const createdTo = urlParams.get('createdTo') || '';
    const updatedFrom = urlParams.get('updatedFrom') || '';
    const updatedTo = urlParams.get('updatedTo') || '';

    setUiFilters({
      search,
      category,
      minStock,
      maxStock,
      unit,
      createdFrom,
      createdTo,
      updatedFrom,
      updatedTo
    });
  }, []);

  return {
    uiFilters,
    handleUiFiltersChange,
    handleClearAllFilters
  };
};