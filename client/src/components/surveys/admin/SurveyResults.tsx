import React, { useState } from 'react';
import {
  Container,
  Paper,
  Typography,
  Box,
  Tabs,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Button,
  Alert,
  CircularProgress,
  Chip,
  LinearProgress,
  Card,
  CardContent,
  Grid,
  IconButton,
  Tooltip,
  useTheme,
  useMediaQuery,
  Stack,
  Divider,
} from '@mui/material';
import {
  ArrowBack,
  Download,
  Refresh,
  TrendingUp,
  People,
  Star,
  HowToVote,
  CheckCircle,
  Pending,
} from '@mui/icons-material';
import { useNavigate, useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import AdminRoute from '../../../components/AdminRoute';
import SurveyService from '../../../services/surveyService';
import { SurveyResults as SurveyResultsType, ActivityResult, SurveyParticipant } from '../../../types/survey.types';

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
      id={`survey-results-tabpanel-${index}`}
      aria-labelledby={`survey-results-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ py: 3 }}>{children}</Box>}
    </div>
  );
}

const SurveyResults: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const isSmallMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const [activeTab, setActiveTab] = useState(0);

  // Fetch survey results
  const {
    data: resultsResponse,
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: ['survey-results', id],
    queryFn: () => SurveyService.getSurveyResults(id!),
    enabled: !!id,
    staleTime: 30000,
  });

  const results: SurveyResultsType | undefined = resultsResponse?.data;

  // Fetch survey participants
  const {
    data: participantsResponse,
    isLoading: participantsLoading,
    error: participantsError,
  } = useQuery({
    queryKey: ['survey-participants', id],
    queryFn: () => SurveyService.getSurveyParticipants(id!),
    enabled: !!id,
    staleTime: 30000,
  });

  const participants: SurveyParticipant[] | undefined = participantsResponse?.data;

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
  };

  const handleBack = () => {
    navigate('/admin/surveys');
  };

  const handleExport = (format: 'csv' | 'json') => {
    if (!results) return;

    let content: string;
    let filename: string;
    let mimeType: string;

    if (format === 'csv') {
      const headers = ['Activity', 'Average Vote', 'Vote Count'];
      const csvData = results.activityResults.map(activity => [
        activity.activityName,
        activity.averageVote.toFixed(2),
        activity.voteCount,
      ]);

      content = [headers, ...csvData].map(row => row.join(',')).join('\n');
      filename = `survey-results-${results.surveyId}.csv`;
      mimeType = 'text/csv';
    } else {
      content = JSON.stringify(results, null, 2);
      filename = `survey-results-${results.surveyId}.json`;
      mimeType = 'application/json';
    }

    const blob = new Blob([content], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  };

  const getVoteDistributionColor = (percentage: number) => {
    if (percentage >= 80) return 'success';
    if (percentage >= 60) return 'info';
    if (percentage >= 40) return 'warning';
    return 'error';
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  // Calculate if a participant has completed the survey based on voting activity
  const isParticipantCompleted = (participant: SurveyParticipant): boolean => {
    if (!results?.activityResults || !results.votes) return participant.isCompleted;

    // Get all survey activity IDs
    const surveyActivityIds = results.activityResults.map(activity => activity.surveyActivityId);

    // Get votes for this participant
    const participantVotes = results.votes.filter(vote => vote.surveyParticipantId === participant.id);

    // Check if participant has voted on all activities
    const votedActivityIds = participantVotes.map(vote => vote.surveyActivityId);
    const hasVotedOnAllActivities = surveyActivityIds.every(activityId =>
      votedActivityIds.includes(activityId)
    );

    return hasVotedOnAllActivities;
  };

  if (isLoading) {
    return (
      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Box display="flex" justifyContent="center" py={8}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (error || !results) {
    return (
      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Alert severity="error">
          Error loading survey results. Please try again.
        </Alert>
      </Container>
    );
  }

  return (
    <AdminRoute>
      <Container maxWidth="xl" sx={{ py: { xs: 2, sm: 3, md: 4 } }}>
        <Stack spacing={3} mb={3}>
          {/* Header */}
          <Box
            display="flex"
            flexDirection={{ xs: 'column', sm: 'row' }}
            alignItems={{ xs: 'flex-start', sm: 'center' }}
            justifyContent={{ xs: 'flex-start', sm: 'space-between' }}
            gap={{ xs: 2, sm: 0 }}
          >
            <Box display="flex" alignItems="center" width={{ xs: '100%', sm: 'auto' }}>
              <Button
                startIcon={<ArrowBack />}
                onClick={handleBack}
                sx={{ mr: { xs: 1, sm: 2 } }}
                size={isSmallMobile ? 'small' : 'medium'}
              >
                {isSmallMobile ? 'Back' : 'Back to Surveys'}
              </Button>
              <Box flex={1}>
                <Typography
                  variant={isSmallMobile ? 'h5' : 'h4'}
                  component="h1"
                  fontWeight="bold"
                >
                  Survey Results
                </Typography>
                <Typography
                  variant={isSmallMobile ? 'body1' : 'h6'}
                  color="text.secondary"
                >
                  {results.surveyTitle}
                </Typography>
              </Box>
            </Box>

            <Stack
              direction={{ xs: 'column', sm: 'row' }}
              spacing={{ xs: 1, sm: 1 }}
              width={{ xs: '100%', sm: 'auto' }}
            >
              <Button
                variant="outlined"
                startIcon={<Refresh />}
                onClick={() => refetch()}
                fullWidth={isSmallMobile}
                size={isSmallMobile ? 'small' : 'medium'}
              >
                Refresh
              </Button>
              <Button
                variant="outlined"
                startIcon={<Download />}
                onClick={() => handleExport('csv')}
                fullWidth={isSmallMobile}
                size={isSmallMobile ? 'small' : 'medium'}
              >
                Export CSV
              </Button>
              <Button
                variant="contained"
                startIcon={<Download />}
                onClick={() => handleExport('json')}
                fullWidth={isSmallMobile}
                size={isSmallMobile ? 'small' : 'medium'}
              >
                Export JSON
              </Button>
            </Stack>
          </Box>
        </Stack>

      {/* Summary Cards */}
      <Box
        display="grid"
        gridTemplateColumns={{
          xs: '1fr',
          sm: 'repeat(2, 1fr)',
          md: 'repeat(auto-fit, minmax(250px, 1fr))'
        }}
        gap={3}
        sx={{ mb: 4 }}
      >
        <Card>
          <CardContent>
            <Box display="flex" alignItems="center" justifyContent="space-between">
              <Box>
                <Typography color="text.secondary" gutterBottom>
                  Total Participants
                </Typography>
                <Typography variant="h4" fontWeight="bold">
                  {participantsLoading ? '...' : (participants?.length || 0)}
                </Typography>
              </Box>
              <People color="primary" sx={{ fontSize: 40 }} />
            </Box>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Box display="flex" alignItems="center" justifyContent="space-between">
              <Box>
                <Typography color="text.secondary" gutterBottom>
                  Completed
                </Typography>
                <Typography variant="h4" fontWeight="bold">
                  {participantsLoading ? '...' : (participants?.filter(p => p.isCompleted).length || 0)}
                </Typography>
              </Box>
              <TrendingUp color="success" sx={{ fontSize: 40 }} />
            </Box>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Box display="flex" alignItems="center" justifyContent="space-between">
              <Box>
                <Typography color="text.secondary" gutterBottom>
                  Completion Rate
                </Typography>
                <Typography variant="h4" fontWeight="bold">
                  {participantsLoading ? '...' : (
                    participants && participants.length > 0
                      ? ((participants.filter(p => p.isCompleted).length / participants.length) * 100).toFixed(1)
                      : '0.0'
                  )}%
                </Typography>
              </Box>
              <Star color="warning" sx={{ fontSize: 40 }} />
            </Box>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Box display="flex" alignItems="center" justifyContent="space-between">
              <Box>
                <Typography color="text.secondary" gutterBottom>
                  Total Votes
                </Typography>
                <Typography variant="h4" fontWeight="bold">
                  {results.activityResults.reduce((sum, activity) => sum + activity.voteCount, 0)}
                </Typography>
              </Box>
              <HowToVote color="info" sx={{ fontSize: 40 }} />
            </Box>
          </CardContent>
        </Card>
      </Box>

      {/* Tabs */}
      <Paper>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs
            value={activeTab}
            onChange={handleTabChange}
            aria-label="survey results tabs"
            variant={isMobile ? 'fullWidth' : 'standard'}
            scrollButtons={isMobile ? 'auto' : false}
          >
            <Tab
              label={isSmallMobile ? 'Overview' : 'Overview'}
              id="survey-results-tab-0"
            />
            <Tab
              label={isSmallMobile ? 'Activities' : 'Activity Breakdown'}
              id="survey-results-tab-1"
            />
            <Tab
              label={isSmallMobile ? 'Participants' : 'Participants'}
              id="survey-results-tab-2"
            />
          </Tabs>
        </Box>

        {/* Overview Tab */}
        <TabPanel value={activeTab} index={0}>
          <Box
            display="grid"
            gridTemplateColumns={{
              xs: '1fr',
              md: 'repeat(auto-fit, minmax(300px, 1fr))'
            }}
            gap={3}
          >
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Survey Overview
                </Typography>
                <Box sx={{ mb: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    Survey ID: {results.surveyId}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Total Activities: {results.activityResults.length}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Average Rating: {results.activityResults.length > 0 ? (results.activityResults.reduce((sum, a) => sum + a.averageVote, 0) / results.activityResults.length).toFixed(2) : 'N/A'}
                  </Typography>
                </Box>
              </CardContent>
            </Card>

            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Top Performing Activities
                </Typography>
                <Box>
                  {results.activityResults
                    .sort((a, b) => b.averageVote - a.averageVote)
                    .slice(0, 3)
                    .map((activity, index) => (
                      <Box key={activity.activityId} display="flex" alignItems="center" mb={1}>
                        <Typography variant="body2" sx={{ minWidth: 20 }}>
                          #{index + 1}
                        </Typography>
                        <Typography variant="body2" sx={{ flex: 1, ml: 1 }}>
                          {activity.activityName}
                        </Typography>
                        <Chip
                          label={activity.averageVote.toFixed(2)}
                          size="small"
                          color={getVoteDistributionColor(activity.averageVote * 20)}
                        />
                      </Box>
                    ))}
                </Box>
              </CardContent>
            </Card>
          </Box>
        </TabPanel>

        {/* Activity Breakdown Tab */}
        <TabPanel value={activeTab} index={1}>
          <TableContainer sx={{ overflowX: 'auto' }}>
            <Table sx={{ minWidth: isMobile ? 600 : 'auto' }}>
              <TableHead>
                <TableRow>
                  <TableCell>Activity</TableCell>
                  <TableCell align="center">Average Vote</TableCell>
                  <TableCell align="center">Vote Count</TableCell>
                  <TableCell align="center">Vote Distribution</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {results.activityResults.map((activity) => (
                  <TableRow key={activity.activityId} hover>
                    <TableCell>
                      <Box>
                        <Typography variant="subtitle2" fontWeight="medium">
                          {activity.activityName}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          {activity.activityDescription}
                        </Typography>
                      </Box>
                    </TableCell>
                    <TableCell align="center">
                      <Box display="flex" alignItems="center" justifyContent="center" gap={1}>
                        <Typography variant="h6" fontWeight="bold">
                          {activity.averageVote.toFixed(2)}
                        </Typography>
                        <Chip
                          label={`${(activity.averageVote * 20).toFixed(0)}%`}
                          size="small"
                          color={getVoteDistributionColor(activity.averageVote * 20)}
                        />
                      </Box>
                    </TableCell>
                    <TableCell align="center">
                      <Typography variant="h6">
                        {activity.voteCount}
                      </Typography>
                    </TableCell>
                    <TableCell align="center">
                      <Box sx={{
                        width: isMobile ? 150 : 200,
                        mx: 'auto'
                      }}>
                        {Object.entries(activity.voteDistribution)
                          .sort(([a], [b]) => parseInt(b) - parseInt(a))
                          .map(([vote, count]) => {
                            const percentage = (count / activity.voteCount) * 100;
                            return (
                              <Box key={vote} display="flex" alignItems="center" mb={0.5}>
                                <Typography variant="caption" sx={{ minWidth: 15 }}>
                                  {vote}
                                </Typography>
                                <LinearProgress
                                  variant="determinate"
                                  value={percentage}
                                  sx={{ flex: 1, mx: 0.5, height: 6 }}
                                  color={getVoteDistributionColor(percentage)}
                                />
                                <Typography variant="caption" sx={{ minWidth: 20 }}>
                                  {count}
                                </Typography>
                              </Box>
                            );
                          })}
                      </Box>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </TabPanel>

        {/* Participants Tab */}
        <TabPanel value={activeTab} index={2}>
          {participantsLoading ? (
            <Box display="flex" justifyContent="center" py={4}>
              <CircularProgress />
            </Box>
          ) : participantsError ? (
            <Alert severity="error">
              Error loading participants. Please try again.
            </Alert>
          ) : participants && participants.length > 0 ? (
            <>
              {/* Participants Summary */}
              <Box
                display="grid"
                gridTemplateColumns={{
                  xs: 'repeat(2, 1fr)',
                  sm: 'repeat(auto-fit, minmax(200px, 1fr))'
                }}
                gap={3}
                sx={{ mb: 4 }}
              >
                <Card>
                  <CardContent sx={{ textAlign: 'center' }}>
                    <Typography variant="h4" fontWeight="bold" color="primary">
                      {participants.length}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Total Participants
                    </Typography>
                  </CardContent>
                </Card>

                <Card>
                  <CardContent sx={{ textAlign: 'center' }}>
                    <Typography variant="h4" fontWeight="bold" color="success.main">
                      {participants.filter(p => p.isCompleted).length}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Completed
                    </Typography>
                  </CardContent>
                </Card>

                <Card>
                  <CardContent sx={{ textAlign: 'center' }}>
                    <Typography variant="h4" fontWeight="bold" color="warning.main">
                      {participants.filter(p => !p.isCompleted).length}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      In Progress
                    </Typography>
                  </CardContent>
                </Card>

                <Card>
                  <CardContent sx={{ textAlign: 'center' }}>
                    <Typography variant="h4" fontWeight="bold" color="info.main">
                      {participants.reduce((sum, p) => sum + p.childrenCount, 0)}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Total Children
                    </Typography>
                  </CardContent>
                </Card>
              </Box>

              {/* Participants Table */}
              <TableContainer component={Paper} sx={{ overflowX: 'auto' }}>
                <Table sx={{ minWidth: isMobile ? 500 : 'auto' }}>
                  <TableHead>
                    <TableRow>
                      <TableCell>Name</TableCell>
                      <TableCell>Registration Date</TableCell>
                      <TableCell align="center">Children</TableCell>
                      <TableCell align="center">Status</TableCell>
                      <TableCell>Completion Date</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {participants.map((participant) => (
                      <TableRow key={participant.id} hover>
                        <TableCell>
                          <Typography variant="subtitle2" fontWeight="medium">
                            {participant.firstName} {participant.lastName}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2">
                            {isSmallMobile
                              ? new Date(participant.participatedAt).toLocaleDateString('en-US', {
                                  month: 'short',
                                  day: 'numeric'
                                })
                              : formatDate(participant.participatedAt)
                            }
                          </Typography>
                        </TableCell>
                        <TableCell align="center">
                          <Typography variant="body2">
                            {participant.childrenCount}
                          </Typography>
                        </TableCell>
                        <TableCell align="center">
                          <Chip
                            label={participant.isCompleted ? 'Completed' : 'In Progress'}
                            color={participant.isCompleted ? 'success' : 'warning'}
                            size="small"
                            icon={participant.isCompleted ? <CheckCircle /> : <Pending />}
                          />
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2">
                            {participant.completedAt
                              ? (isSmallMobile
                                  ? new Date(participant.completedAt).toLocaleDateString('en-US', {
                                      month: 'short',
                                      day: 'numeric'
                                    })
                                  : formatDate(participant.completedAt)
                                )
                              : '-'
                            }
                          </Typography>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </>
          ) : (
            <Box sx={{ p: 4, textAlign: 'center' }}>
              <Typography variant="h6" color="text.secondary" gutterBottom>
                No participants found
              </Typography>
              <Typography variant="body2" color="text.secondary">
                This survey doesn't have any participants yet.
              </Typography>
            </Box>
          )}
        </TabPanel>
      </Paper>
    </Container>
    </AdminRoute>
  );
};

export default SurveyResults;