import React, { useState, useEffect } from 'react';
import {
  Container,
  Paper,
  Typography,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Chip,
  Button,
  Box,
  IconButton,
  Tooltip,
  Alert,
  CircularProgress,
  InputAdornment,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Snackbar,
  Card,
  CardContent,
  CardActions,
  useTheme,
  useMediaQuery,
  Stack,
  Divider,
} from '@mui/material';
import {
  Search,
  Add,
  Edit,
  BarChart,
  Delete,
  Refresh,
  FilterList,
  Share,
  ContentCopy,
} from '@mui/icons-material';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import AdminRoute from '../../../components/AdminRoute';
import SurveyService from '../../../services/surveyService';
import { SurveyListItem, GetSurveysParams } from '../../../types/survey.types';

const SurveyList: React.FC = () => {
  const navigate = useNavigate();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const isSmallMobile = useMediaQuery(theme.breakpoints.down('sm'));

  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('');
  const [shareDialogOpen, setShareDialogOpen] = useState(false);
  const [selectedSurveyForShare, setSelectedSurveyForShare] = useState<SurveyListItem | null>(null);
  const [shareUrl, setShareUrl] = useState('');
  const [copySuccess, setCopySuccess] = useState(false);

  // Debounce search term
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm);
    }, 300);

    return () => clearTimeout(timer);
  }, [searchTerm]);

  // Query parameters
  const queryParams: GetSurveysParams = {
    pageNumber: page + 1,
    pageSize: rowsPerPage,
    searchTerm: debouncedSearchTerm || undefined,
    status: statusFilter === 'all' ? undefined : statusFilter,
    sortBy: 'createdAt',
    sortOrder: 'desc',
  };

  // Fetch surveys
  const {
    data: surveysResponse,
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: ['surveys', queryParams],
    queryFn: () => SurveyService.getSurveys(queryParams),
    staleTime: 30000,
  });

  const surveys = surveysResponse?.data?.surveys || [];
  const totalCount = surveysResponse?.data?.totalCount || 0;

  const handleChangePage = (event: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(event.target.value);
    setPage(0);
  };

  const handleStatusFilterChange = (event: any) => {
    setStatusFilter(event.target.value);
    setPage(0);
  };

  const handleCreateSurvey = () => {
    navigate('/admin/surveys/create');
  };

  const handleEditSurvey = (surveyId: string) => {
    navigate(`/admin/surveys/${surveyId}/edit`);
  };

  const handleViewResults = (surveyId: string) => {
    navigate(`/admin/surveys/${surveyId}/results`);
  };



  const handleDeleteSurvey = async (surveyId: string) => {
    if (window.confirm('Are you sure you want to delete this survey?')) {
      try {
        await SurveyService.deleteSurvey(surveyId);
        refetch();
      } catch (error) {
        console.error('Error deleting survey:', error);
      }
    }
  };

  const handleShareSurvey = async (survey: SurveyListItem) => {
    try {
      const response = await SurveyService.getShareUrl(survey.id);
      const shareUrl = response.data.shareUrl;
      setShareUrl(shareUrl);
      setSelectedSurveyForShare(survey);
      setShareDialogOpen(true);
    } catch (error) {
      console.error('Error getting share URL:', error);
    }
  };

  const handleCopyToClipboard = async () => {
    try {
      await navigator.clipboard.writeText(shareUrl);
      setCopySuccess(true);
      setTimeout(() => setCopySuccess(false), 2000);
    } catch (error) {
      console.error('Error copying to clipboard:', error);
    }
  };

  const handleCloseShareDialog = () => {
    setShareDialogOpen(false);
    setSelectedSurveyForShare(null);
    setShareUrl('');
  };

  const getStatusColor = (survey: SurveyListItem) => {
    if (!survey.isActive) return 'error';

    const now = new Date();
    const startDate = new Date(survey.startDate);
    const endDate = survey.endDate ? new Date(survey.endDate) : null;

    if (now < startDate) return 'info';
    if (endDate && now > endDate) return 'default';
    if (survey.maxParticipants && survey.participantCount >= survey.maxParticipants) return 'warning';

    return 'success';
  };

  const getStatusText = (survey: SurveyListItem) => {
    if (!survey.isActive) return 'Inactive';

    const now = new Date();
    const startDate = new Date(survey.startDate);
    const endDate = survey.endDate ? new Date(survey.endDate) : null;

    if (now < startDate) return 'Scheduled';
    if (endDate && now > endDate) return 'Completed';
    if (survey.maxParticipants && survey.participantCount >= survey.maxParticipants) return 'Full';

    return 'Active';
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  return (
    <AdminRoute>
      <Container maxWidth="xl" sx={{ py: { xs: 2, sm: 3, md: 4 } }}>
        <Box
          display="flex"
          flexDirection={{ xs: 'column', sm: 'row' }}
          justifyContent={{ xs: 'flex-start', sm: 'space-between' }}
          alignItems={{ xs: 'flex-start', sm: 'center' }}
          gap={{ xs: 2, sm: 0 }}
          mb={3}
        >
          <Typography
            variant={isSmallMobile ? 'h5' : 'h4'}
            component="h1"
            fontWeight="bold"
          >
            Survey Management
          </Typography>
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={handleCreateSurvey}
            size={isSmallMobile ? 'medium' : 'large'}
            fullWidth={isSmallMobile}
            sx={{ alignSelf: { xs: 'flex-start', sm: 'auto' } }}
          >
            Create Survey
          </Button>
        </Box>

      {/* Filters */}
      <Paper sx={{ p: { xs: 2, sm: 3 }, mb: 3 }}>
        <Stack
          direction={{ xs: 'column', sm: 'row' }}
          spacing={{ xs: 2, sm: 2 }}
          alignItems={{ xs: 'stretch', sm: 'center' }}
        >
          <TextField
            label="Search surveys"
            variant="outlined"
            value={searchTerm}
            onChange={handleSearchChange}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <Search />
                </InputAdornment>
              ),
            }}
            fullWidth={isSmallMobile}
            sx={{
              minWidth: { xs: '100%', sm: 250 },
              flex: { sm: 1 }
            }}
          />

          <FormControl
            variant="outlined"
            fullWidth={isSmallMobile}
            sx={{
              minWidth: { xs: '100%', sm: 150 },
              flex: { sm: 'none' }
            }}
          >
            <InputLabel>Status</InputLabel>
            <Select
              value={statusFilter}
              onChange={handleStatusFilterChange}
              label="Status"
              startAdornment={
                <FilterList sx={{ mr: 1, color: 'text.secondary' }} />
              }
            >
              <MenuItem value="all">All Status</MenuItem>
              <MenuItem value="active">Active</MenuItem>
              <MenuItem value="scheduled">Scheduled</MenuItem>
              <MenuItem value="completed">Completed</MenuItem>
              <MenuItem value="inactive">Inactive</MenuItem>
            </Select>
          </FormControl>

          <Button
            variant="outlined"
            startIcon={<Refresh />}
            onClick={() => refetch()}
            disabled={isLoading}
            fullWidth={isSmallMobile}
            sx={{ alignSelf: { xs: 'flex-start', sm: 'auto' } }}
          >
            Refresh
          </Button>
        </Stack>
      </Paper>

      {/* Error State */}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          Error loading surveys. Please try again.
        </Alert>
      )}

      {/* Loading State */}
      {isLoading && (
        <Box display="flex" justifyContent="center" py={4}>
          <CircularProgress />
        </Box>
      )}

      {/* Surveys List */}
      {!isLoading && !error && (
        <Box>
          {/* Desktop Table View */}
          {!isMobile && (
            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Title</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Participants</TableCell>
                    <TableCell>Start Date</TableCell>
                    <TableCell>End Date</TableCell>
                    <TableCell>Created By</TableCell>
                    <TableCell align="center">Actions</TableCell>
                    <TableCell align="center">Share</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {surveys.map((survey) => (
                    <TableRow key={survey.id} hover>
                      <TableCell>
                        <Typography variant="subtitle1" fontWeight="medium">
                          {survey.title}
                        </Typography>
                        {survey.description && (
                          <Typography variant="body2" color="text.secondary">
                            {survey.description}
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={getStatusText(survey)}
                          color={getStatusColor(survey)}
                          size="small"
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {survey.participantCount}
                          {survey.maxParticipants && ` / ${survey.maxParticipants}`}
                        </Typography>
                      </TableCell>
                      <TableCell>{formatDate(survey.startDate)}</TableCell>
                      <TableCell>
                        {survey.endDate ? formatDate(survey.endDate) : 'No end date'}
                      </TableCell>
                      <TableCell>{survey.createdByUserName}</TableCell>
                      <TableCell align="center">
                        <Box display="flex" gap={1} justifyContent="center">
                          <Tooltip title="Edit Survey">
                            <IconButton
                              size="small"
                              onClick={() => handleEditSurvey(survey.id)}
                              color="primary"
                            >
                              <Edit />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="View Results">
                            <IconButton
                              size="small"
                              onClick={() => handleViewResults(survey.id)}
                              color="info"
                            >
                              <BarChart />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete Survey">
                            <IconButton
                              size="small"
                              onClick={() => handleDeleteSurvey(survey.id)}
                              color="error"
                            >
                              <Delete />
                            </IconButton>
                          </Tooltip>
                        </Box>
                      </TableCell>
                      <TableCell align="center">
                        <Tooltip title="Share Survey">
                          <IconButton
                            size="small"
                            onClick={() => handleShareSurvey(survey)}
                            color="success"
                          >
                            <Share />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>

              {/* Pagination */}
              <TablePagination
                rowsPerPageOptions={[5, 10, 25, 50]}
                component="div"
                count={totalCount}
                rowsPerPage={rowsPerPage}
                page={page}
                onPageChange={handleChangePage}
                onRowsPerPageChange={handleChangeRowsPerPage}
              />
            </TableContainer>
          )}

          {/* Mobile Card View */}
          {isMobile && (
            <Stack spacing={2}>
              {surveys.map((survey) => (
                <Card key={survey.id} sx={{ width: '100%' }}>
                  <CardContent sx={{ pb: 1 }}>
                    <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={2}>
                      <Box flex={1} mr={2}>
                        <Typography variant="h6" fontWeight="bold" gutterBottom>
                          {survey.title}
                        </Typography>
                        {survey.description && (
                          <Typography variant="body2" color="text.secondary" mb={1}>
                            {survey.description}
                          </Typography>
                        )}
                        <Box display="flex" alignItems="center" gap={1} mb={1}>
                          <Chip
                            label={getStatusText(survey)}
                            color={getStatusColor(survey)}
                            size="small"
                          />
                          <Typography variant="body2" color="text.secondary">
                            {survey.participantCount}
                            {survey.maxParticipants && ` / ${survey.maxParticipants}`} participants
                          </Typography>
                        </Box>
                      </Box>
                      <Tooltip title="Share Survey">
                        <IconButton
                          onClick={() => handleShareSurvey(survey)}
                          color="success"
                          size="small"
                        >
                          <Share />
                        </IconButton>
                      </Tooltip>
                    </Box>

                    <Stack direction="row" spacing={1} flexWrap="wrap" mb={2}>
                      <Typography variant="caption" color="text.secondary">
                        Start: {formatDate(survey.startDate)}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        •
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        End: {survey.endDate ? formatDate(survey.endDate) : 'No end date'}
                      </Typography>
                    </Stack>

                    <Typography variant="caption" color="text.secondary">
                      Created by: {survey.createdByUserName}
                    </Typography>
                  </CardContent>

                  <Divider />

                  <CardActions sx={{ justifyContent: 'space-between', px: 2, py: 1 }}>
                    <Stack direction="row" spacing={1}>
                      <Tooltip title="Edit Survey">
                        <IconButton
                          size="small"
                          onClick={() => handleEditSurvey(survey.id)}
                          color="primary"
                        >
                          <Edit />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="View Results">
                        <IconButton
                          size="small"
                          onClick={() => handleViewResults(survey.id)}
                          color="info"
                        >
                          <BarChart />
                        </IconButton>
                      </Tooltip>
                    </Stack>

                    <Tooltip title="Delete Survey">
                      <IconButton
                        size="small"
                        onClick={() => handleDeleteSurvey(survey.id)}
                        color="error"
                      >
                        <Delete />
                      </IconButton>
                    </Tooltip>
                  </CardActions>
                </Card>
              ))}

              {/* Mobile Pagination */}
              <Box display="flex" justifyContent="center" mt={3}>
                <TablePagination
                  rowsPerPageOptions={[5, 10, 25, 50]}
                  component="div"
                  count={totalCount}
                  rowsPerPage={rowsPerPage}
                  page={page}
                  onPageChange={handleChangePage}
                  onRowsPerPageChange={handleChangeRowsPerPage}
                  size="small"
                />
              </Box>
            </Stack>
          )}
        </Box>
      )}

      {/* Empty State */}
      {!isLoading && !error && surveys.length === 0 && (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Typography variant="h6" color="text.secondary" gutterBottom>
            No surveys found
          </Typography>
          <Typography variant="body2" color="text.secondary" mb={3}>
            {searchTerm || statusFilter !== 'all'
              ? 'Try adjusting your search or filter criteria.'
              : 'Get started by creating your first survey.'}
          </Typography>
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={handleCreateSurvey}
          >
            Create Survey
          </Button>
        </Paper>
      )}

      {/* Share Dialog */}
      <Dialog open={shareDialogOpen} onClose={handleCloseShareDialog} maxWidth="sm" fullWidth>
        <DialogTitle>
          Share Survey: {selectedSurveyForShare?.title}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Share this link with participants to allow them to vote on the survey:
            </Typography>
            <TextField
              fullWidth
              value={shareUrl}
              InputProps={{
                readOnly: true,
                endAdornment: (
                  <IconButton onClick={handleCopyToClipboard} edge="end">
                    <ContentCopy />
                  </IconButton>
                ),
              }}
              variant="outlined"
              sx={{ mt: 1 }}
            />
            <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
              Share Token: {selectedSurveyForShare?.shareToken}
            </Typography>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseShareDialog}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Copy Success Snackbar */}
      <Snackbar
        open={copySuccess}
        autoHideDuration={2000}
        onClose={() => setCopySuccess(false)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert severity="success" sx={{ width: '100%' }}>
          Link copied to clipboard!
        </Alert>
      </Snackbar>
    </Container>
    </AdminRoute>
  );
};

export default SurveyList;