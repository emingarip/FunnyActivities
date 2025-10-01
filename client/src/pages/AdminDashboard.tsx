import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { Card, CardContent, Button, Grid } from '@mui/material';
import { useUserCount } from '../hooks/useUserCount';
import { useOnlineUsers } from '../hooks/useOnlineUsers';
import UserGrowthChart from '../components/charts/UserGrowthChart';
import './AdminDashboard.css';

interface DashboardStats {
  totalUsers: number;
  activeUsers: number;
  inactiveUsers: number;
  adminUsers: number;
  regularUsers: number;
}


const AdminDashboard: React.FC = () => {
  const [stats, setStats] = useState<DashboardStats | null>(null);

  // Use hooks for data fetching
  const {
    data: userCountData,
    isLoading: countLoading,
    error: countError,
    refetch: refetchCount
  } = useUserCount();

  const {
    data: onlineUsersData,
    isLoading: onlineUsersLoading,
    error: onlineUsersError,
    refetch: refetchOnlineUsers
  } = useOnlineUsers();

  const loading = countLoading || onlineUsersLoading;
  const error = countError || (onlineUsersError as Error | null);

  // Update stats when user count data is available
  React.useEffect(() => {
    if (userCountData) {
      setStats(prev => prev ? { ...prev, totalUsers: userCountData.totalUsers } : {
        totalUsers: userCountData.totalUsers,
        activeUsers: 142,
        inactiveUsers: 8,
        adminUsers: 3,
        regularUsers: 147
      });
    }
  }, [userCountData]);


  if (loading) {
    return (
      <div className="page-container">
        <div className="loading">
          <div className="spinner"></div>
          Loading dashboard...
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="page-container">
        <div className="error">
          {error.message || 'Error loading data'}
          <div className="retry-buttons">
            <button onClick={() => { refetchCount(); if (refetchOnlineUsers) refetchOnlineUsers(); }} className="retry-btn">
              Retry
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="page-container">
      <h1 aria-label={`Admin Dashboard - Total users: ${stats?.totalUsers || 0}`}>
        Admin Dashboard - Total Users: {stats?.totalUsers || 0}
      </h1>
  
      {/* Stats Cards */}
      <div className="stats-grid">
        <div className="stat-card">
          <h3>Total Users</h3>
          <p className="stat-number">{stats?.totalUsers || 0}</p>
        </div>
        <div className="stat-card">
          <h3>Online Users</h3>
          <p className="stat-number">{onlineUsersData || 0}</p>
          <div className="online-indicator">
            <span className="online-dot"></span>
            Live Data
          </div>
        </div>
      </div>

      {/* User Growth Chart */}
      <div className="chart-section" role="region" aria-label="User growth chart">
        <UserGrowthChart height={400} />
      </div>

      {/* Management Sections */}
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Button component={Link} to="/admin/activities" variant="contained" color="primary">
                Manage Activities
              </Button>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Button component={Link} to="/admin/products" variant="contained" color="primary">
                Manage Products
              </Button>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Button component={Link} to="/admin/materials" variant="contained" color="primary">
                Manage Materials
              </Button>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Button component={Link} to="/admin/surveys" variant="contained" color="primary">
                Manage Surveys
              </Button>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* System Settings */}
      <div className="system-settings">
        <h2>System Settings</h2>
        <div className="settings-grid">
          <div className="setting-item">
            <h4>User Registration</h4>
            <button className="btn btn-secondary">Configure</button>
          </div>
          <div className="setting-item">
            <h4>Email Settings</h4>
            <button className="btn btn-secondary">Configure</button>
          </div>
          <div className="setting-item">
            <h4>Security Settings</h4>
            <button className="btn btn-secondary">Configure</button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AdminDashboard;