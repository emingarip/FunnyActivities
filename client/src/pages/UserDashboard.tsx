import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { userAPI } from '../services/api';
import './UserDashboard.css';

interface Activity {
  id: string;
  type: string;
  description: string;
  date: string;
  status: 'completed' | 'pending' | 'failed';
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

  useEffect(() => {
    if (isAuthenticated) {
      fetchDashboardData();
    }
  }, [isAuthenticated]);

  const fetchDashboardData = async () => {
    try {
      setLoading(true);
      setError(null);

      // Fetch user profile data
      await userAPI.getProfile();
      // Note: The API might not have activities/notifications endpoints yet
      // For now, we'll use mock data for activities and notifications

      setActivities([
        {
          id: '1',
          type: 'Login',
          description: 'Successfully logged in',
          date: new Date().toISOString(),
          status: 'completed'
        },
        {
          id: '2',
          type: 'Profile Update',
          description: 'Updated profile information',
          date: new Date(Date.now() - 86400000).toISOString(), // 1 day ago
          status: 'completed'
        }
      ]);

      setNotifications([
        {
          id: '1',
          title: 'Welcome!',
          message: 'Welcome to our platform. Your account has been successfully created.',
          date: new Date().toISOString(),
          read: false,
          type: 'success'
        }
      ]);
    } catch (err: any) {
      setError(err.message || 'Failed to load dashboard data');
    } finally {
      setLoading(false);
    }
  };

  const markNotificationAsRead = (id: string) => {
    setNotifications(prev =>
      prev.map(notification =>
        notification.id === id ? { ...notification, read: true } : notification
      )
    );
  };

  if (loading) {
    return (
      <div className="page-container">
        <div className="loading">Loading dashboard...</div>
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
        <h1>User Dashboard</h1>
        <button
          onClick={logout}
          className="logout-btn"
        >
          Logout
        </button>
      </div>

      {/* Personal Information */}
      <div className="personal-info">
        <h2>Personal Information</h2>
        {user && (
          <div className="info-card">
            <div className="info-row">
              <span className="label">Name:</span>
              <span className="value">{user.firstName} {user.lastName}</span>
            </div>
            <div className="info-row">
              <span className="label">Email:</span>
              <span className="value">{user.email}</span>
            </div>
            <div className="info-row">
              <span className="label">Role:</span>
              <span className="value">{user.role}</span>
            </div>
            <div className="info-row">
              <span className="label">Member Since:</span>
              <span className="value">{new Date(user.createdAt).toLocaleDateString()}</span>
            </div>
          </div>
        )}
      </div>

      {/* Recent Activities */}
      <div className="recent-activities">
        <h2>Recent Activities</h2>
        <div className="activities-list">
          {activities.map((activity) => (
            <div key={activity.id} className="activity-item">
              <div className="activity-icon">
                {activity.type === 'Login' && '🔐'}
                {activity.type === 'Profile Update' && '👤'}
                {activity.type === 'Password Change' && '🔒'}
              </div>
              <div className="activity-content">
                <h4>{activity.type}</h4>
                <p>{activity.description}</p>
                <span className="activity-date">
                  {new Date(activity.date).toLocaleString()}
                </span>
              </div>
              <div className={`activity-status ${activity.status}`}>
                {activity.status}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Notifications */}
      <div className="notifications">
        <h2>Notifications</h2>
        <div className="notifications-list">
          {notifications.map((notification) => (
            <div
              key={notification.id}
              className={`notification-item ${notification.read ? 'read' : 'unread'} ${notification.type}`}
            >
              <div className="notification-header">
                <h4>{notification.title}</h4>
                <span className="notification-date">
                  {new Date(notification.date).toLocaleString()}
                </span>
              </div>
              <p>{notification.message}</p>
              {!notification.read && (
                <button
                  className="mark-read-btn"
                  onClick={() => markNotificationAsRead(notification.id)}
                >
                  Mark as Read
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