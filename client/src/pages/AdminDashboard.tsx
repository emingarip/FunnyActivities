import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { Card, CardContent, Button, Grid } from '@mui/material';
import { useUserCount } from '../hooks/useUserCount';
import { useOnlineUsers } from '../hooks/useOnlineUsers';
import UserGrowthChart from '../components/charts/UserGrowthChart';
import './AdminDashboard.css';
import { useTranslation } from '../hooks/useTranslation';

interface DashboardStats {
  totalUsers: number;
  activeUsers: number;
  inactiveUsers: number;
  adminUsers: number;
  regularUsers: number;
}

const formatWithPlaceholder = (template: string, value: string | number) =>
  template.replace('{0}', String(value));

const AdminDashboard: React.FC = () => {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const { t } = useTranslation();

  const {
    data: userCountData,
    isLoading: countLoading,
    error: countError,
    refetch: refetchCount,
  } = useUserCount();

  const {
    data: onlineUsersData,
    isLoading: onlineUsersLoading,
    error: onlineUsersError,
    refetch: refetchOnlineUsers,
  } = useOnlineUsers();

  const loading = countLoading || onlineUsersLoading;
  const error = countError || (onlineUsersError as Error | null);

  React.useEffect(() => {
    if (userCountData) {
      setStats((prev) =>
        prev
          ? { ...prev, totalUsers: userCountData.totalUsers }
          : {
              totalUsers: userCountData.totalUsers,
              activeUsers: 142,
              inactiveUsers: 8,
              adminUsers: 3,
              regularUsers: 147,
            }
      );
    }
  }, [userCountData]);

  if (loading) {
    return (
      <div className="page-container">
        <div className="loading">
          <div className="spinner"></div>
          {t('admin_loading')}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="page-container">
        <div className="error">
          {error.message || t('admin_error')}
          <div className="retry-buttons">
            <button
              onClick={() => {
                refetchCount();
                if (refetchOnlineUsers) refetchOnlineUsers();
              }}
              className="retry-btn"
            >
              {t('admin_retry')}
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="page-container">
      <h1 aria-label={formatWithPlaceholder(t('admin_title'), stats?.totalUsers || 0)}>
        {formatWithPlaceholder(t('admin_title'), stats?.totalUsers || 0)}
      </h1>

      {/* Stats Cards */}
      <div className="stats-grid">
        <div className="stat-card">
          <h3>{t('admin_stats_total')}</h3>
          <p className="stat-number">{stats?.totalUsers || 0}</p>
        </div>
        <div className="stat-card">
          <h3>{t('admin_stats_online')}</h3>
          <p className="stat-number">{onlineUsersData || 0}</p>
          <div className="online-indicator">
            <span className="online-dot"></span>
            {t('admin_stats_live')}
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
                {t('admin_manage_activities')}
              </Button>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Button component={Link} to="/admin/personas" variant="contained" color="primary">
                {t('admin_manage_personas')}
              </Button>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Button component={Link} to="/admin/products" variant="contained" color="primary">
                {t('admin_manage_products')}
              </Button>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Button component={Link} to="/admin/materials" variant="contained" color="primary">
                {t('admin_manage_materials')}
              </Button>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Button component={Link} to="/admin/surveys" variant="contained" color="primary">
                {t('admin_manage_surveys')}
              </Button>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Button component={Link} to="/admin/translations" variant="contained" color="primary">
                {t('admin_manage_translations')}
              </Button>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
          <CardContent>
              <Button component={Link} to="/admin/ai-settings" variant="contained" color="primary">
                {t('admin_manage_ai_settings')}
              </Button>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* System Settings */}
      <div className="system-settings">
        <h2>{t('admin_system_settings')}</h2>
        <div className="settings-grid">
          <div className="setting-item">
            <h4>{t('admin_setting_user_registration')}</h4>
            <p>{formatWithPlaceholder(t('admin_setting_min_password'), 8)}</p>
            <p>{t('admin_setting_email_verification')}</p>
          </div>
        </div>
      </div>

      {/* Activity Logs */}
      <div className="activity-logs">
        <h2>{t('admin_activity_logs')}</h2>
        <div className="logs-actions">
          <Button variant="outlined" color="primary">
            {t('admin_activity_download')}
          </Button>
          <Button variant="text" color="primary">
            {t('admin_activity_view')}
          </Button>
        </div>
      </div>

      {/* Maintenance Section */}
      <div className="maintenance">
        <h2>{t('admin_maintenance')}</h2>
        <div className="maintenance-card">
          <p>{t('admin_maintenance_tips')}</p>
          <Button variant="contained" color="secondary">
            {t('admin_maintenance_mode')}
          </Button>
        </div>
      </div>
    </div>
  );
};

export default AdminDashboard;
