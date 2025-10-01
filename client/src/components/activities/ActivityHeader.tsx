import React from 'react';
import {
  Box,
  Typography,
  IconButton,
  Chip,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import {
  Favorite as FavoriteIcon,
  FavoriteBorder as FavoriteBorderIcon,
  AccessTime as TimeIcon,
  Category as CategoryIcon,
} from '@mui/icons-material';

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
}

interface ActivityHeaderProps {
  activity: Activity;
  favorites: Set<string>;
  onToggleFavorite: () => void;
  onBack: () => void;
}

const ActivityHeader: React.FC<ActivityHeaderProps> = ({
  activity,
  favorites,
  onToggleFavorite,
  onBack,
}) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  const formatDuration = (hours?: number, minutes?: number, seconds?: number) => {
    const parts = [];
    if (hours && hours > 0) parts.push(`${hours}h`);
    if (minutes && minutes > 0) parts.push(`${minutes}m`);
    if (seconds && seconds > 0) parts.push(`${seconds}s`);
    return parts.join(' ') || 'N/A';
  };

  return (
    <Box sx={{ mb: 3 }} data-cy="activity-header">
      {/* Header */}
      <Box sx={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'flex-start',
        mb: 2,
        flexDirection: isMobile ? 'column' : 'row',
        gap: isMobile ? 2 : 0,
      }}>
        <Box sx={{ flex: 1 }}>
          <Typography
            variant={isMobile ? 'h5' : 'h4'}
            component="h1"
            gutterBottom
            sx={{ wordBreak: 'break-word' }}
            data-cy="activity-title"
          >
            {activity.name}
          </Typography>
          {activity.description && (
            <Typography
              variant="body1"
              color="text.secondary"
              sx={{ mb: 2, wordBreak: 'break-word' }}
              data-cy="activity-description"
            >
              {activity.description}
            </Typography>
          )}
        </Box>
        <IconButton
          onClick={onToggleFavorite}
          size="large"
          sx={{ alignSelf: isMobile ? 'flex-end' : 'flex-start' }}
        >
          {favorites.has(activity.id) ? (
            <FavoriteIcon color="error" fontSize="large" />
          ) : (
            <FavoriteBorderIcon fontSize="large" />
          )}
        </IconButton>
      </Box>

      {/* Activity Info */}
      <Box sx={{
        display: 'flex',
        gap: 2,
        flexWrap: 'wrap',
        alignItems: 'center',
      }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <TimeIcon fontSize="small" />
          <Typography variant="body2">
            Duration: {formatDuration(activity.durationHours, activity.durationMinutes, activity.durationSeconds)}
          </Typography>
        </Box>
        {activity.activityCategory && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <CategoryIcon fontSize="small" />
            <Chip label={activity.activityCategory.name} size="small" />
          </Box>
        )}
      </Box>
    </Box>
  );
};

export default ActivityHeader;