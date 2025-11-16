import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Paper,
  Typography,
  Autocomplete,
  TextField,
  Button,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  IconButton,
  Slider,
  Chip,
  Alert,
  CircularProgress,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
} from '@mui/icons-material';
import { personasAPI } from '../../services/api';

interface Persona {
  id: string;
  name: string;
  description?: string;
  avatarImageUrl?: string;
  age?: number;
  gender?: string;
  nationality?: string;
  biography?: string;
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

interface PersonaAssociationsTabProps {
  activityId?: string;
}

const PersonaAssociationsTab: React.FC<PersonaAssociationsTabProps> = ({ activityId }) => {
  const [personas, setPersonas] = useState<Persona[]>([]);
  const [associations, setAssociations] = useState<PersonaActivityAssociation[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedPersona, setSelectedPersona] = useState<Persona | null>(null);
  const [preferenceLevel, setPreferenceLevel] = useState<number>(3);
  const [addingAssociation, setAddingAssociation] = useState(false);

  // Load personas for autocomplete
  const loadPersonas = useCallback(async () => {
    try {
      const response = await personasAPI.getPersonas({ pageSize: 100 });
      console.log('=== PERSONA API RESPONSE DEBUG ===');
      console.log('Full API response:', response);
      console.log('Response status:', response.status);
      console.log('Response headers:', response.headers);
      console.log('Response data type:', typeof response.data);
      console.log('Response data keys:', Object.keys(response.data));
      console.log('Response data structure:', JSON.stringify(response.data, null, 2));

      // Check for different possible structures
      console.log('response.data.success:', response.data.success);
      console.log('response.data.items:', response.data.items);
      console.log('response.data.data:', response.data.data);
      console.log('response.data directly as array?', Array.isArray(response.data));
      console.log('response.data.items as array?', Array.isArray(response.data.items));
      console.log('response.data.data as array?', Array.isArray(response.data.data));

      // Try to extract personas from different possible locations
      let personasArray = null;
      if (Array.isArray(response.data)) {
        personasArray = response.data;
        console.log('Personas found directly in response.data (array)');
      } else if (response.data.items && Array.isArray(response.data.items)) {
        personasArray = response.data.items;
        console.log('Personas found in response.data.items');
      } else if (response.data.data && Array.isArray(response.data.data)) {
        personasArray = response.data.data;
        console.log('Personas found in response.data.data');
      } else {
        console.log('Could not find personas array in expected locations');
      }

      // Based on the API structure, personas are in response.data.items
      if (response.data.success && response.data.items && Array.isArray(response.data.items)) {
        setPersonas(response.data.items);
        console.log('Set personas in state:', response.data.items);
      } else if (personasArray) {
        setPersonas(personasArray);
        console.log('Set personas in state (fallback):', personasArray);
      } else {
        console.log('Response does not indicate success or no personas found');
        setPersonas([]);
      }
    } catch (err: any) {
      console.error('Failed to load personas:', err);
      console.error('Error details:', err.response?.data);
    }
  }, []);

  // Load existing associations for the activity
  const loadAssociations = useCallback(async () => {
    if (!activityId) return;

    try {
      setLoading(true);
      console.log('=== LOAD ASSOCIATIONS DEBUG ===');
      console.log('Loading associations for activityId:', activityId);

      const response = await personasAPI.getActivityPersonaAssociations(activityId);
      console.log('API Response received:', response);
      console.log('Response status:', response.status);
      console.log('Response data type:', typeof response.data);
      console.log('Response data keys:', Object.keys(response.data));
      console.log('Response data structure:', JSON.stringify(response.data, null, 2));
      console.log('response.data.success:', response.data.success);
      console.log('response.data.data:', response.data.data);
      console.log('response.data directly as array?', Array.isArray(response.data));

      // The API returns the data directly as an array, not wrapped in a success/data structure
      if (Array.isArray(response.data)) {
        console.log('Setting associations directly from response.data (array):', response.data);
        setAssociations(response.data);
      } else {
        console.log('Response is not an array, setting empty associations');
        setAssociations([]);
      }

      console.log('Final associations state:', Array.isArray(response.data) ? response.data : []);
    } catch (err: any) {
      console.error('Failed to load associations:', err);
      console.error('Error response:', err.response?.data);
      setError('Failed to load persona associations');
    } finally {
      setLoading(false);
    }
  }, [activityId]);

  useEffect(() => {
    loadPersonas();
    loadAssociations();
  }, [loadPersonas, loadAssociations]);

  const handleAddAssociation = async () => {
    if (!selectedPersona || !activityId) return;

    try {
      setAddingAssociation(true);
      setError(null);

      const response = await personasAPI.createActivityPersonaAssociation(activityId, {
        personaId: selectedPersona.id,
        preferenceLevel,
      });

      if (response.data.success) {
        setAssociations(prev => [...prev, response.data.data]);
        setSelectedPersona(null);
        setPreferenceLevel(3);
      }
    } catch (err: any) {
      console.error('Failed to add association:', err);
      setError(err.response?.data?.message || 'Failed to add persona association');
    } finally {
      setAddingAssociation(false);
    }
  };

  const handleRemoveAssociation = async (associationId: string) => {
    try {
      setError(null);
      await personasAPI.deleteActivityPersonaAssociation(associationId);
      setAssociations(prev => prev.filter(a => a.id !== associationId));
    } catch (err: any) {
      console.error('Failed to remove association:', err);
      setError(err.response?.data?.message || 'Failed to remove persona association');
    }
  };

  const handleUpdatePreferenceLevel = async (associationId: string, newLevel: number) => {
    try {
      setError(null);
      const response = await personasAPI.updateActivityPersonaAssociation(associationId, {
        preferenceLevel: newLevel,
      });

      if (response.data.success) {
        setAssociations(prev =>
          prev.map(a =>
            a.id === associationId
              ? { ...a, preferenceLevel: newLevel, updatedAt: response.data.data.updatedAt }
              : a
          )
        );
      }
    } catch (err: any) {
      console.error('Failed to update preference level:', err);
      setError(err.response?.data?.message || 'Failed to update preference level');
    }
  };

  const getPreferenceLevelLabel = (level: number) => {
    switch (level) {
      case 1: return 'Not suitable';
      case 2: return 'Poor fit';
      case 3: return 'Neutral';
      case 4: return 'Good fit';
      case 5: return 'Excellent fit';
      default: return 'Unknown';
    }
  };

  const getPreferenceLevelColor = (level: number) => {
    if (level <= 2) return 'error';
    if (level === 3) return 'default';
    if (level === 4) return 'primary';
    return 'success';
  };

  if (!activityId) {
    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom>
            Persona Associations
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Save the activity first to manage persona associations.
          </Typography>
        </Paper>
      </Box>
    );
  }

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {/* Add new association */}
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Add Persona Association
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Associate this activity with personas to define which user types this activity is most suitable for.
        </Typography>

        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', flexWrap: 'wrap' }}>
          <Autocomplete
            options={personas}
            getOptionLabel={(option) => option.name}
            value={selectedPersona}
            onChange={(_, newValue) => setSelectedPersona(newValue)}
            renderInput={(params) => (
              <TextField
                {...params}
                label="Select Persona"
                sx={{ minWidth: '200px', flex: 1 }}
              />
            )}
            renderOption={(props, option) => (
              <li {...props}>
                <Box>
                  <Typography variant="body1">{option.name}</Typography>
                  {option.description && (
                    <Typography variant="body2" color="text.secondary">
                      {option.description}
                    </Typography>
                  )}
                </Box>
              </li>
            )}
          />

          <Box sx={{ minWidth: '200px' }}>
            <Typography variant="body2" gutterBottom>
              Preference Level: {preferenceLevel} - {getPreferenceLevelLabel(preferenceLevel)}
            </Typography>
            <Slider
              value={preferenceLevel}
              onChange={(_, value) => setPreferenceLevel(value as number)}
              min={1}
              max={5}
              step={1}
              marks={[
                { value: 1, label: '1' },
                { value: 2, label: '2' },
                { value: 3, label: '3' },
                { value: 4, label: '4' },
                { value: 5, label: '5' },
              ]}
              valueLabelDisplay="auto"
            />
          </Box>

          <Button
            variant="contained"
            startIcon={addingAssociation ? <CircularProgress size={16} /> : <AddIcon />}
            onClick={handleAddAssociation}
            disabled={!selectedPersona || addingAssociation}
          >
            {addingAssociation ? 'Adding...' : 'Add Association'}
          </Button>
        </Box>
      </Paper>

      {/* Existing associations */}
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Current Associations ({associations.length})
        </Typography>

        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
            <CircularProgress />
          </Box>
        ) : associations.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No persona associations yet. Add some above to define which user types this activity is suitable for.
          </Typography>
        ) : (
          <List>
            {associations.map((association) => {
              const persona = personas.find(p => p.id === association.personaId);
              return (
                <ListItem key={association.id} divider>
                  <ListItemText
                    primary={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography variant="body1">
                          {persona?.name || 'Unknown Persona'}
                        </Typography>
                        <Chip
                          label={`${association.preferenceLevel} - ${getPreferenceLevelLabel(association.preferenceLevel)}`}
                          color={getPreferenceLevelColor(association.preferenceLevel)}
                          size="small"
                        />
                      </Box>
                    }
                    secondary={
                      <Box sx={{ mt: 1 }}>
                        <Typography variant="body2" color="text.secondary">
                          {persona?.description || 'No description'}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          Added: {new Date(association.createdAt).toLocaleDateString()}
                        </Typography>
                      </Box>
                    }
                  />
                  <ListItemSecondaryAction>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                      <Box sx={{ minWidth: '150px' }}>
                        <Typography variant="caption" display="block">
                          Preference: {association.preferenceLevel}
                        </Typography>
                        <Slider
                          size="small"
                          value={association.preferenceLevel}
                          onChange={(_, value) => handleUpdatePreferenceLevel(association.id, value as number)}
                          min={1}
                          max={5}
                          step={1}
                          sx={{ mt: 0.5 }}
                        />
                      </Box>
                      <IconButton
                        edge="end"
                        onClick={() => handleRemoveAssociation(association.id)}
                        color="error"
                      >
                        <DeleteIcon />
                      </IconButton>
                    </Box>
                  </ListItemSecondaryAction>
                </ListItem>
              );
            })}
          </List>
        )}
      </Paper>
    </Box>
  );
};

export default PersonaAssociationsTab;