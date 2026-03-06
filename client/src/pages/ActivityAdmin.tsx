import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Container,
  Typography,
  Paper,
  Tabs,
  Tab,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  Chip,
  TextField,
  Switch,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
} from '@mui/icons-material';
import { activitiesAPI, activityCategoriesAPI } from '../services/api';
import ActivityForm from '../components/activities/ActivityForm';
import { useTranslation } from '../hooks/useTranslation';

interface Activity {
  id: string;
  name: string;
  description?: string;
  videoUrl?: string;
  introVideoUrl?: string;
  duration?: string;
  durationHours?: number;
  durationMinutes?: number;
  durationSeconds?: number;
  activityCategoryId?: string;
  activityCategory?: {
    id: string;
    name: string;
  };
  createdAt: string;
  updatedAt: string;
  isPublic?: boolean;
}

interface ActivityCategory {
  id: string;
  name: string;
  description?: string;
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`activity-admin-tabpanel-${index}`}
      aria-labelledby={`activity-admin-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

const ActivityAdmin: React.FC = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  const [activeTab, setActiveTab] = useState(0);
  const [activities, setActivities] = useState<Activity[]>([]);
  const [categories, setCategories] = useState<ActivityCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [categoryFormOpen, setCategoryFormOpen] = useState(false);
  const [categoryFormData, setCategoryFormData] = useState({
    name: '',
    description: '',
  });
  const [categoryFormErrors, setCategoryFormErrors] = useState<{ name?: string }>({});
  const [updatingPublicId, setUpdatingPublicId] = useState<string | null>(null);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);

      const [activitiesResponse, categoriesResponse] = await Promise.all([
        activitiesAPI.getActivities(),
        activityCategoriesAPI.getActivityCategories(),
      ]);

      const fetchedCategories = categoriesResponse.data.success
        ? categoriesResponse.data.data?.items || []
        : [];
      setCategories(fetchedCategories);

      if (activitiesResponse.data.success) {
        const list = activitiesResponse.data.data?.items || [];
        const enhancedActivities = list.map((activity: any) => {
          if (activity.duration && !activity.durationHours && !activity.durationMinutes && !activity.durationSeconds) {
            const durationParts = activity.duration.split(':');
            const [first, second, third] = durationParts;
            if (durationParts.length === 2) {
              const minutes = parseInt(first, 10);
              const seconds = parseInt(second, 10);
              if (!Number.isNaN(minutes) && !Number.isNaN(seconds)) {
                activity.durationHours = 0;
                activity.durationMinutes = minutes;
                activity.durationSeconds = seconds;
              }
            } else if (durationParts.length === 3) {
              const hours = parseInt(first, 10);
              const minutes = parseInt(second, 10);
              const seconds = parseInt(third, 10);
              if (!Number.isNaN(hours) && !Number.isNaN(minutes) && !Number.isNaN(seconds)) {
                activity.durationHours = hours;
                activity.durationMinutes = minutes;
                activity.durationSeconds = seconds;
              }
            }
          }
          if (activity.activityCategoryId && !activity.activityCategory) {
            const found = fetchedCategories.find(
              (c: ActivityCategory) => c.id === activity.activityCategoryId
            );
            return {
              ...activity,
              activityCategory: found ? { id: found.id, name: found.name } : activity.activityCategory,
            };
          }
          return activity;
        });
        setActivities(enhancedActivities);
      }
    } catch (err: any) {
      setError(t('activity_admin_error'));
    } finally {
      setLoading(false);
    }
  };

  const handleTogglePublicStatus = async (activity: Activity) => {
    try {
      setUpdatingPublicId(activity.id);
      setError(null);
      let durationPayload: { durationHours: number; durationMinutes: number; durationSeconds: number } | null = null;
      const hasDurationParts =
        activity.durationHours != null || activity.durationMinutes != null || activity.durationSeconds != null;

      if (hasDurationParts) {
        durationPayload = {
          durationHours: activity.durationHours ?? 0,
          durationMinutes: activity.durationMinutes ?? 0,
          durationSeconds: activity.durationSeconds ?? 0,
        };
      } else if (activity.duration) {
        const durationParts = activity.duration.split(':');
        if (durationParts.length === 2) {
          const minutes = parseInt(durationParts[0], 10);
          const seconds = parseInt(durationParts[1], 10);
          if (!Number.isNaN(minutes) && !Number.isNaN(seconds)) {
            durationPayload = { durationHours: 0, durationMinutes: minutes, durationSeconds: seconds };
          }
        } else if (durationParts.length === 3) {
          const hours = parseInt(durationParts[0], 10);
          const minutes = parseInt(durationParts[1], 10);
          const seconds = parseInt(durationParts[2], 10);
          if (!Number.isNaN(hours) && !Number.isNaN(minutes) && !Number.isNaN(seconds)) {
            durationPayload = { durationHours: hours, durationMinutes: minutes, durationSeconds: seconds };
          }
        }
      }

      const payload = {
        name: activity.name,
        description: activity.description ?? undefined,
        isPublic: !activity.isPublic,
        ...(durationPayload ?? {}),
      };
      await activitiesAPI.updateActivity(activity.id, payload);
      setActivities((prev) =>
        prev.map((item) =>
          item.id === activity.id ? { ...item, isPublic: !activity.isPublic } : item
        )
      );
    } catch (toggleError) {
      console.error('Failed to update activity visibility', toggleError);
      setError(t('activity_admin_public_update_error'));
    } finally {
      setUpdatingPublicId(null);
    }
  };

  const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
  };

  const handleCreateActivity = () => setFormOpen(true);

  const handleEditActivity = (activity: Activity) => {
    navigate(`/admin/activities/${activity.id}/edit`);
  };

  const handleDeleteActivity = async (activity: Activity) => {
    const confirmText = t('activity_admin_confirm_delete').replace('{0}', activity.name);
    if (!window.confirm(confirmText)) return;
    try {
      await activitiesAPI.deleteActivity(activity.id);
      await loadData();
    } catch (err: any) {
      setError(err?.message || t('activity_admin_error'));
    }
  };

  const handleFormClose = () => setFormOpen(false);

  const handleFormSuccess = async () => {
    setFormOpen(false);
    await loadData();
  };

  const handleCreateCategory = () => {
    setCategoryFormData({ name: '', description: '' });
    setCategoryFormErrors({});
    setCategoryFormOpen(true);
  };

  const handleCategoryFormClose = () => {
    setCategoryFormOpen(false);
    setCategoryFormData({ name: '', description: '' });
    setCategoryFormErrors({});
  };

  const handleCategoryFormSubmit = async () => {
    const errors: { name?: string } = {};
    if (!categoryFormData.name.trim()) {
      errors.name = t('activity_admin_category_validation');
    }
    if (Object.keys(errors).length > 0) {
      setCategoryFormErrors(errors);
      return;
    }

    try {
      await activityCategoriesAPI.createActivityCategory({
        name: categoryFormData.name.trim(),
        description: categoryFormData.description.trim() || undefined,
      });
      setCategoryFormOpen(false);
      setCategoryFormData({ name: '', description: '' });
      await loadData();
    } catch {
      setError(t('activity_admin_error'));
    }
  };

  const formatDuration = (activity: Activity) => {
    const h = activity.durationHours ?? 0;
    const m = activity.durationMinutes ?? 0;
    const s = activity.durationSeconds ?? 0;
    if (h === 0 && m === 0 && s === 0) return activity.duration || '-';
    if (h === 0) return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
    return `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}:${s
      .toString()
      .padStart(2, '0')}`;
  };

  const getCategoryName = (activity: Activity) => {
    if (activity.activityCategory?.name) return activity.activityCategory.name;
    const found = categories.find((c) => c.id === activity.activityCategoryId);
    return found?.name || '-';
  };

  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
          <Typography>{t('activity_admin_loading')}</Typography>
        </Box>
      </Container>
    );
  }

  if (error) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Box display="flex" flexDirection="column" alignItems="center" gap={2}>
          <Typography color="error">{error}</Typography>
          <Button variant="outlined" onClick={loadData}>
            {t('activity_admin_retry')}
          </Button>
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Typography variant="h4" gutterBottom>
        {t('activity_admin_title')}
      </Typography>

      <Paper sx={{ width: '100%', mb: 2 }}>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={activeTab} onChange={handleTabChange} aria-label="activity admin tabs">
            <Tab label={t('activity_admin_tab_activities')} id="activity-admin-tab-0" aria-controls="activity-admin-tabpanel-0" />
            <Tab label={t('activity_admin_tab_categories')} id="activity-admin-tab-1" aria-controls="activity-admin-tabpanel-1" />
          </Tabs>
        </Box>

        <TabPanel value={activeTab} index={0}>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={handleCreateActivity}
              sx={{ mb: 2 }}
            >
              {t('activity_admin_create')}
            </Button>
          </Box>

          {isMobile ? (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              {activities.map((activity) => (
                <Paper key={activity.id} sx={{ p: 2 }}>
                  <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', gap: 1 }}>
                    <Box sx={{ flex: 1 }}>
                      <Typography variant="subtitle1" fontWeight="medium">
                        {activity.name}
                      </Typography>
                      {activity.description && (
                        <Typography variant="body2" color="text.secondary">
                          {activity.description.length > 80
                            ? `${activity.description.substring(0, 80)}...`
                            : activity.description}
                        </Typography>
                      )}
                    </Box>
                    <Switch
                      checked={Boolean(activity.isPublic)}
                      onChange={() => handleTogglePublicStatus(activity)}
                      disabled={updatingPublicId === activity.id}
                      size="small"
                      color="success"
                      inputProps={{
                        'aria-label': activity.isPublic
                          ? t('activity_admin_public_label_public')
                          : t('activity_admin_public_label_private'),
                      }}
                    />
                  </Box>

                  <Box sx={{ mt: 1.5, display: 'grid', gridTemplateColumns: 'repeat(2, minmax(0, 1fr))', gap: 1 }}>
                    <Box>
                      <Typography variant="caption" color="text.secondary">
                        {t('activity_admin_category_label')}
                      </Typography>
                      <Typography variant="body2">
                        {getCategoryName(activity)}
                      </Typography>
                    </Box>
                    <Box>
                      <Typography variant="caption" color="text.secondary">
                        {t('activity_admin_duration_label')}
                      </Typography>
                      <Typography variant="body2">{formatDuration(activity)}</Typography>
                    </Box>
                    <Box>
                      <Typography variant="caption" color="text.secondary">
                        {t('activity_admin_created_at')}
                      </Typography>
                      <Typography variant="body2">{new Date(activity.createdAt).toLocaleDateString()}</Typography>
                    </Box>
                  </Box>

                  <Box sx={{ mt: 1.5, display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                    <Button
                      size="small"
                      variant="outlined"
                      startIcon={<EditIcon />}
                      onClick={() => handleEditActivity(activity)}
                    >
                      {t('activity_admin_edit')}
                    </Button>
                    <Button
                      size="small"
                      color="error"
                      variant="outlined"
                      startIcon={<DeleteIcon />}
                      onClick={() => handleDeleteActivity(activity)}
                    >
                      {t('activity_admin_delete')}
                    </Button>
                  </Box>
                </Paper>
              ))}
              {activities.length === 0 && (
                <Paper sx={{ p: 2 }}>
                  <Typography align="center">{t('activity_admin_no_activities')}</Typography>
                </Paper>
              )}
            </Box>
          ) : (
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>{t('activity_admin_tab_activities')}</TableCell>
                    <TableCell>{t('activity_admin_category_label')}</TableCell>
                    <TableCell>{t('activity_admin_duration_label')}</TableCell>
                    <TableCell>{t('activity_admin_public_status')}</TableCell>
                    <TableCell>{t('activity_admin_created_at')}</TableCell>
                    <TableCell align="right">{t('activity_admin_actions')}</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {activities.map((activity) => (
                    <TableRow key={activity.id} hover>
                      <TableCell>
                        <Typography variant="body1" fontWeight="medium">
                          {activity.name}
                        </Typography>
                        {activity.description && (
                          <Typography variant="body2" color="text.secondary">
                            {activity.description.length > 50
                              ? `${activity.description.substring(0, 50)}...`
                              : activity.description}
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>
                        {activity.activityCategory ? (
                          <Chip label={activity.activityCategory.name} size="small" variant="outlined" />
                        ) : (
                          <Typography variant="body2" color="text.secondary">
                            {getCategoryName(activity)}
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>{formatDuration(activity)}</TableCell>
                      <TableCell>
                        <Switch
                          checked={Boolean(activity.isPublic)}
                          onChange={() => handleTogglePublicStatus(activity)}
                          disabled={updatingPublicId === activity.id}
                          size="small"
                          color="success"
                          inputProps={{
                            'aria-label': activity.isPublic
                              ? t('activity_admin_public_label_public')
                              : t('activity_admin_public_label_private'),
                          }}
                        />
                      </TableCell>
                      <TableCell>{new Date(activity.createdAt).toLocaleDateString()}</TableCell>
                      <TableCell align="right">
                        <IconButton size="small" onClick={() => handleEditActivity(activity)} sx={{ mr: 1 }} title={t('activity_admin_edit')}>
                          <EditIcon />
                        </IconButton>
                        <IconButton size="small" color="error" onClick={() => handleDeleteActivity(activity)} title={t('activity_admin_delete')}>
                          <DeleteIcon />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))}
                  {activities.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={6} align="center">
                        {t('activity_admin_no_activities')}
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </TabPanel>

        <TabPanel value={activeTab} index={1}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6">{t('activity_admin_manage_categories')}</Typography>
            <Button variant="contained" startIcon={<AddIcon />} onClick={handleCreateCategory}>
              {t('activity_admin_category_create')}
            </Button>
          </Box>

          {isMobile ? (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              {categories.map((category) => (
                <Paper key={category.id} sx={{ p: 2 }}>
                  <Typography variant="subtitle1" fontWeight="medium">
                    {category.name}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {category.description || '-'}
                  </Typography>
                  <Box sx={{ mt: 1.5, display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
                    <IconButton size="small" title={t('activity_admin_edit')}>
                      <EditIcon />
                    </IconButton>
                    <IconButton size="small" color="error" title={t('activity_admin_delete')}>
                      <DeleteIcon />
                    </IconButton>
                  </Box>
                </Paper>
              ))}
              {categories.length === 0 && (
                <Paper sx={{ p: 2 }}>
                  <Typography align="center" color="text.secondary">
                    {t('activity_admin_no_activities')}
                  </Typography>
                </Paper>
              )}
            </Box>
          ) : (
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>{t('activity_admin_category_name')}</TableCell>
                    <TableCell>{t('activity_admin_category_description')}</TableCell>
                    <TableCell>{t('activity_admin_actions')}</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {categories.map((category) => (
                    <TableRow key={category.id} hover>
                      <TableCell>{category.name}</TableCell>
                      <TableCell>{category.description || '-'}</TableCell>
                      <TableCell align="right">
                        <IconButton size="small" title={t('activity_admin_edit')}>
                          <EditIcon />
                        </IconButton>
                        <IconButton size="small" color="error" title={t('activity_admin_delete')}>
                          <DeleteIcon />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))}
                  {categories.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={3} align="center" sx={{ py: 4 }}>
                        <Typography variant="body1" color="text.secondary">
                          {t('activity_admin_no_activities')}
                        </Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </TabPanel>
      </Paper>

      <Dialog open={formOpen} onClose={handleFormClose} maxWidth="md" fullWidth fullScreen={isMobile}>
        <DialogTitle>{t('activity_form_create_title')}</DialogTitle>
        <DialogContent>
          <ActivityForm
            activity={null}
            categories={categories}
            onSuccess={handleFormSuccess}
            onCancel={handleFormClose}
          />
        </DialogContent>
      </Dialog>

      <Dialog open={categoryFormOpen} onClose={handleCategoryFormClose} maxWidth="sm" fullWidth fullScreen={isMobile}>
        <DialogTitle>{t('activity_admin_category_create')}</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <TextField
              fullWidth
              label={t('activity_admin_category_name')}
              value={categoryFormData.name}
              onChange={(e) => setCategoryFormData((prev) => ({ ...prev, name: e.target.value }))}
              error={!!categoryFormErrors.name}
              helperText={categoryFormErrors.name || t('activity_admin_category_validation')}
              required
              sx={{ mb: 2 }}
            />
            <TextField
              fullWidth
              label={t('activity_admin_category_description')}
              value={categoryFormData.description}
              onChange={(e) => setCategoryFormData((prev) => ({ ...prev, description: e.target.value }))}
              multiline
              rows={3}
              sx={{ mb: 2 }}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCategoryFormClose}>{t('activity_admin_category_cancel')}</Button>
          <Button onClick={handleCategoryFormSubmit} variant="contained" disabled={!categoryFormData.name.trim()}>
            {t('activity_admin_category_save')}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default ActivityAdmin;
