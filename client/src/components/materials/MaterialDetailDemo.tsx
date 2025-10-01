import React, { useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  Divider,
  Grid,
  Card,
  CardContent,
  Chip,
  Button,
} from '@mui/material';
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  PhotoCamera as PhotoCameraIcon,
} from '@mui/icons-material';
import MaterialPhotoDisplay from './MaterialPhotoDisplay';
import MultiPhotoUpload from './MultiPhotoUpload';

// Mock material data
const mockMaterial = {
  id: 'demo-material-123',
  name: 'Steel Pipe 2" Schedule 40',
  description: 'High-quality carbon steel pipe for industrial applications',
  stockQuantity: 150,
  category: 'Piping',
  unit: 'pieces',
  usageNotes: 'Used in construction and manufacturing',
  createdAt: '2024-01-15T10:30:00Z',
  updatedAt: '2024-09-01T14:20:00Z',
};

// Mock photos data
const mockPhotos = [
  {
    id: 'photo-1',
    url: 'https://via.placeholder.com/800x600/4CAF50/FFFFFF?text=Steel+Pipe+Front',
    filename: 'steel_pipe_front.jpg',
    uploadedAt: '2024-09-01T10:00:00Z',
    size: 2457600, // 2.4MB
  },
  {
    id: 'photo-2',
    url: 'https://via.placeholder.com/800x600/2196F3/FFFFFF?text=Steel+Pipe+Side',
    filename: 'steel_pipe_side.jpg',
    uploadedAt: '2024-09-01T10:05:00Z',
    size: 1894400, // 1.8MB
  },
  {
    id: 'photo-3',
    url: 'https://via.placeholder.com/800x600/FF9800/FFFFFF?text=Steel+Pipe+Detail',
    filename: 'steel_pipe_detail.jpg',
    uploadedAt: '2024-09-01T10:10:00Z',
    size: 3123200, // 3MB
  },
];

const MaterialDetailDemo: React.FC = () => {
  const [photos, setPhotos] = useState(mockPhotos);
  const [showUpload, setShowUpload] = useState(false);

  const handlePhotosChange = (updatedPhotos: any[]) => {
    setPhotos(updatedPhotos);
  };

  const handleUploadSuccess = (uploadedUrls: string[]) => {
    console.log('Photos uploaded successfully:', uploadedUrls);
    setShowUpload(false);
    // In a real app, you would refresh the photos list here
  };

  const handleUploadError = (error: string) => {
    console.error('Upload failed:', error);
  };

  return (
    <Box sx={{ maxWidth: 1200, mx: 'auto', p: 3 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Material Detail Demo
      </Typography>

      <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
        This demo shows how the MaterialPhotoDisplay component integrates with a material detail page.
        It demonstrates photo viewing, management, and upload functionality.
      </Typography>

      {/* Material Information */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
          <Box>
            <Typography variant="h5" component="h2" gutterBottom>
              {mockMaterial.name}
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              {mockMaterial.description}
            </Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button
              variant="outlined"
              startIcon={<EditIcon />}
              size="small"
            >
              Edit
            </Button>
            <Button
              variant="outlined"
              color="error"
              startIcon={<DeleteIcon />}
              size="small"
            >
              Delete
            </Button>
          </Box>
        </Box>

        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 6 }}>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Typography variant="body2" color="text.secondary">
                  Stock Quantity:
                </Typography>
                <Chip
                  label={`${mockMaterial.stockQuantity} ${mockMaterial.unit}`}
                  color={mockMaterial.stockQuantity > 100 ? 'success' : 'warning'}
                  size="small"
                />
              </Box>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Typography variant="body2" color="text.secondary">
                  Category:
                </Typography>
                <Chip label={mockMaterial.category} variant="outlined" size="small" />
              </Box>
            </Box>
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
              <Typography variant="body2" color="text.secondary">
                Created: {new Date(mockMaterial.createdAt).toLocaleDateString()}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Last Updated: {new Date(mockMaterial.updatedAt).toLocaleDateString()}
              </Typography>
            </Box>
          </Grid>
        </Grid>

        {mockMaterial.usageNotes && (
          <Box sx={{ mt: 2 }}>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
              Usage Notes:
            </Typography>
            <Typography variant="body2">
              {mockMaterial.usageNotes}
            </Typography>
          </Box>
        )}
      </Paper>

      {/* Photos Section */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h6" component="h3">
            Photos ({photos.length})
          </Typography>
          <Button
            variant="contained"
            startIcon={<PhotoCameraIcon />}
            onClick={() => setShowUpload(!showUpload)}
          >
            {showUpload ? 'Cancel Upload' : 'Add Photos'}
          </Button>
        </Box>

        {showUpload && (
          <Box sx={{ mb: 3 }}>
            <MultiPhotoUpload
              materialId={mockMaterial.id}
              maxFiles={5}
              maxFileSize={5}
              onUploadSuccess={handleUploadSuccess}
              onUploadError={handleUploadError}
            />
          </Box>
        )}

        <MaterialPhotoDisplay
          materialId={mockMaterial.id}
          photos={photos}
          onPhotosChange={handlePhotosChange}
        />
      </Paper>

      <Divider sx={{ my: 4 }} />

      {/* Usage Instructions */}
      <Box sx={{ mt: 4 }}>
        <Typography variant="h6" gutterBottom>
          Photo Display Features Demonstrated
        </Typography>
        <Typography variant="body2" component="div">
          <ul>
            <li><strong>Responsive Grid:</strong> Photos are displayed in a responsive grid that adapts to screen size</li>
            <li><strong>Thumbnail View:</strong> Click any thumbnail to view the full-size image in a modal</li>
            <li><strong>Modal Navigation:</strong> Use arrow keys or navigation buttons to browse through photos</li>
            <li><strong>Photo Management:</strong> Delete photos using the delete button on each thumbnail</li>
            <li><strong>Drag & Drop Reorder:</strong> Drag photos to reorder them (drag indicator visible on hover)</li>
            <li><strong>Lazy Loading:</strong> Images load only when they come into view for better performance</li>
            <li><strong>Loading States:</strong> Shows loading spinner while fetching photos</li>
            <li><strong>Error Handling:</strong> Displays error messages with retry options</li>
            <li><strong>Mobile Responsive:</strong> Optimized layout for mobile devices</li>
            <li><strong>Material-UI Integration:</strong> Consistent with Material Design principles</li>
          </ul>
        </Typography>
      </Box>
    </Box>
  );
};

export default MaterialDetailDemo;