import React, { useState } from 'react';
import ProductList from './ProductList';
import ProductWizard from './ProductWizard';
import { ProductListDto } from '../../services/api.types';
import './ProductsOverview.css';

/**
 * Comprehensive Products Overview component
 * Demonstrates the product listing with BaseProducts and their variants
 */
const ProductsOverview: React.FC = () => {
  const [selectedProduct, setSelectedProduct] = useState<ProductListDto | null>(null);
  const [viewMode, setViewMode] = useState<'list' | 'grid'>('list');
  const [productWizardOpen, setProductWizardOpen] = useState(false);
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  // Handle product selection
  const handleProductSelect = (product: ProductListDto) => {
    setSelectedProduct(product);
    console.log('Selected product:', product);
  };

  // Handle product edit
  const handleProductEdit = (product: ProductListDto) => {
    console.log('Edit product:', product);
    // TODO: Open edit modal/form
    alert(`Edit functionality for "${product.name}" would open here`);
  };

  // Handle product delete
  const handleProductDelete = (product: ProductListDto) => {
    console.log('Delete product:', product);
    // The confirmation is handled in ProductList component
  };

  // Handle variant view
  const handleVariantView = (variantId: string) => {
    console.log('View variant:', variantId);
    // TODO: Open variant details modal
    alert(`Variant details for ID "${variantId}" would open here`);
  };

  // Handle add product button click
  const handleAddProduct = () => {
    setProductWizardOpen(true);
  };

  // Handle product wizard success
  const handleProductWizardSuccess = (productId: string) => {
    console.log('Product created successfully:', productId);
    setRefreshTrigger(prev => prev + 1);
    setProductWizardOpen(false);
  };

  return (
    <div className="products-overview">
      <header className="products-header" role="banner">
        <div className="header-content">
          <h1 id="products-heading">Product Management</h1>
          <p id="products-description">Manage your base products and their variants</p>
        </div>
        <div className="header-actions">
          <button
            className="btn btn-primary"
            onClick={handleAddProduct}
            aria-describedby="products-description"
            aria-label="Add a new product with variants to the system"
          >
            + Add Product
          </button>
        </div>
      </header>

      <main className="products-content" role="main" aria-labelledby="products-heading">
        <div className="view-controls" role="toolbar" aria-label="View options">
          <div className="view-toggle" role="radiogroup" aria-label="Choose view mode">
            <button
              className={`btn ${viewMode === 'list' ? 'btn-primary' : 'btn-outline-primary'}`}
              onClick={() => setViewMode('list')}
              role="radio"
              aria-checked={viewMode === 'list'}
              aria-label="Switch to list view"
            >
              📋 List View
            </button>
            <button
              className={`btn ${viewMode === 'grid' ? 'btn-primary' : 'btn-outline-primary'}`}
              onClick={() => setViewMode('grid')}
              role="radio"
              aria-checked={viewMode === 'grid'}
              aria-label="Switch to grid view (coming soon)"
              disabled
            >
              🔲 Grid View
            </button>
          </div>
        </div>

        {viewMode === 'list' ? (
          <ProductList
            onProductSelect={handleProductSelect}
            onProductEdit={handleProductEdit}
            onProductDelete={handleProductDelete}
            onVariantView={handleVariantView}
            showActions={true}
            refreshTrigger={refreshTrigger}
          />
        ) : (
          <div className="grid-view-placeholder">
            <div className="placeholder-content">
              <h3>Grid View Coming Soon</h3>
              <p>The grid view will display products in a card-based layout for better visual browsing.</p>
              <button
                className="btn btn-outline-primary"
                onClick={() => setViewMode('list')}
              >
                Switch to List View
              </button>
            </div>
          </div>
        )}

        {selectedProduct && (
          <div className="selected-product-info">
            <h3>Selected Product Details</h3>
            <div className="product-summary">
              <h4>{selectedProduct.name}</h4>
              <p>{selectedProduct.description}</p>
              <div className="product-meta">
                <span>Category: {selectedProduct.categoryName || 'Uncategorized'}</span>
                <span>Base Price: ${selectedProduct.basePrice?.toFixed(2) || 'N/A'}</span>
                <span>Variants: {selectedProduct.totalVariants}</span>
                <span>Stock Status: {selectedProduct.stockStatus.replace('-', ' ')}</span>
              </div>
            </div>
          </div>
        )}
      </main>

      <ProductWizard
        open={productWizardOpen}
        onComplete={handleProductWizardSuccess}
        onCancel={() => setProductWizardOpen(false)}
      />
    </div>
  );
};

export default ProductsOverview;