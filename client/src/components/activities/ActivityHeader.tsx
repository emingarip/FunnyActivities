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
  Category as CategoryIcon,
} from '@mui/icons-material';

interface Activity {
  id: string;
  name: string;
  description?: string;
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
  showFavoriteButton?: boolean;
}

const ActivityHeader: React.FC<ActivityHeaderProps> = ({
  activity,
  favorites,
  onToggleFavorite,
  onBack,
  showFavoriteButton = true,
}) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));


  return (
    <Box sx={{ mb: 3 }} data-cy="activity-header">
      {/* Header */}
      <Box sx={{
        display: 'flex',
        justifyContent: 'flex-start',
        alignItems: 'flex-start',
        mb: 2,
        flexDirection: 'column',
        gap: 2,
      }}>
        <Box sx={{ width: '100%' }}>
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
      </Box>

      {/* Activity Info */}
      <Box sx={{
        display: 'flex',
        gap: 2,
        flexWrap: 'wrap',
        alignItems: 'center',
      }}>
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