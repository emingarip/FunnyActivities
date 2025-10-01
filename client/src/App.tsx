import React from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter as Router, Routes, Route, useLocation } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import { AuthProvider } from './contexts/AuthContext';
import BottomNavigation from './components/BottomNavigation';
import ProtectedRoute from './components/ProtectedRoute';
import AdminRoute from './components/AdminRoute';
import Home from './pages/Home';
import Login from './pages/Login';
import Profile from './pages/Profile';
import Settings from './pages/Settings';
import AdminDashboard from './pages/AdminDashboard';
import ActivityAdmin from './pages/ActivityAdmin';
import ActivityPage from './pages/ActivityPage';
import UserDashboard from './pages/UserDashboard';
import Wallet from './pages/Wallet';
import MaterialsAdmin from './pages/MaterialsAdmin';
import { ProductsOverview, ProductWizardDemo } from './components/products';
import {
  SurveyList,
  SurveyCreate,
  SurveyEdit,
  SurveyResults,
  PublicSurvey,
  VoteSuccess
} from './components/surveys';
import './App.css';

// Create Material-UI theme
const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
});

function AppContent() {
  const location = useLocation();
  return (
    <div className="App">
      {/* Global loading indicator */}
      <div id="global-loading" className="global-loading">
        <div className="loading-spinner"></div>
        <p>Loading...</p>
      </div>

      <main className={`main-content ${location.pathname === '/login' ? 'login-page' : ''}`}>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/login" element={<Login />} />
          <Route path="/profile" element={<ProtectedRoute><Profile /></ProtectedRoute>} />
          <Route path="/settings" element={<ProtectedRoute><Settings /></ProtectedRoute>} />
          <Route path="/admin" element={<AdminRoute><AdminDashboard /></AdminRoute>} />
          <Route path="/admin/activities" element={<AdminRoute><ActivityAdmin /></AdminRoute>} />
          <Route path="/admin/materials" element={<AdminRoute><MaterialsAdmin /></AdminRoute>} />
          <Route path="/admin/products" element={<AdminRoute><ProductsOverview /></AdminRoute>} />
          <Route path="/admin/product-wizard-demo" element={<AdminRoute><ProductWizardDemo /></AdminRoute>} />
          <Route path="/admin/surveys" element={<AdminRoute><SurveyList /></AdminRoute>} />
          <Route path="/admin/surveys/create" element={<AdminRoute><SurveyCreate /></AdminRoute>} />
          <Route path="/admin/surveys/:id/edit" element={<AdminRoute><SurveyEdit /></AdminRoute>} />
          <Route path="/admin/surveys/:id/results" element={<AdminRoute><SurveyResults /></AdminRoute>} />
          <Route path="/survey/:surveyId" element={<PublicSurvey />} />
          <Route path="/survey/:surveyId/success" element={<VoteSuccess />} />
          <Route path="/activity/:id" element={<ActivityPage />} />
          <Route path="/dashboard" element={<ProtectedRoute><UserDashboard /></ProtectedRoute>} />
          <Route path="/wallet" element={<ProtectedRoute><Wallet /></ProtectedRoute>} />
        </Routes>
      </main>
      <BottomNavigation />
    </div>
  );
}

function App() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 30000, // 30 seconds
      },
    },
  });

  return (
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
          <Router>
            <AppContent />
          </Router>
        </AuthProvider>
      </QueryClientProvider>
    </ThemeProvider>
  );
}

export default App;
