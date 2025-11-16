import React, { useState, useEffect, useCallback } from 'react';
import { productsAPI } from '../../services/api';
import { MaterialListDto } from '../../services/api.types';
import MaterialsTable from './MaterialsTable';
import MaterialsPagination from './MaterialsPagination';
import MaterialsLoading from './MaterialsLoading';
import MaterialsEmpty from './MaterialsEmpty';
import MaterialFilterPanel, { AdvancedFilters } from './MaterialFilterPanel';
import { useMaterialsData } from './hooks/useMaterialsData';
import { useMaterialsFilters } from './hooks/useMaterialsFilters';
import './MaterialsList.css';

/**
 * Props for the MaterialsList component
 */
interface MaterialsListProps {
  /** Callback when a material is selected */
  onMaterialSelect?: (material: MaterialListDto) => void;
  /** Callback when a material is edited */
  onMaterialEdit?: (material: MaterialListDto) => void;
  /** Callback when a material is deleted */
  onMaterialDelete?: (material: MaterialListDto) => void;
  /** Callback when a material's photo is clicked */
  onPhotoClick?: (material: MaterialListDto) => void;
  /** Whether to show action buttons */
  showActions?: boolean;
}

/**
 * Main MaterialsList component with improved structure and performance
 */
const MaterialsList: React.FC<MaterialsListProps> = React.memo(({
  onMaterialSelect,
  onMaterialEdit,
  onMaterialDelete,
  onPhotoClick,
  showActions = true
}) => {
  // Use custom hooks for data management and filtering
  const {
    materials,
    loading,
    error,
    pagination,
    filters,
    handlePageChange,
    handlePageSizeChange,
    handleSort,
    updateFilters
  } = useMaterialsData();

  const { uiFilters, handleUiFiltersChange, handleClearAllFilters } = useMaterialsFilters(updateFilters);

  // Advanced filters panel state
  const [advancedFilters, setAdvancedFilters] = useState<AdvancedFilters>({
    minStock: '',
    maxStock: '',
    unit: '',
    createdFrom: '',
    createdTo: '',
    updatedFrom: '',
    updatedTo: '',
    stockStatus: 'all'
  });

  const [showAdvancedPanel, setShowAdvancedPanel] = useState(false);
  const [categories, setCategories] = useState<string[]>([]);

  // Memoized categories loading
  const loadCategories = useCallback(async () => {
    try {
      const response = await productsAPI.getProducts({ pageSize: 1000 });
      const productData = response.data.data?.items || [];
      const uniqueCategories: string[] = [];
      const categorySet = new Set<string>();

      productData.forEach((item: any) => {
        const categoryName = item.categoryName;
        if (categoryName && !categorySet.has(categoryName)) {
          categorySet.add(categoryName);
          uniqueCategories.push(categoryName);
        }
      });

      setCategories(uniqueCategories.sort());
    } catch (error: any) {
      console.error('Error loading categories:', error);
    }
  }, []);

  useEffect(() => {
    loadCategories();
  }, [loadCategories]);

  // Handle advanced filters
  const handleAdvancedFiltersChange = useCallback((newFilters: Partial<AdvancedFilters>) => {
    setAdvancedFilters(prev => ({ ...prev, ...newFilters }));

    // Update API filters
    updateFilters({
      minStock: newFilters.minStock !== undefined ? (newFilters.minStock ? parseFloat(newFilters.minStock) : undefined) : undefined,
      maxStock: newFilters.maxStock !== undefined ? (newFilters.maxStock ? parseFloat(newFilters.maxStock) : undefined) : undefined,
      unit: newFilters.unit !== undefined ? (newFilters.unit || undefined) : undefined,
      createdFrom: newFilters.createdFrom !== undefined ? (newFilters.createdFrom || undefined) : undefined,
      createdTo: newFilters.createdTo !== undefined ? (newFilters.createdTo || undefined) : undefined,
      updatedFrom: newFilters.updatedFrom !== undefined ? (newFilters.updatedFrom || undefined) : undefined,
      updatedTo: newFilters.updatedTo !== undefined ? (newFilters.updatedTo || undefined) : undefined,
      page: 1
    });
  }, [updateFilters]);

  // Handle advanced filters panel toggle
  const handleAdvancedFiltersToggle = useCallback(() => {
    setShowAdvancedPanel(prev => !prev);
  }, []);

  // Handle advanced filters apply
  const handleAdvancedFiltersApply = useCallback(() => {
    setShowAdvancedPanel(false);
  }, []);

  // Handle advanced filters reset
  const handleAdvancedFiltersReset = useCallback(() => {
    handleClearAllFilters();
    setShowAdvancedPanel(false);
  }, [handleClearAllFilters]);

  // Handle material deletion (informational only; actual delete via products module)
  const handleDeleteMaterial = useCallback(async (material: MaterialListDto) => {
    if (!window.confirm(`Are you sure you want to delete "${material.name}"?`)) {
      return;
    }

    try {
      alert('Silmeyi urunler uzerinden yapin. (Materials gorunumu sadece listeleme amacli)');
    } catch (err: any) {
      console.error('Error deleting material:', err);
      const errorMessage = err?.message || 'Failed to delete material';
      alert('Failed to delete material: ' + errorMessage);
    }
  }, []);

  // Memoized error retry handler
  const handleRetry = useCallback(() => {
    window.location.reload(); // Temporary solution
  }, []);

  // Loading state
  if (loading && materials.length === 0) {
    return <MaterialsLoading />;
  }

  // Error state
  if (error) {
    return (
      <div className="materials-error">
        <div className="error-message">
          <h3>Error Loading Materials</h3>
          <p>{error}</p>
          <button onClick={handleRetry} className="btn btn-primary">
            Try Again
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="materials-list">
      {/* Filters */}

      {/* Advanced Filter Panel */}
      <MaterialFilterPanel
        filters={advancedFilters}
        onFiltersChange={handleAdvancedFiltersChange}
        onApply={handleAdvancedFiltersApply}
        onReset={handleAdvancedFiltersReset}
        isOpen={showAdvancedPanel}
        onToggle={handleAdvancedFiltersToggle}
      />

      {/* Materials Table */}
      {materials.length === 0 ? (
        <MaterialsEmpty />
      ) : (
        <MaterialsTable
          materials={materials}
          sortBy={filters.sortBy}
          sortOrder={filters.sortOrder}
          onSort={handleSort}
          onMaterialSelect={onMaterialSelect}
          onMaterialDelete={showActions ? handleDeleteMaterial : undefined}
          onPhotoClick={onPhotoClick}
          showActions={showActions}
        />
      )}

      {/* Pagination */}
      {pagination.totalPages > 1 && (
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
      {loading && materials.length > 0 && (
        <div className="loading-overlay">
          <div className="loading-spinner"></div>
        </div>
      )}
    </div>
  );
});

MaterialsList.displayName = 'MaterialsList';

export default MaterialsList;


