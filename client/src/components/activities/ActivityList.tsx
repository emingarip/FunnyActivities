import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Card,
  CardContent,
  CardActions,
  Typography,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Grid,
  Button,
  Chip,
  IconButton,
  Pagination,
  CircularProgress,
  Alert,
  useTheme,
  useMediaQuery,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  List,
  ListItem,
  ListItemText,
  Divider,
  Snackbar,
} from '@mui/material';
import {
  Search as SearchIcon,
  Favorite as FavoriteIcon,
  FavoriteBorder as FavoriteBorderIcon,
  PlayArrow as PlayIcon,
  AccessTime as TimeIcon,
  Category as CategoryIcon,
  ListAlt as ListIcon,
} from '@mui/icons-material';
import { activitiesAPI, activityCategoriesAPI, favoritesAPI } from '../../services/api';
import { useAuth } from '../../contexts/AuthContext';
import { getJsonItem } from '../../utils/storage';

interface Activity {
  id: string;
  name: string;
  description?: string;
  durationHours?: number;
  durationMinutes?: number;
  durationSeconds?: number;
  activityCategoryId?: string;
  activityCategory?: {
    id: string;
    name: string;
  };
  videoUrl?: string;
}

interface ActivityCategory {
  id: string;
  name: string;
  description?: string;
}

interface ActivityProgress {
  activityId: string;
  completedSteps: number;
  totalSteps: number;
  lastWatchedAt?: string;
}

interface ActivityListProps {
  onActivitySelect: (activity: Activity) => void;
}

const ActivityList: React.FC<ActivityListProps> = ({ onActivitySelect }) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();

  const [activities, setActivities] = useState<Activity[]>([]);
  const [categories, setCategories] = useState<ActivityCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [favorites, setFavorites] = useState<Set<string>>(new Set());
  const [favoritesLoading, setFavoritesLoading] = useState<Set<string>>(new Set());
  const [progress, setProgress] = useState<Map<string, ActivityProgress>>(new Map());
  const [selectedActivity, setSelectedActivity] = useState<Activity | null>(null);
  const [materialsDialogOpen, setMaterialsDialogOpen] = useState(false);
  const [activityMaterials, setActivityMaterials] = useState<any[]>([]);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false,
    message: '',
    severity: 'success'
  });

  const pageSize = 12;

  useEffect(() => {
    loadCategories();
    loadActivities();
    if (isAuthenticated) {
      loadUserData();
    }
  }, [isAuthenticated]);

  useEffect(() => {
    loadActivities();
  }, [searchTerm, selectedCategory, page]);

  const loadCategories = async () => {
    try {
      const response = await activityCategoriesAPI.getActivityCategories({
        pageSize: 100,
      });
      if (response.data.success) {
        setCategories(response.data.data?.items || []);
      }
    } catch (err) {
      console.error('Error loading categories:', err);
    }
  };

  const loadActivities = async () => {
    try {
      setLoading(true);
      setError(null);

      const params: any = {
        pageNumber: page,
        pageSize,
        sortBy: 'name',
        sortOrder: 'asc',
      };

      if (searchTerm) {
        params.searchTerm = searchTerm;
      }

      if (selectedCategory) {
        params.activityCategoryId = selectedCategory;
      }

      const response = await activitiesAPI.getActivities(params);

      if (response.data.success) {
        setActivities(response.data.data?.items || []);
        setTotalPages(Math.ceil((response.data.data?.totalCount || 0) / pageSize));
      }
    } catch (err: any) {
      console.error('Error loading activities:', err);
      setError('Failed to load activities');
    } finally {
      setLoading(false);
    }
  };

  const loadUserData = async () => {
    try {
      console.log('dY", Loading user favorites...');
      // Load favorites from API
      const favoritesResponse = await favoritesAPI.getUserFavorites();
      if (favoritesResponse.data.success) {
        const favoriteActivityIds = favoritesResponse.data.data?.map((fav: any) => fav.activityId) || [];
        console.log('�o. Loaded favorites from API:', favoriteActivityIds);
        setFavorites(new Set(favoriteActivityIds));
      } else {
        console.warn('�s��,? API response not successful:', favoritesResponse.data);
      }
    } catch (err) {
      console.error('�?O Error loading user favorites:', err);
      // Fallback to localStorage if API fails
      const storedFavorites = getJsonItem<string[]>('activityFavorites', []);
      if (storedFavorites.length > 0) {
        console.log('dY", Loading favorites from localStorage fallback');
        setFavorites(new Set(storedFavorites));
      } else {
        console.log('�,1�,? No favorites found in localStorage');
      }
    }

    // Load progress from localStorage (in a real app, this would come from API)
    const storedProgress = getJsonItem<Record<string, ActivityProgress>>('activityProgress', {});
    setProgress(new Map(Object.entries(storedProgress)));
  };

  const toggleFavorite = async (activityId: string) => {
    console.log('ðŸ’– Toggle favorite clicked for activity:', activityId);
    console.log('ðŸ” Is authenticated:', isAuthenticated);
    console.log('â³ Is loading:', favoritesLoading.has(activityId));
    console.log('â¤ï¸ Is currently favorited:', favorites.has(activityId));

    // Check if user is authenticated
    if (!isAuthenticated) {
      console.log('ðŸš« User not authenticated, redirecting to login');
      navigate('/login');
      return;
    }

    // Check if already loading this activity
    if (favoritesLoading.has(activityId)) {
      console.log('â³ Already loading this activity, ignoring click');
      return;
    }

    const isCurrentlyFavorited = favorites.has(activityId);
    const newFavorites = new Set(favorites);

    try {
      // Add to loading state
      console.log('â³ Adding to loading state');
      setFavoritesLoading(prev => new Set(prev).add(activityId));

      if (isCurrentlyFavorited) {
        console.log('ðŸ’” Removing from favorites');
        // Remove from favorites
        await favoritesAPI.removeFromFavorites(activityId);
        newFavorites.delete(activityId);
        console.log('âœ… Successfully removed from favorites');
        showSnackbar('Removed from favorites', 'success');
      } else {
        console.log('â¤ï¸ Adding to favorites');
        // Add to favorites
        await favoritesAPI.addToFavorites(activityId);
        newFavorites.add(activityId);
        console.log('âœ… Successfully added to favorites');
        showSnackbar('Added to favorites', 'success');
      }

      console.log('ðŸ”„ Updating favorites state');
      setFavorites(newFavorites);
    } catch (err: any) {
      console.error('âŒ Error toggling favorite:', err);
      let errorMessage = 'Failed to update favorites';

      if (err.response?.data?.message) {
        errorMessage = err.response.data.message;
        console.error('âŒ API Error:', err.response.data);
      } else if (err.message) {
        errorMessage = err.message;
      }

      console.error('âŒ Error message:', errorMessage);
      showSnackbar(errorMessage, 'error');

      // Revert optimistic update on error
      console.log('ðŸ”„ Reverting optimistic update due to error');
      if (isCurrentlyFavorited) {
        newFavorites.add(activityId);
      } else {
        newFavorites.delete(activityId);
      }
      setFavorites(newFavorites);
    } finally {
      // Remove from loading state
      console.log('â³ Removing from loading state');
      setFavoritesLoading(prev => {
        const newSet = new Set(prev);
        newSet.delete(activityId);
        return newSet;
      });
    }
  };

  const showSnackbar = (message: string, severity: 'success' | 'error') => {
    setSnackbar({ open: true, message, severity });
  };

  const handleCloseSnackbar = () => {
    setSnackbar(prev => ({ ...prev, open: false }));
  };

  const formatDuration = (hours?: number, minutes?: number, seconds?: number) => {
    const parts = [];
    if (hours && hours > 0) parts.push(`${hours}h`);
    if (minutes && minutes > 0) parts.push(`${minutes}m`);
    if (seconds && seconds > 0) parts.push(`${seconds}s`);
    return parts.join(' ') || 'N/A';
  };

  const getProgressPercentage = (activityId: string) => {
    const activityProgress = progress.get(activityId);
    if (!activityProgress) return 0;
    return Math.round((activityProgress.completedSteps / activityProgress.totalSteps) * 100);
  };

  const handleActivityClick = (activity: Activity) => {
    onActivitySelect(activity);
  };

  const handleMaterialsClick = async (activity: Activity) => {
    setSelectedActivity(activity);
    try {
      // Load materials for this activity
      const response = await activitiesAPI.getActivityWithDetails(activity.id);
      if (response.data.success) {
        setActivityMaterials(response.data.data?.activityProductVariants || []);
        setMaterialsDialogOpen(true);
      }
    } catch (err) {
      console.error('Error loading materials:', err);
    }
  };

  const handlePageChange = (event: React.ChangeEvent<unknown>, value: number) => {
    setPage(value);
  };

  if (loading && activities.length === 0) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Search and Filter Section */}
      <Box sx={{ mb: 4 }}>
        <Box sx={{ display: 'flex', flexDirection: isMobile ? 'column' : 'row', gap: 2, alignItems: 'center' }}>
          <TextField
            fullWidth
            variant="outlined"
            placeholder="Search activities..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            InputProps={{
              startAdornment: <SearchIcon sx={{ mr: 1, color: 'text.secondary' }} />,
            }}
            sx={{ flex: isMobile ? 1 : 2 }}
          />
          <FormControl sx={{ minWidth: '200px', flex: 1 }}>
            <InputLabel>Category</InputLabel>
            <Select
              value={selectedCategory}
              label="Category"
              onChange={(e) => setSelectedCategory(e.target.value)}
            >
              <MenuItem value="">
                <em>All Categories</em>
              </MenuItem>
              {categories.map((category) => (
                <MenuItem key={category.id} value={category.id}>
                  {category.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          <Button
            variant="outlined"
            onClick={() => {
              setSearchTerm('');
              setSelectedCategory('');
              setPage(1);
            }}
            sx={{ minWidth: '120px', minHeight: 44 }}
          >
            Clear Filters
          </Button>
        </Box>
      </Box>

      {/* Activities Grid */}
      {activities.length === 0 && !loading ? (
        <Box textAlign="center" py={8}>
          <Typography variant="h6" color="text.secondary">
            No activities found
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Try adjusting your search or filters
          </Typography>
        </Box>
      ) : (
        <>
          <Box sx={{ display: 'grid', gridTemplateColumns: isMobile ? '1fr' : 'repeat(auto-fill, minmax(300px, 1fr))', gap: 3 }}>
            {activities.map((activity) => (
              <Card
                key={activity.id}
                sx={{
                  height: '100%',
                  display: 'flex',
                  flexDirection: 'column',
                  cursor: 'pointer',
                  transition: 'transform 0.2s, box-shadow 0.2s',
                  '&:hover': {
                    transform: 'translateY(-4px)',
                    boxShadow: theme.shadows[8],
                  },
                }}
                onClick={() => handleActivityClick(activity)}
              >
                <CardContent sx={{ flexGrow: 1 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                    <Typography variant="h6" component="h2" sx={{ flexGrow: 1, mr: 1 }}>
                      {activity.name}
                    </Typography>
                    <IconButton
                      size="medium"
                      onClick={(e) => {
                        e.stopPropagation();
                        toggleFavorite(activity.id);
                      }}
                      sx={{
                        p: 1,
                        color: favorites.has(activity.id) ? 'error.main' : 'action.active',
                        '&:hover': {
                          backgroundColor: 'rgba(0, 0, 0, 0.04)',
                        }
                      }}
                      disabled={favoritesLoading.has(activity.id)}
                      title={favorites.has(activity.id) ? 'Remove from favorites' : 'Add to favorites'}
                    >
                      {favoritesLoading.has(activity.id) ? (
                        <CircularProgress size={20} />
                      ) : favorites.has(activity.id) ? (
                        <FavoriteIcon sx={{ fontSize: 24 }} />
                      ) : (
                        <FavoriteBorderIcon sx={{ fontSize: 24 }} />
                      )}
                    </IconButton>
                  </Box>

                  {activity.description && (
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                      {activity.description.length > 100
                        ? `${activity.description.substring(0, 100)}...`
                        : activity.description}
                    </Typography>
                  )}

                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                    <TimeIcon fontSize="small" color="action" />
                    <Typography variant="body2" color="text.secondary">
                      {formatDuration(activity.durationHours, activity.durationMinutes, activity.durationSeconds)}
                    </Typography>
                  </Box>

                  {activity.activityCategory && (
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                      <CategoryIcon fontSize="small" color="action" />
                      <Chip
                        label={activity.activityCategory.name}
                        size="small"
                        variant="outlined"
                      />
                    </Box>
                  )}

                  {getProgressPercentage(activity.id) > 0 && (
                    <Box sx={{ mb: 2 }}>
                      <Typography variant="body2" color="text.secondary" sx={{ mb: 0.5 }}>
                        Progress: {getProgressPercentage(activity.id)}%
                      </Typography>
                      <Box
                        sx={{
                          width: '100%',
                          height: 4,
                          bgcolor: 'grey.200',
                          borderRadius: 2,
                        }}
                      >
                        <Box
                          sx={{
                            width: `${getProgressPercentage(activity.id)}%`,
                            height: '100%',
                            bgcolor: 'primary.main',
                            borderRadius: 2,
                          }}
                        />
                      </Box>
                    </Box>
                  )}
                </CardContent>

                <CardActions sx={{ justifyContent: 'space-between', px: 2, pb: 2 }}>
                  <Button
                    size="small"
                    startIcon={<ListIcon />}
                    onClick={(e) => {
                      e.stopPropagation();
                      handleMaterialsClick(activity);
                    }}
                    sx={{ minHeight: 44 }}
                  >
                    Materials
                  </Button>
                  <Button
                    size="small"
                    variant="contained"
                    startIcon={<PlayIcon />}
                    onClick={(e) => {
                      e.stopPropagation();
                      handleActivityClick(activity);
                    }}
                    sx={{ minHeight: 44 }}
                  >
                    Start
                  </Button>
                </CardActions>
              </Card>
            ))}
          </Box>

          {/* Pagination */}
          {totalPages > 1 && (
            <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
              <Pagination
                count={totalPages}
                page={page}
                onChange={handlePageChange}
                color="primary"
                size={isMobile ? 'small' : 'medium'}
              />
            </Box>
          )}
        </>
      )}

      {/* Materials Dialog */}
      <Dialog
        open={materialsDialogOpen}
        onClose={() => setMaterialsDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>
          Materials for {selectedActivity?.name}
        </DialogTitle>
        <DialogContent>
          {activityMaterials.length === 0 ? (
            <Typography color="text.secondary">
              No materials required for this activity
            </Typography>
          ) : (
            <List>
              {activityMaterials.map((material, index) => (
                <React.Fragment key={material.id}>
                  <ListItem>
                    <ListItemText
                      primary={`${material.productVariant?.baseProduct?.name} - ${material.productVariant?.name}`}
                      secondary={`Quantity: ${material.quantity} ${material.unitOfMeasure?.name || ''}`}
                    />
                  </ListItem>
                  {index < activityMaterials.length - 1 && <Divider />}
                </React.Fragment>
              ))}
            </List>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setMaterialsDialogOpen(false)} sx={{ minHeight: 44 }}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Snackbar for notifications */}
      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={handleCloseSnackbar}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'left' }}
      >
        <Alert onClose={handleCloseSnackbar} severity={snackbar.severity} sx={{ width: '100%' }}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default ActivityList;
