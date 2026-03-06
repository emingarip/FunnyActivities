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
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  IconButton,
  InputAdornment,
} from '@mui/material';
import {
  LockReset as LockResetIcon,
  Visibility as VisibilityIcon,
  VisibilityOff as VisibilityOffIcon,
} from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';
import { useTranslation } from '../hooks/useTranslation';
import { userAPI } from '../services/api';

const Profile: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [passwordDialogOpen, setPasswordDialogOpen] = useState(false);
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [passwordLoading, setPasswordLoading] = useState(false);
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const [showCurrentPassword, setShowCurrentPassword] = useState(false);
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [snackbar, setSnackbar] = useState({
    open: false,
    message: '',
    severity: 'success' as 'success' | 'error',
  });

  const passwordMismatch = Boolean(confirmPassword) && newPassword !== confirmPassword;
  const passwordTooShort = Boolean(newPassword) && newPassword.length < 8;
  const passwordSameAsCurrent = Boolean(currentPassword) && Boolean(newPassword) && currentPassword === newPassword;
  const canSubmitPassword =
    Boolean(currentPassword) &&
    Boolean(newPassword) &&
    Boolean(confirmPassword) &&
    !passwordMismatch &&
    !passwordTooShort &&
    !passwordSameAsCurrent &&
    !passwordLoading;

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

  const resetPasswordDialog = () => {
    setCurrentPassword('');
    setNewPassword('');
    setConfirmPassword('');
    setPasswordError(null);
    setShowCurrentPassword(false);
    setShowNewPassword(false);
    setShowConfirmPassword(false);
  };

  const handleOpenPasswordDialog = () => {
    resetPasswordDialog();
    setPasswordDialogOpen(true);
  };

  const handleClosePasswordDialog = () => {
    if (passwordLoading) {
      return;
    }

    setPasswordDialogOpen(false);
    resetPasswordDialog();
  };

  const handleChangePassword = async () => {
    if (!currentPassword || !newPassword || !confirmPassword) {
      setPasswordError(t('profile_password_required'));
      return;
    }

    if (newPassword !== confirmPassword) {
      setPasswordError(t('profile_password_mismatch'));
      return;
    }

    if (newPassword.length < 8) {
      setPasswordError(t('profile_password_too_short'));
      return;
    }

    if (currentPassword === newPassword) {
      setPasswordError(t('profile_password_same'));
      return;
    }

    try {
      setPasswordLoading(true);
      setPasswordError(null);
      await userAPI.changePassword({
        currentPassword,
        newPassword,
      });

      setPasswordDialogOpen(false);
      resetPasswordDialog();
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

      <Card variant="outlined" sx={{ mt: 3 }}>
        <CardContent
          sx={{
            p: 3,
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: { xs: 'flex-start', sm: 'center' },
            gap: 2,
            flexDirection: { xs: 'column', sm: 'row' },
          }}
        >
          <Box>
            <Typography variant="h6" gutterBottom>
              {t('profile_security_title')}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {t('profile_password_description')}
            </Typography>
          </Box>
          <Button
            variant="contained"
            startIcon={<LockResetIcon />}
            onClick={handleOpenPasswordDialog}
          >
            {t('profile_password_open')}
          </Button>
        </CardContent>
      </Card>

      <Dialog
        open={passwordDialogOpen}
        onClose={handleClosePasswordDialog}
        fullWidth
        maxWidth="xs"
      >
        <DialogTitle>{t('profile_password_title')}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            <Typography variant="body2" color="text.secondary">
              {t('profile_password_hint')}
            </Typography>

            {passwordError && (
              <Alert severity="error">{passwordError}</Alert>
            )}

            <TextField
              label={t('profile_password_current')}
              type={showCurrentPassword ? 'text' : 'password'}
              value={currentPassword}
              onChange={(event) => setCurrentPassword(event.target.value)}
              fullWidth
              autoFocus
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      onClick={() => setShowCurrentPassword((value) => !value)}
                      edge="end"
                    >
                      {showCurrentPassword ? <VisibilityOffIcon /> : <VisibilityIcon />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />
            <TextField
              label={t('profile_password_new')}
              type={showNewPassword ? 'text' : 'password'}
              value={newPassword}
              onChange={(event) => setNewPassword(event.target.value)}
              fullWidth
              error={passwordTooShort || passwordSameAsCurrent}
              helperText={
                passwordTooShort
                  ? t('profile_password_too_short')
                  : passwordSameAsCurrent
                    ? t('profile_password_same')
                    : ' '
              }
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      onClick={() => setShowNewPassword((value) => !value)}
                      edge="end"
                    >
                      {showNewPassword ? <VisibilityOffIcon /> : <VisibilityIcon />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />
            <TextField
              label={t('profile_password_confirm')}
              type={showConfirmPassword ? 'text' : 'password'}
              value={confirmPassword}
              onChange={(event) => setConfirmPassword(event.target.value)}
              fullWidth
              error={passwordMismatch}
              helperText={passwordMismatch ? t('profile_password_mismatch') : ' '}
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      onClick={() => setShowConfirmPassword((value) => !value)}
                      edge="end"
                    >
                      {showConfirmPassword ? <VisibilityOffIcon /> : <VisibilityIcon />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 3 }}>
          <Button onClick={handleClosePasswordDialog} disabled={passwordLoading}>
            {t('profile_password_cancel')}
          </Button>
          <Button
            variant="contained"
            onClick={handleChangePassword}
            disabled={!canSubmitPassword}
            startIcon={passwordLoading ? <CircularProgress size={18} color="inherit" /> : undefined}
          >
            {passwordLoading ? t('profile_password_submitting') : t('profile_password_submit')}
          </Button>
        </DialogActions>
      </Dialog>

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
