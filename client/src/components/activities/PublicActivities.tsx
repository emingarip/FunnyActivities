import React, { useState, useEffect, useCallback, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Button,
  Card,
  CardContent,
  Typography,
  CircularProgress,
  Alert,
  useTheme,
  useMediaQuery,
  Chip,
  IconButton,
  Tooltip,
} from '@mui/material';
import {
  AccessTime as TimeIcon,
  Category as CategoryIcon,
  PlayArrow as PlayIcon,
  Refresh as RefreshIcon,
  Schedule as ScheduleIcon,
  Videocam as VideoIcon,
  Favorite as FavoriteIcon,
  FavoriteBorder as FavoriteBorderIcon,
} from '@mui/icons-material';
import { activitiesAPI, favoritesAPI } from '../../services/api';
import { useAuth } from '../../contexts/AuthContext';
import { useTranslation } from '../../hooks/useTranslation';
import VideoPreview from './VideoPreview';
import { getJsonItem } from '../../utils/storage';

interface Activity {
  id: string;
  name: string;
  description?: string;
  duration?: string;
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


interface PublicActivitiesProps {
  maxItems?: number;
  showTitle?: boolean;
  title?: string;
}

const PublicActivities: React.FC<PublicActivitiesProps> = ({
  maxItems = 6,
  showTitle = true,
  title = "Featured Activities"
}) => {
  const { t } = useTranslation();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();

  const [activities, setActivities] = useState<Activity[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdated, setLastUpdated] = useState<Date | null>(null);
  const [isPolling, setIsPolling] = useState(false);
  const [pollingError, setPollingError] = useState<string | null>(null);
  const [isVisible, setIsVisible] = useState(true);
  const [favorites, setFavorites] = useState<Set<string>>(new Set());
  const [favoritesLoading, setFavoritesLoading] = useState<Set<string>>(new Set());

  // Refs for polling control
  const pollingIntervalRef = useRef<NodeJS.Timeout | null>(null);
  const isComponentMountedRef = useRef(true);

  // Polling function to check for new activities
  const pollForUpdates = useCallback(async () => {
    if (!isVisible || !isComponentMountedRef.current) {
      return;
    }

    try {
      setIsPolling(true);
      setPollingError(null);

      const response = await activitiesAPI.getPublicActivities({
        pageNumber: 1,
        pageSize: maxItems,
        sortBy: 'name',
        sortOrder: 'asc',
      });

      if (response.data.success && response.data.data?.items) {
        const newActivities = response.data.data.items;

        // Check if there are any changes
        const hasChanges = JSON.stringify(activities.map((a: Activity) => a.id).sort()) !==
                          JSON.stringify(newActivities.map((a: Activity) => a.id).sort());

        if (hasChanges) {
          setActivities(newActivities);
          console.log('ðŸ”„ Activities updated via polling:', newActivities.length, 'activities');
        }
      }
    } catch (err: any) {
      console.error('Polling error:', err);
      setPollingError('Failed to check for updates');
    } finally {
      setIsPolling(false);
    }
  }, [activities, maxItems, isVisible]);

  // Manual refresh function
  const handleManualRefresh = useCallback(async () => {
    setPollingError(null);
    await loadActivities();
  }, []);

  // Visibility detection
  useEffect(() => {
    const handleVisibilityChange = () => {
      setIsVisible(!document.hidden);
    };

    document.addEventListener('visibilitychange', handleVisibilityChange);
    return () => {
      document.removeEventListener('visibilitychange', handleVisibilityChange);
    };
  }, []);

  // Polling setup
  useEffect(() => {
    if (isVisible && isComponentMountedRef.current) {
      // Initial poll after a short delay
      const initialPollTimeout = setTimeout(() => {
        pollForUpdates();
      }, 2000);

      // Set up polling interval
      pollingIntervalRef.current = setInterval(() => {
        pollForUpdates();
      }, 30000); // 30 seconds

      return () => {
        clearTimeout(initialPollTimeout);
        if (pollingIntervalRef.current) {
          clearInterval(pollingIntervalRef.current);
          pollingIntervalRef.current = null;
        }
      };
    } else {
      // Clear polling when not visible
      if (pollingIntervalRef.current) {
        clearInterval(pollingIntervalRef.current);
        pollingIntervalRef.current = null;
      }
    }
  }, [isVisible, pollForUpdates]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      isComponentMountedRef.current = false;
      if (pollingIntervalRef.current) {
        clearInterval(pollingIntervalRef.current);
      }
    };
  }, []);

  useEffect(() => {
    loadActivities();
    if (isAuthenticated) {
      loadUserData();
    }
  }, [maxItems, isAuthenticated]); // eslint-disable-line react-hooks/exhaustive-deps

  const loadActivities = async () => {
    try {
      setLoading(true);
      setError(null);

      const response = await activitiesAPI.getPublicActivities({
        pageNumber: 1,
        pageSize: maxItems,
        sortBy: 'name',
        sortOrder: 'asc',
      });

      if (response.data.success) {
        const newActivities = response.data.data?.items || [];
        setActivities(newActivities);
        setLastUpdated(new Date());

        // Process video URLs to handle corrupted object keys and parse duration
        const processedActivities = newActivities.map((activity: Activity) => {
          let processedActivity = { ...activity };

          if (activity.videoUrl) {
            // Handle corrupted object keys that may have bucket name prepended
            let processedVideoUrl = activity.videoUrl;
            if (processedVideoUrl.startsWith('activity-videos/')) {
              processedVideoUrl = processedVideoUrl.substring('activity-videos/'.length);
              console.log('Cleaned corrupted object key in PublicActivities:', {
                activityId: activity.id,
                originalUrl: activity.videoUrl,
                cleanedUrl: processedVideoUrl
              });
            }
            processedActivity.videoUrl = processedVideoUrl;
          }

          // Parse duration string "HH:MM:SS" into separate numeric fields
          if (activity.duration) {
            const durationParts = activity.duration.split(':');
            if (durationParts.length === 3) {
              const hours = parseInt(durationParts[0], 10);
              const minutes = parseInt(durationParts[1], 10);
              const seconds = parseInt(durationParts[2], 10);

              if (!isNaN(hours) && !isNaN(minutes) && !isNaN(seconds)) {
                processedActivity.durationHours = hours;
                processedActivity.durationMinutes = minutes;
                processedActivity.durationSeconds = seconds;
              }
            }
          }

          return processedActivity;
        });

        // Enhanced video URL logging
        console.log('ðŸŽ¬ PublicActivities: Activities loaded from API:', {
          totalActivities: processedActivities.length,
          activitiesWithVideoUrls: processedActivities.filter((activity: Activity) => activity.videoUrl).length,
          activitiesWithoutVideoUrls: processedActivities.filter((activity: Activity) => !activity.videoUrl).length,
          timestamp: new Date().toISOString(),
          videoUrls: processedActivities
            .filter((activity: Activity) => activity.videoUrl)
            .map((activity: Activity) => ({
              activityId: activity.id,
              activityName: activity.name,
              videoUrl: activity.videoUrl,
              isMinioObjectKey: activity.videoUrl?.includes('/') && !activity.videoUrl?.startsWith('http'),
              urlLength: activity.videoUrl?.length || 0,
              hasCategory: !!activity.activityCategory,
              categoryName: activity.activityCategory?.name,
              duration: formatDuration(activity.durationHours, activity.durationMinutes, activity.durationSeconds)
            }))
        });

        setActivities(processedActivities);

        // Log activities without video URLs for debugging
        const activitiesWithoutVideo = newActivities.filter((activity: Activity) => !activity.videoUrl);
        if (activitiesWithoutVideo.length > 0) {
          console.log('ðŸš« PublicActivities: Activities without video URLs:', activitiesWithoutVideo.map((activity: Activity) => ({
            activityId: activity.id,
            activityName: activity.name,
            hasCategory: !!activity.activityCategory,
            categoryName: activity.activityCategory?.name
          })));
        }
      }
    } catch (err: any) {
      console.error('Error loading public activities:', err);

      // Handle different types of errors
      if (err.isNetworkError) {
        setError('Network error - please check your internet connection');
      } else if (err.isServerError) {
        setError('Server error - please try again later');
      } else if (err.status === 404) {
        setError('Activities not found');
      } else if (err.status === 500) {
        setError('Internal server error - please try again later');
      } else if (err.message) {
        setError(err.message);
      } else {
        setError('Failed to load activities');
      }
    } finally {
      setLoading(false);
    }
  };

  const loadUserData = async () => {
    try {
      console.log('ðŸ”„ Loading user favorites in PublicActivities...');
      // Load favorites from API
      const favoritesResponse = await favoritesAPI.getUserFavorites();
      if (favoritesResponse.data.success) {
        const favoriteActivityIds = favoritesResponse.data.data?.map((fav: any) => fav.activityId) || [];
        console.log('âœ… Loaded favorites from API in PublicActivities:', favoriteActivityIds);
        setFavorites(new Set(favoriteActivityIds));
      } else {
        console.warn('âš ï¸ API response not successful in PublicActivities:', favoritesResponse.data);
      }
    } catch (err) {
      console.error('�?O Error loading user favorites in PublicActivities:', err);
      // Fallback to localStorage if API fails
      const storedFavorites = getJsonItem<string[]>('activityFavorites', []);
      if (storedFavorites.length > 0) {
        console.log('dY", Loading favorites from localStorage fallback in PublicActivities');
        setFavorites(new Set(storedFavorites));
      } else {
        console.log('�,1�,? No favorites found in localStorage in PublicActivities');
      }
    }
  };

  const toggleFavorite = async (activityId: string) => {
    console.log('ðŸ’– Toggle favorite clicked for activity in PublicActivities:', activityId);
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
      console.log('â³ Adding to loading state in PublicActivities');
      setFavoritesLoading(prev => new Set(prev).add(activityId));

      if (isCurrentlyFavorited) {
        console.log('ðŸ’” Removing from favorites in PublicActivities');
        // Remove from favorites
        await favoritesAPI.removeFromFavorites(activityId);
        newFavorites.delete(activityId);
        console.log('âœ… Successfully removed from favorites in PublicActivities');
      } else {
        console.log('â¤ï¸ Adding to favorites in PublicActivities');
        // Add to favorites
        await favoritesAPI.addToFavorites(activityId);
        newFavorites.add(activityId);
        console.log('âœ… Successfully added to favorites in PublicActivities');
      }

      console.log('ðŸ”„ Updating favorites state in PublicActivities');
      setFavorites(newFavorites);
    } catch (err: any) {
      console.error('âŒ Error toggling favorite in PublicActivities:', err);
      let errorMessage = 'Failed to update favorites';

      if (err.response?.data?.message) {
        errorMessage = err.response.data.message;
        console.error('âŒ API Error in PublicActivities:', err.response.data);
      } else if (err.message) {
        errorMessage = err.message;
      }

      console.error('âŒ Error message in PublicActivities:', errorMessage);

      // Revert optimistic update on error
      console.log('ðŸ”„ Reverting optimistic update due to error in PublicActivities');
      if (isCurrentlyFavorited) {
        newFavorites.add(activityId);
      } else {
        newFavorites.delete(activityId);
      }
      setFavorites(newFavorites);
    } finally {
      // Remove from loading state
      console.log('â³ Removing from loading state in PublicActivities');
      setFavoritesLoading(prev => {
        const newSet = new Set(prev);
        newSet.delete(activityId);
        return newSet;
      });
    }
  };

  const formatDuration = (hours?: number, minutes?: number, seconds?: number) => {
    const parts = [];
    if (hours && hours > 0) parts.push(`${hours}h`);
    if (minutes && minutes > 0) parts.push(`${minutes}m`);
    if (seconds && seconds > 0) parts.push(`${seconds}s`);
    return parts.join(' ') || 'N/A';
  };


  const handleActivityClick = (activity: Activity) => {
    // For public display, we could navigate to a public activity detail page
    // or open in a new tab. For now, we'll just log the activity
    console.log('Activity clicked:', activity.name);
    // TODO: Implement navigation to public activity detail
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mb: 2 }}>
        {error}
      </Alert>
    );
  }

  return (
    <Box sx={{ py: { xs: 2, md: 4 }, px: { xs: 2, md: 3 } }}>
      {showTitle && (
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', mb: { xs: 3, md: 4 } }}>
          <Typography
            variant={isMobile ? "h4" : "h3"}
            component="h2"
            sx={{
              textAlign: 'center',
              fontWeight: 700,
              color: theme.palette.primary.main,
              mb: isMobile ? 1 : 0
            }}
          >
            {title}
          </Typography>
        </Box>
      )}

      {/* Polling Status Indicator */}
      {!showTitle && isPolling && (
        <Box sx={{ display: 'flex', justifyContent: 'center', mb: 2 }}>
          <Typography variant="body2" color="text.secondary">
            Checking for updates...
          </Typography>
        </Box>
      )}

      {/* Polling Error Display */}
      {pollingError && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          {pollingError} - Auto-refresh may not be working
        </Alert>
      )}

      {activities.length === 0 ? (
        <Box textAlign="center" py={8}>
          <Typography variant="h6" color="text.secondary" gutterBottom>
            {t('publicActivities.noActivitiesAvailable')}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {t('publicActivities.checkBackLater')}
          </Typography>
        </Box>
      ) : (
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: isMobile ? '1fr' : 'repeat(auto-fit, minmax(300px, 440px))',
            gap: 3,
            width: '100%',
            justifyContent: 'center',
          }}
        >
          {activities.map((activity, index) => (
            <Card
              key={activity.id}
              sx={{
                width: '100%',
                height: '100%',
                display: 'flex',
                flexDirection: 'column',
                cursor: 'pointer',
                transition: 'transform 0.2s, box-shadow 0.2s',
                '&:hover': {
                  transform: 'translateY(-4px)',
                  boxShadow: theme.shadows[8],
                },
                borderRadius: 2,
              }}
              onClick={() => handleActivityClick(activity)}
            >
              {/* Video Preview Section */}
              {(() => {
                const shouldRenderVideo = !!activity.videoUrl;
                console.log('ðŸŽ¬ PublicActivities: VideoPreview rendering decision:', {
                  activityId: activity.id,
                  activityName: activity.name,
                  videoUrl: activity.videoUrl,
                  shouldRenderVideo,
                  isValidUrl: activity.videoUrl ? (activity.videoUrl.startsWith('http') || activity.videoUrl.includes('/')) : false,
                  isMinioObjectKey: activity.videoUrl ? (activity.videoUrl.includes('/') && !activity.videoUrl.startsWith('http')) : false,
                  index,
                  timestamp: new Date().toISOString()
                });
                return shouldRenderVideo;
              })() && (
                <Box
                  sx={{
                    position: 'relative',
                    width: '100%',
                    aspectRatio: '16 / 9',
                    overflow: 'hidden',
                    borderTopLeftRadius: 8,
                    borderTopRightRadius: 8,
                    bgcolor: 'black',
                  }}
                >
                  <VideoPreview
                    src={activity.videoUrl!}
                    activityId={activity.id}
                    autoPlay={isMobile}
                    muted={isMobile}
                    loop={true}
                    width="100%"
                    height="100%"
                    index={index}
                    onPlay={() => console.log(`Playing video for activity: ${activity.name}`)}
                    onPause={() => console.log(`Paused video for activity: ${activity.name}`)}
                    onError={(error) => {
                      console.error(`Video error for activity ${activity.name}:`, error);
                    }}
                  />

                  {/* Video indicator overlay */}
                  <Box
                    sx={{
                      position: 'absolute',
                      top: 12,
                      right: 12,
                      bgcolor: 'rgba(0, 0, 0, 0.6)',
                      borderRadius: '50%',
                      width: 32,
                      height: 32,
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                    }}
                  >
                    <VideoIcon sx={{ color: 'white', fontSize: 16 }} />
                  </Box>
                </Box>
              )}

              <CardContent sx={{ flexGrow: 1, p: 2 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                  <Typography
                    variant="h6"
                    component="h3"
                    sx={{
                      flexGrow: 1,
                      mr: 1,
                      fontWeight: 600,
                      lineHeight: 1.3,
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      display: '-webkit-box',
                      WebkitLineClamp: 2,
                      WebkitBoxOrient: 'vertical',
                    }}
                  >
                    {activity.name}
                  </Typography>
                  {isAuthenticated && (
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
                  )}
                </Box>

                {activity.description && (
                  <Typography
                    variant="body2"
                    color="text.secondary"
                    sx={{
                      mb: 2,
                      lineHeight: 1.5,
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      display: '-webkit-box',
                      WebkitLineClamp: activity.videoUrl ? 2 : 3, // Less space if video is present
                      WebkitBoxOrient: 'vertical',
                    }}
                  >
                    {activity.description}
                  </Typography>
                )}

                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <TimeIcon fontSize="small" color="action" />
                    <Typography variant="body2" color="text.secondary">
                      {formatDuration(activity.durationHours, activity.durationMinutes, activity.durationSeconds)}
                    </Typography>
                  </Box>
                  {activity.activityCategory && (
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <CategoryIcon fontSize="small" color="action" />
                      <Chip
                        label={activity.activityCategory.name}
                        size="small"
                        variant="outlined"
                        sx={{
                          fontSize: '0.75rem',
                          height: 24,
                        }}
                      />
                    </Box>
                  )}
                </Box>
              </CardContent>

              <Box
                sx={{
                  p: 1,
                  pt: 0,
                  display: 'flex',
                  justifyContent: 'flex-end',
                }}
              >
                <Button
                  variant="text"
                  data-cy="start-activity-button"
                  onClick={(e) => {
                    e.stopPropagation();
                    navigate(`/activity/${activity.id}`);
                  }}
                  sx={{
                    color: theme.palette.primary.main,
                    fontSize: '0.875rem',
                    fontWeight: 500,
                    display: 'flex',
                    alignItems: 'center',
                    gap: 0.5,
                    minHeight: 44,
                    '&:hover': {
                      color: theme.palette.primary.dark,
                    },
                  }}
                >
                  <PlayIcon fontSize="small" />
                  Start Activity
                </Button>
              </Box>
            </Card>
          ))}
        </Box>
      )}
    </Box>
  );
};

export default PublicActivities;

