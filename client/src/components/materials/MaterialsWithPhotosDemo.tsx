import React, { useState } from 'react';
import { MaterialListDto } from '../../services/api.types';
import MaterialsList from './MaterialsList';
import MaterialPhotoDisplay from './MaterialPhotoDisplay';

// Demo component to showcase photo previews in materials list
const MaterialsWithPhotosDemo: React.FC = () => {
  const [selectedMaterial, setSelectedMaterial] = useState<MaterialListDto | null>(null);
  const [showPhotoGallery, setShowPhotoGallery] = useState(false);

  // Mock data with photo information
  const mockMaterials: MaterialListDto[] = [
    {
      id: '1',
      name: 'Steel Pipe 2"',
      category: 'Pipes',
      stockQuantity: 150,
      unit: 'Pieces',
      photoCount: 3,
      thumbnailUrl: 'https://via.placeholder.com/100x100/4a90e2/ffffff?text=Pipe'
    },
    {
      id: '2',
      name: 'Copper Wire 10mm',
      category: 'Electrical',
      stockQuantity: 500,
      unit: 'Meters',
      photoCount: 1,
      thumbnailUrl: 'https://via.placeholder.com/100x100/e74c3c/ffffff?text=Wire'
    },
    {
      id: '3',
      name: 'Concrete Blocks',
      category: 'Building Materials',
      stockQuantity: 2000,
      unit: 'Pieces',
      photoCount: 0 // No photos
    },
    {
      id: '4',
      name: 'PVC Pipe 4"',
      category: 'Pipes',
      stockQuantity: 75,
      unit: 'Pieces',
      photoCount: 5,
      thumbnailUrl: 'https://via.placeholder.com/100x100/27ae60/ffffff?text=PVC'
    },
    {
      id: '5',
      name: 'Steel Rebar 12mm',
      category: 'Reinforcement',
      stockQuantity: 300,
      unit: 'Meters',
      photoCount: 2,
      thumbnailUrl: 'https://via.placeholder.com/100x100/f39c12/ffffff?text=Rebar'
    }
  ];

  const handleMaterialSelect = (material: MaterialListDto) => {
    setSelectedMaterial(material);
    console.log('Selected material:', material);
  };

  const handlePhotoClick = (material: MaterialListDto) => {
    setSelectedMaterial(material);
    setShowPhotoGallery(true);
    console.log('Photo clicked for material:', material);
  };

  const handleClosePhotoGallery = () => {
    setShowPhotoGallery(false);
    setSelectedMaterial(null);
  };

  return (
    <div style={{ padding: '20px' }}>
      <h1>Materials List with Photo Previews</h1>
      <p>This demo shows the MaterialsList component with integrated photo previews.</p>

      <div style={{ marginBottom: '20px' }}>
        <h3>Features:</h3>
        <ul>
          <li>Photo count indicator in material list rows</li>
          <li>Thumbnail preview for materials with photos</li>
          <li>Lazy loading for photo thumbnails</li>
          <li>Click to view full photo gallery</li>
          <li>Responsive design for different screen sizes</li>
          <li>Performance optimization (lazy loading)</li>
        </ul>
      </div>

      <MaterialsList
        onMaterialSelect={handleMaterialSelect}
        onPhotoClick={handlePhotoClick}
      />

      {/* Photo Gallery Modal */}
      {showPhotoGallery && selectedMaterial && (
        <div style={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          backgroundColor: 'rgba(0,0,0,0.8)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 1000
        }}>
          <div style={{
            backgroundColor: 'white',
            padding: '20px',
            borderRadius: '8px',
            maxWidth: '80%',
            maxHeight: '80%',
            overflow: 'auto'
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
              <h2>Photos for {selectedMaterial.name}</h2>
              <button
                onClick={handleClosePhotoGallery}
                style={{
                  background: 'none',
                  border: 'none',
                  fontSize: '24px',
                  cursor: 'pointer',
                  padding: '5px'
                }}
              >
                ×
              </button>
            </div>

            <MaterialPhotoDisplay
              materialId={selectedMaterial.id}
              photos={[
                {
                  id: '1',
                  url: selectedMaterial.thumbnailUrl || 'https://via.placeholder.com/400x300',
                  filename: 'sample-photo.jpg',
                  uploadedAt: new Date().toISOString(),
                  size: 1024000
                }
              ]}
            />
          </div>
        </div>
      )}

      {/* Instructions */}
      <div style={{ marginTop: '40px', padding: '20px', backgroundColor: '#f5f5f5', borderRadius: '8px' }}>
        <h3>Instructions:</h3>
        <p>Click on the photo thumbnails in the table to open the full photo gallery.</p>
        <p>The photo count is displayed as a badge on thumbnails when there are multiple photos.</p>
        <p>Materials without photos show a placeholder icon.</p>
        <p>Thumbnails are lazy-loaded for better performance.</p>
      </div>
    </div>
  );
};

export default MaterialsWithPhotosDemo;