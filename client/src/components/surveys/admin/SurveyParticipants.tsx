import React from 'react';
import {
  Container,
  Paper,
  Typography,
  Box,
  Button,
  Alert,
  CircularProgress,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
} from '@mui/material';
import {
  ArrowBack,
  Refresh,
  CheckCircle,
  Pending,
} from '@mui/icons-material';
import { useNavigate, useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import AdminRoute from '../../../components/AdminRoute';
import SurveyService from '../../../services/surveyService';
import { SurveyParticipant } from '../../../types/survey.types';

const SurveyParticipants: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  // Fetch survey participants
  const {
    data: participantsResponse,
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: ['survey-participants', id],
    queryFn: () => SurveyService.getSurveyParticipants(id!),
    enabled: !!id,
    staleTime: 30000,
  });

  const participants: SurveyParticipant[] | undefined = participantsResponse?.data;

  const handleBack = () => {
    navigate('/admin/surveys');
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

  if (isLoading) {
    return (
      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Box display="flex" justifyContent="center" py={8}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (error || !participants) {
    return (
      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Alert severity="error">
          Error loading survey participants. Please try again.
        </Alert>
      </Container>
    );
  }

  return (
    <AdminRoute>
      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Box display="flex" alignItems="center" justifyContent="space-between" mb={3}>
          <Box display="flex" alignItems="center">
            <Button
              startIcon={<ArrowBack />}
              onClick={handleBack}
              sx={{ mr: 2 }}
            >
              Back to Surveys
            </Button>
            <Box>
              <Typography variant="h4" component="h1" fontWeight="bold">
                Survey Participants
              </Typography>
              <Typography variant="h6" color="text.secondary">
                Survey ID: {id}
              </Typography>
            </Box>
          </Box>

          <Box display="flex" gap={2}>
            <Button
              variant="outlined"
              startIcon={<Refresh />}
              onClick={() => refetch()}
            >
              Refresh
            </Button>
          </Box>
        </Box>

        {/* Summary */}
        <Box display="grid" gridTemplateColumns="repeat(auto-fit, minmax(200px, 1fr))" gap={3} sx={{ mb: 4 }}>
          <Paper sx={{ p: 3, textAlign: 'center' }}>
            <Typography variant="h4" fontWeight="bold" color="primary">
              {participants.length}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Total Participants
            </Typography>
          </Paper>

          <Paper sx={{ p: 3, textAlign: 'center' }}>
            <Typography variant="h4" fontWeight="bold" color="success.main">
              {participants.filter(p => p.isCompleted).length}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Completed
            </Typography>
          </Paper>

          <Paper sx={{ p: 3, textAlign: 'center' }}>
            <Typography variant="h4" fontWeight="bold" color="warning.main">
              {participants.filter(p => !p.isCompleted).length}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              In Progress
            </Typography>
          </Paper>

          <Paper sx={{ p: 3, textAlign: 'center' }}>
            <Typography variant="h4" fontWeight="bold" color="info.main">
              {participants.reduce((sum, p) => sum + p.childrenCount, 0)}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Total Children
            </Typography>
          </Paper>
        </Box>

        {/* Participants Table */}
        <Paper>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Registration Date</TableCell>
                  <TableCell align="center">Children Count</TableCell>
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
                        {formatDate(participant.participatedAt)}
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
                        {participant.completedAt ? formatDate(participant.completedAt) : '-'}
                      </Typography>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

          {participants.length === 0 && (
            <Box sx={{ p: 4, textAlign: 'center' }}>
              <Typography variant="h6" color="text.secondary" gutterBottom>
                No participants found
              </Typography>
              <Typography variant="body2" color="text.secondary">
                This survey doesn't have any participants yet.
              </Typography>
            </Box>
          )}
        </Paper>
      </Container>
    </AdminRoute>
  );
};

export default SurveyParticipants;