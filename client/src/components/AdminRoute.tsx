import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import ProtectedRoute from './ProtectedRoute';

interface AdminRouteProps {
  children: React.ReactNode;
}

const AdminRoute: React.FC<AdminRouteProps> = ({ children }) => {
  const { user, isLoading, isAuthenticated } = useAuth();

  console.log('[AdminRoute] Checking permissions:', {
    isLoading,
    isAuthenticated,
    userId: user?.id,
    userRole: user?.role,
    userRoleType: typeof user?.role,
    userRoleString: String(user?.role),
    userRoleLower: String(user?.role).toLowerCase(),
    isAdminCheck: String(user?.role) === '1' || String(user?.role).toLowerCase() === 'admin'
  });

  if (isLoading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <p>Checking permissions...</p>
      </div>
    );
  }

  // If not authenticated, let ProtectedRoute handle the redirect to login
  if (!isAuthenticated) {
    console.log('[AdminRoute] User not authenticated, delegating to ProtectedRoute');
    return <ProtectedRoute>{children}</ProtectedRoute>;
  }

  // If authenticated but not admin, redirect to user dashboard
  if (!user || !(String(user.role) === '1' || String(user.role).toLowerCase() === 'admin')) {
    console.log('[AdminRoute] User is not admin, redirecting to dashboard');
    return <Navigate to="/dashboard" replace />;
  }

  // User is authenticated and is admin, render the protected content
  console.log('[AdminRoute] User is admin, rendering protected content');
  return <ProtectedRoute>{children}</ProtectedRoute>;
};

export default AdminRoute;