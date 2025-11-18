import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { userAPI } from '../services/api';
import './UserDashboard.css';
import { useTranslation } from '../hooks/useTranslation';

type ActivityStatus = 'completed' | 'pending' | 'failed';

type ActivityTypeKey =
  | 'dashboard_activity_login'
  | 'dashboard_activity_profile_update'
  | 'dashboard_activity_password_change';

interface Activity {
  id: string;
  type: ActivityTypeKey;
  description: string;
  date: string;
  status: ActivityStatus;
}

interface Notification {
  id: string;
  title: string;
  message: string;
  date: string;
  read: boolean;
  type: 'info' | 'warning' | 'success';
}

const UserDashboard: React.FC = () => {
  const { user, logout, isAuthenticated } = useAuth();
  const [activities, setActivities] = useState<Activity[]>([]);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { t, locale } = useTranslation();

  useEffect(() => {
    if (isAuthenticated) {
      fetchDashboardData();
    }
  }, [isAuthenticated]);

  const fetchDashboardData = async () => {
    try {
      setLoading(true);
      setError(null);

      await userAPI.getProfile();

      setActivities([
        {
          id: '1',
          type: 'dashboard_activity_login',
          description: t('dashboard_activity_login'),
          date: new Date().toISOString(),
          status: 'completed',
        },
        {
          id: '2',
          type: 'dashboard_activity_profile_update',
          description: t('dashboard_activity_profile_update'),
          date: new Date(Date.now() - 86400000).toISOString(),
          status: 'completed',
        },
      ]);

      setNotifications([
        {
          id: '1',
          title: t('dashboard_title'),
          message: t('dashboard_activity_login'),
          date: new Date().toISOString(),
          read: false,
          type: 'success',
        },
      ]);
    } catch (err: any) {
      setError(err.message || t('dashboard_error'));
    } finally {
      setLoading(false);
    }
  };

  const markNotificationAsRead = (id: string) => {
    setNotifications((prev) =>
      prev.map((notification) =>
        notification.id === id ? { ...notification, read: true } : notification
      )
    );
  };

  const formatDateTime = (dateString: string) =>
    new Date(dateString).toLocaleString(locale === 'tr' ? 'tr-TR' : 'en-US');

  const formatDate = (dateString: string) =>
    new Date(dateString).toLocaleDateString(locale === 'tr' ? 'tr-TR' : 'en-US');

  if (loading) {
    return (
      <div className="page-container">
        <div className="loading">{t('dashboard_loading')}</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="page-container">
        <div className="error">{error}</div>
      </div>
    );
  }

  return (
    <div className="page-container">
      <div className="dashboard-header">
        <h1>{t('dashboard_title')}</h1>
        <button onClick={logout} className="logout-btn">
          {t('dashboard_logout')}
        </button>
      </div>

      <div className="personal-info">
        <h2>{t('dashboard_personal_info')}</h2>
        {user && (
          <div className="info-card">
            <div className="info-row">
              <span className="label">{t('dashboard_name')}</span>
              <span className="value">
                {user.firstName} {user.lastName}
              </span>
            </div>
            <div className="info-row">
              <span className="label">{t('dashboard_email')}</span>
              <span className="value">{user.email}</span>
            </div>
            <div className="info-row">
              <span className="label">{t('dashboard_role')}</span>
              <span className="value">{user.role}</span>
            </div>
            <div className="info-row">
              <span className="label">{t('dashboard_member_since')}</span>
              <span className="value">{formatDate(user.createdAt)}</span>
            </div>
          </div>
        )}
      </div>

      <div className="recent-activities">
        <h2>{t('dashboard_recent_activities')}</h2>
        <div className="activities-list">
          {activities.map((activity) => (
            <div key={activity.id} className="activity-item">
              <div className="activity-icon">
                {activity.type === 'dashboard_activity_login' && '🔑'}
                {activity.type === 'dashboard_activity_profile_update' && '👤'}
                {activity.type === 'dashboard_activity_password_change' && '🔒'}
              </div>
              <div className="activity-content">
                <h4>{t(activity.type)}</h4>
                <p>{activity.description}</p>
                <span className="activity-date">{formatDateTime(activity.date)}</span>
              </div>
              <div className={`activity-status ${activity.status}`}>
                {t(`status_${activity.status}`)}
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="notifications">
        <h2>{t('dashboard_notifications')}</h2>
        <div className="notifications-list">
          {notifications.map((notification) => (
            <div
              key={notification.id}
              className={`notification-item ${notification.read ? 'read' : 'unread'} ${notification.type}`}
            >
              <div className="notification-header">
                <h4>{notification.title}</h4>
                <span className="notification-date">{formatDateTime(notification.date)}</span>
              </div>
              <p>{notification.message}</p>
              {!notification.read && (
                <button
                  className="mark-read-btn"
                  onClick={() => markNotificationAsRead(notification.id)}
                >
                  {t('dashboard_mark_read')}
                </button>
              )}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default UserDashboard;
