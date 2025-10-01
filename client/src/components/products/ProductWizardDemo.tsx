import React, { useState } from 'react';
import { Button, Box, Typography } from '@mui/material';
import { Add as AddIcon } from '@mui/icons-material';
import ProductWizard from './ProductWizard';

/**
 * Demo component showing how to use the ProductWizard
 */
const ProductWizardDemo: React.FC = () => {
  const [wizardOpen, setWizardOpen] = useState(false);
  const [lastCreatedProductId, setLastCreatedProductId] = useState<string | null>(null);

  const handleCreateProduct = () => {
    setWizardOpen(true);
  };

  const handleWizardComplete = (productId: string) => {
    setLastCreatedProductId(productId);
    setWizardOpen(false);
    // Here you could navigate to the product detail page or refresh the product list
    console.log('Product created with ID:', productId);
  };

  const handleWizardCancel = () => {
    setWizardOpen(false);
  };

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Product Management
      </Typography>

      <Typography variant="body1" sx={{ mb: 3 }}>
        Create and manage your products with our comprehensive product wizard.
      </Typography>

      <Button
        variant="contained"
        startIcon={<AddIcon />}
        onClick={handleCreateProduct}
        size="large"
        sx={{ mb: 2 }}
      >
        Create New Product
      </Button>

      {lastCreatedProductId && (
        <Typography variant="body2" color="success.main" sx={{ mt: 2 }}>
          ✓ Product created successfully! ID: {lastCreatedProductId}
        </Typography>
      )}

      <ProductWizard
        open={wizardOpen}
        onComplete={handleWizardComplete}
        onCancel={handleWizardCancel}
      />
    </Box>
  );
};

export default ProductWizardDemo;