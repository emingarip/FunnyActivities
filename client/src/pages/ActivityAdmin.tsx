import React, { useState, useEffect } from 'react';
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
  Alert,
  CircularProgress,
  TextField,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  PlayArrow as PlayIcon,
  VideoLibrary as VideoIcon,
} from '@mui/icons-material';
import { activitiesAPI, activityCategoriesAPI } from '../services/api';
import ActivityForm from '../components/activities/ActivityForm';

interface Activity {
  id: string;
  name: string;
  description?: string;
  videoUrl?: string;
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
  console.log('[ActivityAdmin] Component rendered');

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

  useEffect(() => {
    console.log('[ActivityAdmin] useEffect triggered, calling loadData');
    loadData();
  }, []);

  const loadData = async () => {
    console.log('[ActivityAdmin] loadData called');
    try {
      setLoading(true);
      setError(null);
      console.log('[ActivityAdmin] Making API calls...');

      const [activitiesResponse, categoriesResponse] = await Promise.all([
        activitiesAPI.getActivities(),
        activityCategoriesAPI.getActivityCategories(),
      ]);

      console.log('[ActivityAdmin] Activities response:', activitiesResponse);
      console.log('[ActivityAdmin] Categories response:', categoriesResponse);

      if (activitiesResponse.data.success) {
        const activities = activitiesResponse.data.data?.items || [];
        console.log('[ActivityAdmin] Activities loaded:', activities.length);
        console.log('[ActivityAdmin] Full activities response:', activitiesResponse.data);
        console.log('[ActivityAdmin] First activity from API:', activities[0]);
        console.log('[ActivityAdmin] Activities response data structure:', activitiesResponse.data.data);

        // Check if the data structure is nested differently
        if (activities.length > 0) {
          console.log('[ActivityAdmin] Checking activity properties:');
          const firstActivity = activities[0];
          Object.keys(firstActivity).forEach(key => {
            console.log(`[ActivityAdmin] ${key}:`, firstActivity[key]);
          });

          // Check for nested properties that might contain the data
          console.log('[ActivityAdmin] Checking for nested activityCategory:');
          if (firstActivity.activityCategory) {
            console.log('[ActivityAdmin] activityCategory object:', firstActivity.activityCategory);
          }

          // Check if duration might be in a different field
          console.log('[ActivityAdmin] Checking for duration in other fields:');
          ['duration', 'totalDuration', 'time', 'length', 'durationInSeconds', 'totalTime', 'activityDuration'].forEach(field => {
            if (firstActivity[field]) {
              console.log(`[ActivityAdmin] Found ${field}:`, firstActivity[field]);
            }
          });

          // Check for any numeric fields that might represent duration
          console.log('[ActivityAdmin] Checking for numeric fields that might be duration:');
          Object.keys(firstActivity).forEach(key => {
            const value = firstActivity[key];
            if (typeof value === 'number' && value > 0 && value < 86400) { // Less than 24 hours in seconds
              console.log(`[ActivityAdmin] Potential duration field ${key}:`, value, 'seconds');
            }
          });
        }

        // Enhance activities with category names if only IDs are available
        const enhancedActivities = activities.map((activity: any) => {
          if (activity.activityCategoryId && !activity.activityCategory) {
            console.log('[ActivityAdmin] Activity has categoryId but no category object:', activity.activityCategoryId);
          }
          return activity;
        });

        setActivities(enhancedActivities);
      }

      if (categoriesResponse.data.success) {
        setCategories(categoriesResponse.data.data?.items || []);
        console.log('[ActivityAdmin] Categories loaded:', categoriesResponse.data.data?.items?.length || 0);
      }
    } catch (err: any) {
      console.error('[ActivityAdmin] Error loading data:', err);
      setError('Failed to load activities and categories');
    } finally {
      setLoading(false);
      console.log('[ActivityAdmin] loadData completed');
    }
  };

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
  };

  const handleCreateActivity = () => {
    setFormOpen(true);
  };

  const handleEditActivity = (activity: Activity) => {
    navigate(`/admin/activities/${activity.id}/edit`);
  };

  const handleDeleteActivity = async (activity: Activity) => {
    if (!window.confirm(`Are you sure you want to delete "${activity.name}"?`)) {
      return;
    }

    try {
      await activitiesAPI.deleteActivity(activity.id);
      await loadData(); // Refresh the list
    } catch (err: any) {
      console.error('Error deleting activity:', err);
      setError('Failed to delete activity');
    }
  };

  const handleFormClose = () => {
    setFormOpen(false);
  };

  const handleFormSuccess = async () => {
    setFormOpen(false);
    await loadData(); // Refresh the list
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
    // Validation
    const errors: { name?: string } = {};
    if (!categoryFormData.name.trim()) {
      errors.name = 'Category name is required';
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
      await loadData(); // Refresh the list
    } catch (err: any) {
      console.error('Error creating category:', err);
      setError('Failed to create category');
    }
  };

  const extractDuration = (activity: any) => {
    console.log('[ActivityAdmin] extractDuration called for activity:', activity.name);
    console.log('[ActivityAdmin] Full activity object:', activity);

    // Try multiple possible structures for duration
    let hours = 0, minutes = 0, seconds = 0;

    // Structure 1: Separate fields (current expected structure)
    if (activity.durationHours !== undefined && activity.durationHours !== null) {
      hours = activity.durationHours;
      console.log('[ActivityAdmin] Found durationHours:', hours);
    }
    if (activity.durationMinutes !== undefined && activity.durationMinutes !== null) {
      minutes = activity.durationMinutes;
      console.log('[ActivityAdmin] Found durationMinutes:', minutes);
    }
    if (activity.durationSeconds !== undefined && activity.durationSeconds !== null) {
      seconds = activity.durationSeconds;
      console.log('[ActivityAdmin] Found durationSeconds:', seconds);
    }

    // Structure 2: Nested duration object
    if ((activity as any).duration) {
      const duration = (activity as any).duration;
      console.log('[ActivityAdmin] Found duration object:', duration);

      // Handle string format like "00:07:00"
      if (typeof duration === 'string') {
        console.log('[ActivityAdmin] Duration is a string, parsing:', duration);
        const parts = duration.split(':');
        if (parts.length === 3) {
          hours = parseInt(parts[0]) || 0;
          minutes = parseInt(parts[1]) || 0;
          seconds = parseInt(parts[2]) || 0;
          console.log('[ActivityAdmin] Parsed duration string:', { hours, minutes, seconds });
        } else if (parts.length === 2) {
          // Handle "MM:SS" format
          hours = 0;
          minutes = parseInt(parts[0]) || 0;
          seconds = parseInt(parts[1]) || 0;
          console.log('[ActivityAdmin] Parsed duration string (MM:SS):', { hours, minutes, seconds });
        }
      } else {
        // Handle object format
        if (duration.hours !== undefined) hours = duration.hours;
        if (duration.minutes !== undefined) minutes = duration.minutes;
        if (duration.seconds !== undefined) seconds = duration.seconds;
      }
    }

    // Structure 3: Single duration field in seconds
    if (activity.duration && typeof activity.duration === 'number') {
      const totalSeconds = activity.duration;
      console.log('[ActivityAdmin] Found single duration field:', totalSeconds, 'seconds');
      hours = Math.floor(totalSeconds / 3600);
      minutes = Math.floor((totalSeconds % 3600) / 60);
      seconds = totalSeconds % 60;
    }

    // Structure 4: Check for other possible duration field names
    const possibleDurationFields = ['totalDuration', 'time', 'length', 'durationInSeconds', 'totalTime', 'activityDuration'];
    for (const field of possibleDurationFields) {
      if (activity[field] && typeof activity[field] === 'number') {
        const totalSeconds = activity[field];
        console.log(`[ActivityAdmin] Found duration in ${field}:`, totalSeconds, 'seconds');
        hours = Math.floor(totalSeconds / 3600);
        minutes = Math.floor((totalSeconds % 3600) / 60);
        seconds = totalSeconds % 60;
        break;
      }
    }

    console.log('[ActivityAdmin] Final extracted duration:', { hours, minutes, seconds });
    return { hours, minutes, seconds };
  };

  const getCategoryName = (activityCategoryId: string | undefined): string => {
    if (!activityCategoryId) return 'No category';

    const category = categories.find(cat => cat.id === activityCategoryId);
    if (category) {
      console.log('[ActivityAdmin] Found category name for ID', activityCategoryId, ':', category.name);
      return category.name;
    }

    console.log('[ActivityAdmin] Category not found for ID:', activityCategoryId);
    return `Category ID: ${activityCategoryId}`;
  };

  const formatDuration = (activity: Activity) => {
    const { hours, minutes, seconds } = extractDuration(activity);

    console.log('[ActivityAdmin] formatDuration called for activity:', activity.name, {
      originalActivity: {
        durationHours: activity.durationHours,
        durationMinutes: activity.durationMinutes,
        durationSeconds: activity.durationSeconds,
        duration: (activity as any).duration
      },
      extracted: { hours, minutes, seconds }
    });

    if (hours === 0 && minutes === 0 && seconds === 0) {
      console.log('[ActivityAdmin] formatDuration returning "Not set"');
      return 'Not set';
    }

    const result = `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    console.log('[ActivityAdmin] formatDuration returning:', result);
    return result;
  };

  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Activity Management
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Paper sx={{ width: '100%', mb: 2 }}>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={activeTab} onChange={handleTabChange} aria-label="activity admin tabs">
            <Tab label="Activities" id="activity-admin-tab-0" aria-controls="activity-admin-tabpanel-0" />
            <Tab label="Categories" id="activity-admin-tab-1" aria-controls="activity-admin-tabpanel-1" />
          </Tabs>
        </Box>

        <TabPanel value={activeTab} index={0}>
          <Box sx={{ mb: 2 }}>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={handleCreateActivity}
              sx={{ mb: 2 }}
            >
              Create Activity
            </Button>
          </Box>

          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Category</TableCell>
                  <TableCell>Duration</TableCell>
                  <TableCell>Video</TableCell>
                  <TableCell>Created</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {activities.map((activity) => {
                  console.log('[ActivityAdmin] Rendering activity:', {
                    id: activity.id,
                    name: activity.name,
                    activityCategory: activity.activityCategory,
                    durationHours: activity.durationHours,
                    durationMinutes: activity.durationMinutes,
                    durationSeconds: activity.durationSeconds,
                  });

                  return (
                    <TableRow key={activity.id} hover>
                      <TableCell>
                        <Box>
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
                        </Box>
                      </TableCell>
                      <TableCell>
                        {activity.activityCategory ? (
                          <Chip
                            label={activity.activityCategory.name}
                            size="small"
                            variant="outlined"
                          />
                        ) : (
                          <Typography variant="body2" color="text.secondary">
                            {getCategoryName(activity.activityCategoryId)}
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {formatDuration(activity)}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        {activity.videoUrl ? (
                          <IconButton size="small" color="primary">
                            <VideoIcon />
                          </IconButton>
                        ) : (
                          <Typography variant="body2" color="text.secondary">
                            No video
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {new Date(activity.createdAt).toLocaleDateString()}
                        </Typography>
                      </TableCell>
                      <TableCell align="right">
                        <IconButton
                          size="small"
                          onClick={() => handleEditActivity(activity)}
                          color="primary"
                        >
                          <EditIcon />
                        </IconButton>
                        <IconButton
                          size="small"
                          onClick={() => handleDeleteActivity(activity)}
                          color="error"
                        >
                          <DeleteIcon />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  );
                })}
                {activities.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                      <Typography variant="body1" color="text.secondary">
                        No activities found. Create your first activity to get started.
                      </Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </TabPanel>

        <TabPanel value={activeTab} index={1}>
          <Box sx={{ mb: 2 }}>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={handleCreateCategory}
            >
              Create Category
            </Button>
          </Box>

          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Description</TableCell>
                  <TableCell>Activities Count</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {categories.map((category) => (
                  <TableRow key={category.id} hover>
                    <TableCell>
                      <Typography variant="body1" fontWeight="medium">
                        {category.name}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">
                        {category.description || 'No description'}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">
                        {activities.filter(a => a.activityCategoryId === category.id).length}
                      </Typography>
                    </TableCell>
                    <TableCell align="right">
                      <IconButton size="small" color="primary">
                        <EditIcon />
                      </IconButton>
                      <IconButton size="small" color="error">
                        <DeleteIcon />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
                {categories.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={4} align="center" sx={{ py: 4 }}>
                      <Typography variant="body1" color="text.secondary">
                        No categories found. Create your first category to get started.
                      </Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </TabPanel>
      </Paper>

      {/* Activity Form Dialog */}
      <Dialog
        open={formOpen}
        onClose={handleFormClose}
        maxWidth="md"
        fullWidth
        fullScreen={isMobile}
      >
        <DialogTitle>
          Create New Activity
        </DialogTitle>
        <DialogContent>
          <ActivityForm
            activity={null}
            categories={categories}
            onSuccess={handleFormSuccess}
            onCancel={handleFormClose}
          />
        </DialogContent>
      </Dialog>

      {/* Category Form Dialog */}
      <Dialog
        open={categoryFormOpen}
        onClose={handleCategoryFormClose}
        maxWidth="sm"
        fullWidth
        fullScreen={isMobile}
      >
        <DialogTitle>Create New Category</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <TextField
              fullWidth
              label="Category Name"
              value={categoryFormData.name}
              onChange={(e) => setCategoryFormData(prev => ({ ...prev, name: e.target.value }))}
              error={!!categoryFormErrors.name}
              helperText={categoryFormErrors.name}
              required
              sx={{ mb: 2 }}
            />
            <TextField
              fullWidth
              label="Description"
              value={categoryFormData.description}
              onChange={(e) => setCategoryFormData(prev => ({ ...prev, description: e.target.value }))}
              multiline
              rows={3}
              sx={{ mb: 2 }}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCategoryFormClose}>Cancel</Button>
          <Button
            onClick={handleCategoryFormSubmit}
            variant="contained"
            disabled={!categoryFormData.name.trim()}
          >
            Create
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default ActivityAdmin;
