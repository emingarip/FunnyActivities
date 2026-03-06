import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Card,
  CardContent,
  Typography,
  Box,
  Button,
  Snackbar,
  Alert,
  Stack,
  TextField,
  Divider,
  CircularProgress,
} from '@mui/material';
import { useAuth } from '../contexts/AuthContext';
import { useTranslation } from '../hooks/useTranslation';
import { userAPI } from '../services/api';

const Profile: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [passwordLoading, setPasswordLoading] = useState(false);
  const [snackbar, setSnackbar] = useState({
    open: false,
    message: '',
    severity: 'success' as 'success' | 'error',
  });

  const handleLogout = async () => {
    try {
      logout();
      setSnackbar({
        open: true,
        message: t('profile_logout_success'),
        severity: 'success',
      });
      // Redirect to login page after successful logout
      navigate('/login');
    } catch (error) {
      console.error('Logout error:', error);
      setSnackbar({
        open: true,
        message: t('profile_logout_error'),
        severity: 'error',
      });
    }
  };

  const handleCloseSnackbar = () => {
    setSnackbar(prev => ({ ...prev, open: false }));
  };

  const handleChangePassword = async () => {
    if (!currentPassword || !newPassword || !confirmPassword) {
      setSnackbar({
        open: true,
        message: t('profile_password_required'),
        severity: 'error',
      });
      return;
    }

    if (newPassword !== confirmPassword) {
      setSnackbar({
        open: true,
        message: t('profile_password_mismatch'),
        severity: 'error',
      });
      return;
    }

    try {
      setPasswordLoading(true);
      await userAPI.changePassword({
        currentPassword,
        newPassword,
      });

      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
      setSnackbar({
        open: true,
        message: t('profile_password_success'),
        severity: 'success',
      });
    } catch (error: any) {
      setSnackbar({
        open: true,
        message: error?.message || t('profile_password_error'),
        severity: 'error',
      });
    } finally {
      setPasswordLoading(false);
    }
  };

  if (!user) {
    return (
      <Container maxWidth="md" sx={{ mt: 4 }}>
        <Typography variant="h6">{t('profile_login_prompt')}</Typography>
      </Container>
    );
  }

  return (
    <Container maxWidth="md" sx={{ mt: 4 }}>
      <Card>
        <CardContent sx={{ p: 3 }}>
          <Typography variant="h5" gutterBottom>
            {t('profile_title')}
          </Typography>
          <Box sx={{ mt: 2 }}>
            <Typography variant="body1" sx={{ mb: 1 }}>
              <strong>{t('profile_name_label')}</strong> {user.firstName} {user.lastName}
            </Typography>
            <Typography variant="body1" sx={{ mb: 3 }}>
              <strong>{t('profile_email_label')}</strong> {user.email}
            </Typography>
            <Divider sx={{ my: 3 }} />
            <Typography variant="h6" sx={{ mb: 2 }}>
              {t('profile_password_title')}
            </Typography>
            <Stack spacing={2} sx={{ maxWidth: 420 }}>
              <TextField
                label={t('profile_password_current')}
                type="password"
                value={currentPassword}
                onChange={(event) => setCurrentPassword(event.target.value)}
                fullWidth
              />
              <TextField
                label={t('profile_password_new')}
                type="password"
                value={newPassword}
                onChange={(event) => setNewPassword(event.target.value)}
                fullWidth
              />
              <TextField
                label={t('profile_password_confirm')}
                type="password"
                value={confirmPassword}
                onChange={(event) => setConfirmPassword(event.target.value)}
                fullWidth
              />
              <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', flexWrap: 'wrap' }}>
                <Button
                  variant="contained"
                  onClick={handleChangePassword}
                  disabled={passwordLoading}
                >
                  {passwordLoading ? t('profile_password_submitting') : t('profile_password_submit')}
                </Button>
                {passwordLoading && <CircularProgress size={20} />}
              </Box>
            </Stack>
            <Button
              variant="outlined"
              color="secondary"
              onClick={handleLogout}
              sx={{ mt: 1 }}
            >
              {t('profile_logout')}
            </Button>
          </Box>
        </CardContent>
      </Card>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={6000}
        onClose={handleCloseSnackbar}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'left' }}
      >
        <Alert
          onClose={handleCloseSnackbar}
          severity={snackbar.severity}
          sx={{ width: '100%' }}
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Container>
  );
};

export default Profile;
