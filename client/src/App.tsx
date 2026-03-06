import React from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter as Router, Routes, Route, useLocation } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import { AuthProvider } from './contexts/AuthContext';
import { LocalizationProvider } from './contexts/LocalizationContext';
import { useTranslation } from './hooks/useTranslation';
import BottomNavigation from './components/BottomNavigation';
import ProtectedRoute from './components/ProtectedRoute';
import AdminRoute from './components/AdminRoute';
import Home from './pages/Home';
import Login from './pages/Login';
import ForgotPassword from './pages/ForgotPassword';
import ResetPassword from './pages/ResetPassword';
import Profile from './pages/Profile';
import Settings from './pages/Settings';
import AdminDashboard from './pages/AdminDashboard';
import ActivityAdmin from './pages/ActivityAdmin';
import ActivityEditPage from './pages/ActivityEditPage';
import ActivityPage from './pages/ActivityPage';
import UserDashboard from './pages/UserDashboard';
import MaterialsAdmin from './pages/MaterialsAdmin';
import PersonaAdmin from './pages/PersonaAdmin';
import { ProductsOverview, ProductWizardDemo } from './components/products';
import PromptTemplates from './pages/PromptTemplates';
import {
  SurveyList,
  SurveyCreate,
  SurveyEdit,
  SurveyResults,
  PublicSurvey,
  VoteSuccess
} from './components/surveys';
import './App.css';
import LanguageSelector from './components/LanguageSelector';
import TranslationAdmin from './pages/TranslationAdmin';
import AdminAiSettings from './pages/AdminAiSettings';

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
  const { t } = useTranslation();
  const authPaths = ['/login', '/forgot-password', '/reset-password'];
  const isAuthPage = authPaths.includes(location.pathname);
  return (
    <div className="App">
      {/* Global loading indicator */}
      <div id="global-loading" className="global-loading">
        <div className="loading-spinner"></div>
        <p>{t('loading')}</p>
      </div>

      <div className="language-selector-wrapper">
        <LanguageSelector />
      </div>

      <main className={`main-content ${isAuthPage ? 'login-page' : ''}`}>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/login" element={<Login />} />
          <Route path="/forgot-password" element={<ForgotPassword />} />
          <Route path="/reset-password" element={<ResetPassword />} />
          <Route path="/profile" element={<ProtectedRoute><Profile /></ProtectedRoute>} />
          <Route path="/settings" element={<ProtectedRoute><Settings /></ProtectedRoute>} />
          <Route path="/admin" element={<AdminRoute><AdminDashboard /></AdminRoute>} />
          <Route path="/admin/activities" element={<AdminRoute><ActivityAdmin /></AdminRoute>} />
          <Route path="/admin/activities/:id/edit" element={<AdminRoute><ActivityEditPage /></AdminRoute>} />
          <Route path="/admin/materials" element={<AdminRoute><MaterialsAdmin /></AdminRoute>} />
          <Route path="/admin/personas" element={<AdminRoute><PersonaAdmin /></AdminRoute>} />
          <Route path="/admin/products" element={<AdminRoute><ProductsOverview /></AdminRoute>} />
          <Route path="/admin/product-wizard-demo" element={<AdminRoute><ProductWizardDemo /></AdminRoute>} />
          <Route path="/admin/surveys" element={<AdminRoute><SurveyList /></AdminRoute>} />
          <Route path="/admin/translations" element={<AdminRoute><TranslationAdmin /></AdminRoute>} />
          <Route path="/admin/ai-settings" element={<AdminRoute><AdminAiSettings /></AdminRoute>} />
          <Route path="/admin/prompts" element={<AdminRoute><PromptTemplates /></AdminRoute>} />
          <Route path="/admin/surveys/create" element={<AdminRoute><SurveyCreate /></AdminRoute>} />
          <Route path="/admin/surveys/:id/edit" element={<AdminRoute><SurveyEdit /></AdminRoute>} />
          <Route path="/admin/surveys/:id/results" element={<AdminRoute><SurveyResults /></AdminRoute>} />
          <Route path="/survey/:surveyId" element={<PublicSurvey />} />
          <Route path="/survey/:surveyId/success" element={<VoteSuccess />} />
          <Route path="/activity/:id" element={<ActivityPage />} />
          <Route path="/dashboard" element={<ProtectedRoute><UserDashboard /></ProtectedRoute>} />
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
          <LocalizationProvider>
            <Router>
              <AppContent />
            </Router>
          </LocalizationProvider>
        </AuthProvider>
      </QueryClientProvider>
    </ThemeProvider>
  );
}

export default App;
