import React, { useState } from 'react';
import { Box, Typography, Paper, Divider } from '@mui/material';
import MultiPhotoUpload from './MultiPhotoUpload';

const MaterialPhotoUploadDemo: React.FC = () => {
  const [uploadedUrls, setUploadedUrls] = useState<string[]>([]);

  const handleUploadSuccess = (urls: string[]) => {
    setUploadedUrls(urls);
    console.log('Photos uploaded successfully:', urls);
  };

  const handleUploadError = (error: string) => {
    console.error('Upload failed:', error);
  };

  return (
    <Box sx={{ maxWidth: 800, mx: 'auto', p: 3 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Material Photo Upload Demo
      </Typography>

      <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
        This demo shows how to use the MultiPhotoUpload component for uploading multiple photos
        for materials. The component integrates with the backend API and provides a complete
        upload experience with validation, previews, and progress tracking.
      </Typography>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" gutterBottom>
          Photo Upload Component
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Upload multiple photos for material ID: demo-material-123
        </Typography>

        <MultiPhotoUpload
          materialId="demo-material-123"
          maxFiles={5}
          maxFileSize={3} // 3MB
          acceptedTypes={['image/jpeg', 'image/png', 'image/webp']}
          onUploadSuccess={handleUploadSuccess}
          onUploadError={handleUploadError}
        />
      </Paper>

      {uploadedUrls.length > 0 && (
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom>
            Uploaded Photos
          </Typography>
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
            {uploadedUrls.map((url, index) => (
              <Box key={url} sx={{ width: 100, height: 100 }}>
                <img
                  src={url}
                  alt={`Uploaded photo ${index + 1}`}
                  style={{
                    width: '100%',
                    height: '100%',
                    objectFit: 'cover',
                    borderRadius: 8,
                    border: '1px solid #e0e0e0'
                  }}
                />
              </Box>
            ))}
          </Box>
        </Paper>
      )}

      <Divider sx={{ my: 4 }} />

      <Box sx={{ mt: 4 }}>
        <Typography variant="h6" gutterBottom>
          Usage Instructions
        </Typography>
        <Typography variant="body2" component="div">
          <ul>
            <li><strong>Drag & Drop:</strong> Drag image files directly onto the upload area</li>
            <li><strong>Browse Files:</strong> Click "Choose Files" or the upload area to open file browser</li>
            <li><strong>Multiple Selection:</strong> Hold Ctrl/Cmd to select multiple files at once</li>
            <li><strong>Validation:</strong> Only image files (JPEG, PNG, WebP) under 3MB are accepted</li>
            <li><strong>Preview:</strong> See thumbnails of selected files before uploading</li>
            <li><strong>Progress:</strong> Monitor upload progress for each file</li>
            <li><strong>Error Handling:</strong> Clear error messages for failed uploads</li>
          </ul>
        </Typography>
      </Box>
    </Box>
  );
};

export default MaterialPhotoUploadDemo;