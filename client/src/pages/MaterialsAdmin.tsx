import React, { useState } from 'react';
import MaterialsList from '../components/materials/MaterialsList';
import MaterialsOverview from '../components/materials/MaterialsOverview';
import AuditLogsViewer from '../components/materials/audit/AuditLogsViewer';
import { MaterialListDto } from '../services/api.types';
import './MaterialsAdmin.css';

const MaterialsAdmin: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'materials' | 'audit' | 'overview'>('overview');

  const handleMaterialSelect = (material: MaterialListDto) => {
    // Navigate to material detail view
    console.log('Selected material:', material);
    // TODO: Implement navigation to material detail page
  };

  const handleMaterialEdit = (material: MaterialListDto) => {
    // Navigate to material edit page
    console.log('Edit material:', material);
    // TODO: Implement navigation to material edit page
  };

  const handleMaterialDelete = (material: MaterialListDto) => {
    // Material deletion is handled in MaterialsList component
    console.log('Delete material:', material);
  };


  const handleTabChange = (tab: 'materials' | 'audit' | 'overview') => {
    setActiveTab(tab);
  };

  return (
    <div className="materials-admin">
      <div className="admin-header">
        <div className="header-content">
          <h1>Materials Management</h1>
          <p>Manage your inventory materials, track stock levels, and organize your supplies.</p>
        </div>

      </div>

      {/* Navigation Tabs */}
      <div className="admin-tabs">
        <button
          className={`tab-button ${activeTab === 'overview' ? 'active' : ''}`}
          onClick={() => handleTabChange('overview')}
        >
          📊 Overview
        </button>
        <button
          className={`tab-button ${activeTab === 'materials' ? 'active' : ''}`}
          onClick={() => handleTabChange('materials')}
        >
          📦 Materials
        </button>
        <button
          className={`tab-button ${activeTab === 'audit' ? 'active' : ''}`}
          onClick={() => handleTabChange('audit')}
        >
          📋 Audit Logs
        </button>
      </div>

      {/* Tab Content */}
      <div className="tab-content">

        {activeTab === 'overview' && (
          <MaterialsOverview />
        )}

        {activeTab === 'materials' && (
          <div className="materials-content">
            <MaterialsList
              onMaterialSelect={handleMaterialSelect}
              onMaterialEdit={handleMaterialEdit}
              onMaterialDelete={handleMaterialDelete}
              showActions={true}
            />
          </div>
        )}

        {activeTab === 'audit' && (
          <AuditLogsViewer />
        )}

      </div>
    </div>
  );
};

export default MaterialsAdmin;