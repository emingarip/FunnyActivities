import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './BottomNavigation.css';
import { useTranslation } from '../hooks/useTranslation';

interface NavItem {
  path: string;
  icon: React.ReactElement;
  label: string;
  isCenter?: boolean;
}

const BottomNavigation: React.FC = () => {
  const location = useLocation();
  const { user } = useAuth();
  const { t } = useTranslation();

  const baseNavItems: NavItem[] = [
    {
      path: '/',
      icon: (
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
          <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/>
          <polyline points="9,22 9,12 15,12 15,22"/>
        </svg>
      ),
      label: t('nav_home')
    },
    {
      path: '/dashboard',
      icon: (
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
          <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
        </svg>
      ),
      label: t('nav_activity')
    },
    {
      path: '/profile',
      icon: (
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
          <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
          <circle cx="12" cy="7" r="4"/>
        </svg>
      ),
      label: t('nav_profile')
    }
  ];

  const adminNavItem: NavItem = {
    path: '/admin',
    icon: (
      <svg width="28" height="28" viewBox="0 0 24 24" fill="currentColor">
        <rect x="3" y="3" width="18" height="18" rx="4"/>
        <rect x="9" y="9" width="6" height="6" rx="1" fill="white"/>
      </svg>
    ),
    label: t('nav_dashboard'),
    isCenter: true
  };

  const productsNavItem: NavItem = {
    path: '/admin/products',
    icon: (
      <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <path d="M20 7h-4V4c0-1.1-.9-2-2-2H6c-1.1 0-2 .9-2 2v16c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V9c0-1.1-.9-2-2-2z"/>
        <path d="M9 12l2 2 4-4"/>
      </svg>
    ),
    label: t('nav_products')
  };

  const surveysNavItem: NavItem = {
    path: '/admin/surveys',
    icon: (
      <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <path d="M9 11H5a2 2 0 0 0-2 2v7a2 2 0 0 0 2 2h4"/>
        <path d="M9 7v4"/>
        <path d="M13 16l-3-3 3-3"/>
        <path d="M16 13h4a2 2 0 0 0 2-2V4a2 2 0 0 0-2-2h-4"/>
      </svg>
    ),
    label: 'Surveys'
  };

  // Only show admin navigation item if user is admin
  const isAdmin = user?.role && (user.role.toString().toLowerCase() === 'admin' || user.role.toString() === '1');
  const navItems = isAdmin
    ? [...baseNavItems.slice(0, 2), adminNavItem, productsNavItem, surveysNavItem, ...baseNavItems.slice(2)]
    : baseNavItems;

  const isActive = (path: string) => {
    return location.pathname === path;
  };

  return (
    <nav className="modern-bottom-nav" role="navigation" aria-label={t('nav_home')}>
      <div className="nav-container">
        {navItems.map((item) => (
          <Link
            key={item.path}
            to={item.path}
            className={`nav-item ${isActive(item.path) ? 'active' : ''} ${item.isCenter ? 'center-item' : ''}`}
            aria-label={item.label}
            role="button"
            tabIndex={0}
          >
            <div className="nav-icon" aria-hidden="true">
              {item.icon}
            </div>
            <span className="nav-label sr-only">{item.label}</span>
          </Link>
        ))}
      </div>
    </nav>
  );
};

export default BottomNavigation;
