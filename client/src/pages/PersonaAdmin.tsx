import React, { useState, useEffect } from 'react';
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
  Person as PersonIcon,
} from '@mui/icons-material';
import api from '../services/api';
import { useTranslation } from '../hooks/useTranslation';
import PersonaForm from '../components/admin/PersonaForm';
import PersonaDetails from '../components/admin/PersonaDetails';

interface Persona {
  id: string;
  userId: string;
  name: string;
  description?: string;
  avatarImageUrl?: string;
  age?: number;
  gender?: string;
  nationality?: string;
  biography?: string;
  characteristics: PersonaCharacteristic[];
  activityAssociations: PersonaActivityAssociation[];
  images?: PersonaImage[];
  createdAt: string;
  updatedAt: string;
}

interface PersonaCharacteristic {
  id: string;
  personaId: string;
  name: string;
  value: string;
  type?: string;
  order: number;
  createdAt: string;
  updatedAt: string;
}

interface PersonaActivityAssociation {
  id: string;
  personaId: string;
  activityId: string;
  activityName: string;
  preferenceLevel: number;
  createdAt: string;
  updatedAt: string;
}

interface PersonaImage {
  id: string;
  personaId?: string;
  preSignedUrl: string;
  fileName: string;
  imageType: string;
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
      id={`persona-admin-tabpanel-${index}`}
      aria-labelledby={`persona-admin-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

const PersonaAdmin: React.FC = () => {
  const { t } = useTranslation();
  console.log('[PersonaAdmin] Component rendered');

  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  const [activeTab, setActiveTab] = useState(0);
  const [personas, setPersonas] = useState<Persona[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [detailsOpen, setDetailsOpen] = useState(false);
  const [selectedPersona, setSelectedPersona] = useState<Persona | null>(null);

  useEffect(() => {
    console.log('[PersonaAdmin] useEffect triggered, calling loadData');
    loadData();
  }, []);

  const loadData = async () => {
    console.log('[PersonaAdmin] loadData called');
    try {
      setLoading(true);
      setError(null);
      console.log('[PersonaAdmin] Making API call for personas...');

      const response = await api.get('/personas', {
        params: {
          pageNumber: 1,
          pageSize: 100
        }
      });

      console.log('[PersonaAdmin] Personas response:', response);

      // The API returns the paginated result directly in response.data
      const personas = response.data.data?.items || response.data.items || [];
      console.log('[PersonaAdmin] Personas loaded:', personas.length);
      console.log('[PersonaAdmin] Full response data:', response.data);
      console.log('[PersonaAdmin] Personas data:', personas);
      setPersonas(personas);
    } catch (err: any) {
      console.error('[PersonaAdmin] Error loading personas:', err);
      setError(t('personaAdmin.failedToLoad'));
    } finally {
      setLoading(false);
      console.log('[PersonaAdmin] loadData completed');
    }
  };

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
  };

  const handleCreatePersona = () => {
    setSelectedPersona(null);
    setFormOpen(true);
  };

  const handleEditPersona = (persona: Persona) => {
    setSelectedPersona(persona);
    setFormOpen(true);
  };

  const handleViewPersona = (persona: Persona) => {
    setSelectedPersona(persona);
    setDetailsOpen(true);
  };

  const handleDeletePersona = async (persona: Persona) => {
    const confirmText = t('personaAdmin.confirmDelete').replace('{0}', persona.name);
    if (!window.confirm(confirmText)) {
      return;
    }

    try {
      await api.delete(`/personas/${persona.id}`);
      await loadData(); // Refresh the list
    } catch (err: any) {
      console.error('Error deleting persona:', err);
      setError(t('personaAdmin.failedToDelete'));
    }
  };

  const handleFormClose = () => {
    setFormOpen(false);
    setSelectedPersona(null);
  };

  const handleFormSuccess = async () => {
    setFormOpen(false);
    setSelectedPersona(null);
    await loadData(); // Refresh the list
  };

  const handleDetailsClose = () => {
    setDetailsOpen(false);
    setSelectedPersona(null);
  };

  const formatCharacteristics = (characteristics: PersonaCharacteristic[]) => {
    if (!characteristics || characteristics.length === 0) return t('personaAdmin.noCharacteristics');
    return characteristics.slice(0, 3).map(c => `${c.name}: ${c.value}`).join(', ') +
           (characteristics.length > 3 ? ` (+${characteristics.length - 3} ${t('personaAdmin.moreCharacteristics')})` : '');
  };

  const getActivityCount = (activityAssociations: PersonaActivityAssociation[]) => {
    return activityAssociations?.length || 0;
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
      <Typography variant="h4" component="h1" gutterBottom sx={{ color: '#333', fontWeight: 600 }}>
        {t('personaAdmin.title')}
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Paper sx={{ width: '100%', mb: 2, boxShadow: '0 2px 10px rgba(0, 0, 0, 0.1)' }}>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={activeTab} onChange={handleTabChange} aria-label="persona admin tabs">
            <Tab label={t('personaAdmin.tabPersonas')} id="persona-admin-tab-0" aria-controls="persona-admin-tabpanel-0" />
          </Tabs>
        </Box>

        <TabPanel value={activeTab} index={0}>
          <Box sx={{ mb: 2 }}>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={handleCreatePersona}
              sx={{ mb: 2 }}
            >
              {t('personaAdmin.createPersona')}
            </Button>
          </Box>

          {/* Association Management Section */}
          <Box sx={{ mt: 4, mb: 2 }}>
            <Typography variant="h6" gutterBottom>
              {t('personaAdmin.activityAssociationManagement')}
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              {t('personaAdmin.activityAssociationDescription')}
            </Typography>
            <Alert severity="info" sx={{ mb: 2 }}>
              <Typography variant="body2">
                {t('personaAdmin.activityAssociationTip')}
              </Typography>
            </Alert>
          </Box>

          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>{t('personaAdmin.tableName')}</TableCell>
                  <TableCell>{t('personaAdmin.tableDemographics')}</TableCell>
                  <TableCell>{t('personaAdmin.tableDescription')}</TableCell>
                  <TableCell>{t('personaAdmin.tableCharacteristics')}</TableCell>
                  <TableCell>{t('personaAdmin.tableActivities')}</TableCell>
                  <TableCell>{t('personaAdmin.tableCreated')}</TableCell>
                  <TableCell align="right">{t('personaAdmin.tableActions')}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {personas.map((persona) => {
                  const avatar = persona.avatarImageUrl || persona.images?.find(img => img.imageType === 'original')?.preSignedUrl;

                  return (
                  <TableRow key={persona.id} hover>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <PersonIcon sx={{ mr: 1, color: 'primary.main' }} />
                        <Box>
                          <Typography variant="body1" fontWeight="medium">
                            {persona.name}
                          </Typography>
                          {avatar && (
                            <Typography variant="body2" color="text.secondary">
                              {t('personaAdmin.hasAvatar')}
                            </Typography>
                          )}
                        </Box>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
                        {persona.age && (
                          <Typography variant="body2">
                            {t('personaAdmin.age')}: {persona.age}
                          </Typography>
                        )}
                        {persona.gender && (
                          <Typography variant="body2">
                            {t('personaAdmin.gender')}: {persona.gender}
                          </Typography>
                        )}
                        {persona.nationality && (
                          <Typography variant="body2">
                            {t('personaAdmin.nationality')}: {persona.nationality}
                          </Typography>
                        )}
                        {!persona.age && !persona.gender && !persona.nationality && (
                          <Typography variant="body2" color="text.secondary">
                            {t('personaAdmin.noDemographics')}
                          </Typography>
                        )}
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">
                        {persona.description || t('personaAdmin.noDescription')}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">
                        {formatCharacteristics(persona.characteristics)}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={`${getActivityCount(persona.activityAssociations)} ${t('personaAdmin.activitiesCount')}`}
                        size="small"
                        variant="outlined"
                      />
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">
                        {new Date(persona.createdAt).toLocaleDateString()}
                      </Typography>
                    </TableCell>
                    <TableCell align="right">
                      <IconButton
                        size="small"
                        onClick={() => handleViewPersona(persona)}
                        color="info"
                        title={t('personaAdmin.viewDetails')}
                      >
                        <PersonIcon />
                      </IconButton>
                      <IconButton
                        size="small"
                        onClick={() => handleEditPersona(persona)}
                        color="primary"
                      >
                        <EditIcon />
                      </IconButton>
                      <IconButton
                        size="small"
                        onClick={() => handleDeletePersona(persona)}
                        color="error"
                      >
                        <DeleteIcon />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                );
                })}
                {personas.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                      <Typography variant="body1" color="text.secondary">
                        {t('personaAdmin.noPersonasFound')}
                      </Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </TabPanel>
      </Paper>

      {/* Persona Form Dialog */}
      <Dialog
        open={formOpen}
        onClose={handleFormClose}
        maxWidth="md"
        fullWidth
        fullScreen={isMobile}
      >
        <DialogTitle>
          {selectedPersona ? t('personaAdmin.editPersona') : t('personaAdmin.createNewPersona')}
        </DialogTitle>
        <DialogContent>
          <PersonaForm
            persona={selectedPersona}
            onSuccess={handleFormSuccess}
            onCancel={handleFormClose}
          />
        </DialogContent>
      </Dialog>

      {/* Persona Details Dialog */}
      <Dialog
        open={detailsOpen}
        onClose={handleDetailsClose}
        maxWidth="md"
        fullWidth
        fullScreen={isMobile}
      >
        <DialogTitle>
          {t('personaAdmin.personaDetails')}
        </DialogTitle>
        <DialogContent>
          {selectedPersona && (
            <PersonaDetails persona={selectedPersona} />
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleDetailsClose}>{t('personaAdmin.close')}</Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default PersonaAdmin;

