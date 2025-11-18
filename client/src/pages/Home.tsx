import React from 'react';
import { Box } from '@mui/material';
import PublicActivities from '../components/activities/PublicActivities';
import { useTranslation } from '../hooks/useTranslation';

const Home: React.FC = () => {
  const { t } = useTranslation();
  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
      <PublicActivities
        maxItems={12}
        showTitle={true}
        title={t('home_title')}
      />
    </Box>
  );
};

export default Home;
