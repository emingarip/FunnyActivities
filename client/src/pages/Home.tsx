import React from 'react';
import { Box } from '@mui/material';
import PublicActivities from '../components/activities/PublicActivities';

const Home: React.FC = () => {
  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
      <PublicActivities
        maxItems={12}
        showTitle={true}
        title="Activities"
      />
    </Box>
  );
};

export default Home;