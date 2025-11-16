import React, { useState, useEffect } from 'react';
import { useForm, Controller, useFieldArray } from 'react-hook-form';
import {
  Box,
  TextField,
  Button,
  Typography,
  Paper,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  CircularProgress,
  Grid,
  IconButton,
  Chip,
  Divider,
  Autocomplete,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
} from '@mui/icons-material';
import api from '../../services/api';

interface PersonaCharacteristic {
  id?: string;
  name: string;
  value: string;
  type?: string;
  order: number;
}

interface ActivityOption {
  id: string;
  name: string;
}

interface PersonaActivityAssociation {
  activityId: string;
}

interface PersonaFormData {
  name: string;
  displayName?: string;
  description?: string;
  type?: string;
  age?: number;
  gender?: string;
  nationality?: string;
  biography?: string;
  openness?: number;
  conscientiousness?: number;
  extraversion?: number;
  agreeableness?: number;
  neuroticism?: number;
  characteristics: PersonaCharacteristic[];
  activityAssociations: PersonaActivityAssociation[];
}

interface PersonaFormProps {
  persona?: any; // The persona being edited, if any
  onSuccess: () => void;
  onCancel: () => void;
}

const PERSONA_TYPES = [
  'User',
  'Customer',
  'Employee',
  'Student',
  'Professional',
  'Other'
];

const CHARACTERISTIC_TYPES = [
  'string',
  'number',
  'boolean'
];

// Gender enum mapping (matches backend: Male=1, Female=2, NonBinary=3, Other=4, PreferNotToSay=5)
const GENDER_OPTIONS = [
  { value: 'Male', label: 'Male' },
  { value: 'Female', label: 'Female' },
  { value: 'NonBinary', label: 'Non-binary' },
  { value: 'Other', label: 'Other' },
  { value: 'PreferNotToSay', label: 'Prefer not to say' }
];

// Map integer enum value to string value
const mapGenderEnumToString = (genderValue: any): string => {
  if (typeof genderValue === 'number') {
    switch (genderValue) {
      case 1: return 'Male';
      case 2: return 'Female';
      case 3: return 'NonBinary';
      case 4: return 'Other';
      case 5: return 'PreferNotToSay';
      default: return '';
    }
  }
  return genderValue || '';
};

const PersonaForm: React.FC<PersonaFormProps> = ({
  persona,
  onSuccess,
  onCancel,
}) => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [activities, setActivities] = useState<ActivityOption[]>([]);
  const [activitiesLoading, setActivitiesLoading] = useState(false);
  const [imageFiles, setImageFiles] = useState<File[]>([]);

  const {
    control,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
    reset,
  } = useForm<PersonaFormData>({
    defaultValues: {
      name: '',
      displayName: '',
      description: '',
      type: '',
      age: undefined,
      gender: '',
      nationality: '',
      biography: '',
      openness: 0,
      conscientiousness: 0,
      extraversion: 0,
      agreeableness: 0,
      neuroticism: 0,
      characteristics: [],
      activityAssociations: [],
    },
  });

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'characteristics',
  });

  useEffect(() => {
      if (persona) {
        // Load existing persona data
        reset({
          name: persona.name || '',
          displayName: persona.displayName || '',
          description: persona.description || '',
          type: persona.type || '',
          age: persona.age || undefined,
        gender: mapGenderEnumToString(persona.gender) || '',
        nationality: persona.nationality || '',
        biography: persona.biography || '',
        openness: persona.openness || 0,
        conscientiousness: persona.conscientiousness || 0,
        extraversion: persona.extraversion || 0,
        agreeableness: persona.agreeableness || 0,
        neuroticism: persona.neuroticism || 0,
        characteristics: persona.characteristics || [],
        activityAssociations: persona.activityAssociations?.map((a: any) => ({
          activityId: a.activityId
        })) || [],
      });
    }
  }, [persona, reset]);

  useEffect(() => {
    loadActivities();
  }, []);

  const loadActivities = async () => {
    try {
      setActivitiesLoading(true);
      const response = await api.get('/activities', {
        params: {
          pageSize: 1000,
          sortBy: 'name',
          sortOrder: 'asc'
        }
      });
      if (response.data.success) {
        setActivities(response.data.data?.items || []);
      }
    } catch (err) {
      console.error('Error loading activities:', err);
    } finally {
      setActivitiesLoading(false);
    }
  };

  const addCharacteristic = () => {
    append({
      name: '',
      value: '',
      type: 'string',
      order: fields.length,
    });
  };

  const removeCharacteristic = (index: number) => {
    remove(index);
  };

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (!event.target.files) return;
    setImageFiles(Array.from(event.target.files));
  };

  const onSubmit = async (data: PersonaFormData) => {
    try {
      setLoading(true);
      setError(null);

      // Validate required fields
      if (!data.name?.trim()) {
        setError('Name is required');
        setLoading(false);
        return;
      }

      // Validate personality traits (0-100 range)
      const traits = [data.openness, data.conscientiousness, data.extraversion, data.agreeableness, data.neuroticism];
      for (const trait of traits) {
        if (trait !== undefined && (trait < 0 || trait > 100)) {
          setError('Personality traits must be between 0 and 100');
          setLoading(false);
          return;
        }
      }

      // Validate characteristics
      for (const char of data.characteristics) {
        if (!char.name?.trim() || !char.value?.trim()) {
          setError('All characteristics must have both name and value');
          setLoading(false);
          return;
        }
      }

      // Prepare the payload
      const payload = {
        name: data.name.trim(),
        displayName: data.displayName?.trim() || null,
        description: data.description?.trim() || null,
        type: data.type || null,
        age: data.age || null,
        gender: data.gender || null,
        nationality: data.nationality?.trim() || null,
        biography: data.biography?.trim() || null,
        openness: data.openness || 0,
        conscientiousness: data.conscientiousness || 0,
        extraversion: data.extraversion || 0,
        agreeableness: data.agreeableness || 0,
        neuroticism: data.neuroticism || 0,
        characteristics: data.characteristics.map((char, index) => ({
          ...char,
          name: char.name.trim(),
          value: char.value.trim(),
          order: index,
        })),
      };

      let personaId = persona?.id;

      if (persona?.id) {
        // Update existing persona
        await api.put(`/personas/${persona.id}`, payload);
      } else {
        // Create new persona
        const response = await api.post('/personas', payload);
        personaId = response.data?.id || personaId;
      }

      // Upload images if provided
      if (personaId && imageFiles.length > 0) {
        const formData = new FormData();
        imageFiles.forEach((file) => formData.append('files', file));
        await api.post(`/personas/${personaId}/images`, formData, {
          headers: { 'Content-Type': 'multipart/form-data' }
        });
      }

      onSuccess();
    } catch (err: any) {
      console.error('Error saving persona:', err);
      const errorMessage = err.response?.data?.message ||
                          err.response?.data?.error ||
                          err.message ||
                          'Failed to save persona';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  if (loading && !persona) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box component="form" onSubmit={handleSubmit(onSubmit)} sx={{ mt: 2 }}>
      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          {persona ? 'Edit Persona' : 'Create New Persona'}
        </Typography>

        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
          {/* Basic Information */}
          <Box>
            <Typography variant="subtitle1" gutterBottom sx={{ fontWeight: 'bold' }}>
              Basic Information
            </Typography>
            <Divider sx={{ mb: 2 }} />
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                <Controller
                  name="name"
                  control={control}
                  rules={{ required: 'Name is required' }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Name"
                      sx={{ flex: 1, minWidth: '200px' }}
                      error={!!errors.name}
                      helperText={errors.name?.message}
                      required
                    />
                  )}
                />
                <Controller
                  name="displayName"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Display Name"
                      sx={{ flex: 1, minWidth: '200px' }}
                    />
                  )}
                />
              </Box>

              <Controller
                name="description"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label="Description"
                    fullWidth
                    multiline
                    rows={3}
                  />
                )}
              />

              <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                <Controller
                  name="type"
                  control={control}
                  render={({ field }) => (
                    <FormControl sx={{ minWidth: '200px' }}>
                      <InputLabel>Type</InputLabel>
                      <Select {...field} label="Type">
                        <MenuItem value="">
                          <em>Select type</em>
                        </MenuItem>
                        {PERSONA_TYPES.map((type) => (
                          <MenuItem key={type} value={type}>
                            {type}
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  )}
                />
              </Box>

              <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', flexWrap: 'wrap' }}>
                <Button variant="outlined" component="label" startIcon={<AddIcon />}>
                  Fotoğraf Yükle
                  <input
                    type="file"
                    hidden
                    multiple
                    accept="image/*"
                    onChange={handleFileChange}
                  />
                </Button>
                {imageFiles.length > 0 && (
                  <Typography variant="body2" color="text.secondary">
                    {imageFiles.length} fotoğraf seçildi
                  </Typography>
                )}
              </Box>
              {imageFiles.length > 0 && (
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mt: 1 }}>
                  {imageFiles.map((file, index) => (
                    <Chip key={index} label={file.name} />
                  ))}
                </Box>
              )}
              {persona?.images?.length > 0 && (
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mt: 1 }}>
                  {persona.images
                    .filter((img: any) => img.imageType === 'original')
                    .map((img: any) => (
                      <Box key={img.id} sx={{ width: 72, height: 72, borderRadius: 1, overflow: 'hidden', border: '1px solid', borderColor: 'divider' }}>
                        <img src={img.preSignedUrl} alt={img.fileName} style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
                      </Box>
                    ))}
                </Box>
              )}

              {/* Demographic Information */}
              <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                <Controller
                  name="age"
                  control={control}
                  rules={{
                    min: { value: 1, message: 'Age must be positive' },
                    max: { value: 150, message: 'Age must be reasonable' }
                  }}
                  render={({ field, fieldState }) => (
                    <TextField
                      {...field}
                      label="Age"
                      type="number"
                      sx={{ minWidth: '120px' }}
                      inputProps={{ min: 1, max: 150 }}
                      value={field.value || ''}
                      onChange={(e) => field.onChange(e.target.value === '' ? undefined : parseInt(e.target.value) || undefined)}
                      error={!!fieldState.error}
                      helperText={fieldState.error?.message}
                    />
                  )}
                />
                <Controller
                  name="gender"
                  control={control}
                  render={({ field }) => (
                    <FormControl sx={{ minWidth: '150px' }}>
                      <InputLabel>Gender</InputLabel>
                      <Select {...field} label="Gender">
                        <MenuItem value="">
                          <em>Select gender</em>
                        </MenuItem>
                        <MenuItem value="Male">Male</MenuItem>
                        <MenuItem value="Female">Female</MenuItem>
                        <MenuItem value="NonBinary">Non-binary</MenuItem>
                        <MenuItem value="Other">Other</MenuItem>
                        <MenuItem value="PreferNotToSay">Prefer not to say</MenuItem>
                      </Select>
                    </FormControl>
                  )}
                />
                <Controller
                  name="nationality"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Nationality"
                      sx={{ flex: 1, minWidth: '150px' }}
                    />
                  )}
                />
              </Box>

              <Controller
                name="biography"
                control={control}
                rules={{ maxLength: { value: 2000, message: 'Biography must not exceed 2000 characters' } }}
                render={({ field, fieldState }) => (
                  <TextField
                    {...field}
                    label="Biography"
                    fullWidth
                    multiline
                    rows={4}
                    error={!!fieldState.error}
                    helperText={fieldState.error?.message}
                  />
                )}
              />
            </Box>
          </Box>

          {/* Personality Traits */}
          <Box>
            <Typography variant="subtitle1" gutterBottom sx={{ fontWeight: 'bold' }}>
              Personality Traits (Big Five)
            </Typography>
            <Divider sx={{ mb: 2 }} />
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Rate each trait on a scale from 0-100 (0 = low, 100 = high)
            </Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                <Controller
                  name="openness"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Openness"
                      type="number"
                      sx={{ minWidth: '150px' }}
                      inputProps={{ min: 0, max: 100 }}
                      value={field.value || ''}
                      onChange={(e) => field.onChange(e.target.value === '' ? 0 : parseInt(e.target.value) || 0)}
                    />
                  )}
                />
                <Controller
                  name="conscientiousness"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Conscientiousness"
                      type="number"
                      sx={{ minWidth: '150px' }}
                      inputProps={{ min: 0, max: 100 }}
                      value={field.value || ''}
                      onChange={(e) => field.onChange(e.target.value === '' ? 0 : parseInt(e.target.value) || 0)}
                    />
                  )}
                />
              </Box>

              <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                <Controller
                  name="extraversion"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Extraversion"
                      type="number"
                      sx={{ minWidth: '150px' }}
                      inputProps={{ min: 0, max: 100 }}
                      value={field.value || ''}
                      onChange={(e) => field.onChange(e.target.value === '' ? 0 : parseInt(e.target.value) || 0)}
                    />
                  )}
                />
                <Controller
                  name="agreeableness"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Agreeableness"
                      type="number"
                      sx={{ minWidth: '150px' }}
                      inputProps={{ min: 0, max: 100 }}
                      value={field.value || ''}
                      onChange={(e) => field.onChange(e.target.value === '' ? 0 : parseInt(e.target.value) || 0)}
                    />
                  )}
                />
              </Box>

              <Controller
                name="neuroticism"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label="Neuroticism"
                    type="number"
                    sx={{ minWidth: '150px' }}
                    inputProps={{ min: 0, max: 100 }}
                    value={field.value || ''}
                    onChange={(e) => field.onChange(e.target.value === '' ? 0 : parseInt(e.target.value) || 0)}
                  />
                )}
              />
            </Box>
          </Box>

          {/* Activity Associations */}
          <Box>
            <Typography variant="subtitle1" gutterBottom sx={{ fontWeight: 'bold' }}>
              Activity Preferences
            </Typography>
            <Divider sx={{ mb: 2 }} />
            <Controller
              name="activityAssociations"
              control={control}
              render={({ field }) => (
                <Autocomplete
                  multiple
                  options={activities}
                  getOptionLabel={(option) => option.name}
                  value={activities.filter(activity =>
                    field.value?.some((assoc: PersonaActivityAssociation) => assoc.activityId === activity.id)
                  )}
                  onChange={(event, newValue) => {
                    const newAssociations = newValue.map(activity => {
                      const existing = field.value?.find((assoc: PersonaActivityAssociation) => assoc.activityId === activity.id);
                      return existing || { activityId: activity.id };
                    });
                    field.onChange(newAssociations);
                  }}
                  loading={activitiesLoading}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Select Activities"
                      placeholder="Choose activities for this persona"
                      helperText="Select activities and set preference levels (1-5)"
                    />
                  )}
                  renderTags={(tagValue, getTagProps) =>
                    tagValue.map((activity, index) => {
                      const association = field.value?.find((assoc: PersonaActivityAssociation) => assoc.activityId === activity.id);
                      const tagProps = getTagProps({ index });
                      return (
                        <Chip
                          {...tagProps}
                          key={activity.id}
                          label={activity.name}
                          onDelete={() => {
                            const newAssociations = field.value?.filter((assoc: PersonaActivityAssociation) => assoc.activityId !== activity.id) || [];
                            field.onChange(newAssociations);
                          }}
                        />
                      );
                    })
                  }
                />
              )}
            />
            {watch('activityAssociations')?.length > 0 && (
              <Box sx={{ mt: 2 }}>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                  Selected activities:
                </Typography>
                {watch('activityAssociations')?.map((association: PersonaActivityAssociation, index: number) => {
                  const activity = activities.find(a => a.id === association.activityId);
                  return (
                    <Box key={association.activityId} sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1 }}>
                      <Typography variant="body2" sx={{ minWidth: '150px' }}>
                        {activity?.name}
                      </Typography>
                    </Box>
                  );
                })}
              </Box>
            )}
          </Box>

          {/* Characteristics */}
          <Box>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 'bold' }}>
                Characteristics
              </Typography>
              <Button
                variant="outlined"
                size="small"
                startIcon={<AddIcon />}
                onClick={addCharacteristic}
              >
                Add Characteristic
              </Button>
            </Box>
            <Divider sx={{ mb: 2 }} />

            {fields.map((field, index) => (
              <Paper key={field.id} variant="outlined" sx={{ p: 2, mb: 2 }}>
                <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center' }}>
                  <Controller
                    name={`characteristics.${index}.name`}
                    control={control}
                    rules={{ required: 'Name is required' }}
                    render={({ field: nameField }) => (
                      <TextField
                        {...nameField}
                        label="Trait Name"
                        sx={{ flex: 1, minWidth: '150px' }}
                        size="small"
                        error={!!errors.characteristics?.[index]?.name}
                        helperText={errors.characteristics?.[index]?.name?.message}
                      />
                    )}
                  />
                  <Controller
                    name={`characteristics.${index}.value`}
                    control={control}
                    rules={{ required: 'Value is required' }}
                    render={({ field: valueField }) => (
                      <TextField
                        {...valueField}
                        label="Value"
                        sx={{ flex: 1, minWidth: '150px' }}
                        size="small"
                        error={!!errors.characteristics?.[index]?.value}
                        helperText={errors.characteristics?.[index]?.value?.message}
                      />
                    )}
                  />
                  <Controller
                    name={`characteristics.${index}.type`}
                    control={control}
                    render={({ field: typeField }) => (
                      <FormControl sx={{ minWidth: '120px' }} size="small">
                        <InputLabel>Type</InputLabel>
                        <Select {...typeField} label="Type">
                          {CHARACTERISTIC_TYPES.map((type) => (
                            <MenuItem key={type} value={type}>
                              {type}
                            </MenuItem>
                          ))}
                        </Select>
                      </FormControl>
                    )}
                  />
                  <IconButton
                    color="error"
                    onClick={() => removeCharacteristic(index)}
                    size="small"
                  >
                    <DeleteIcon />
                  </IconButton>
                </Box>
              </Paper>
            ))}

            {fields.length === 0 && (
              <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 2 }}>
                No characteristics added yet. Click "Add Characteristic" to get started.
              </Typography>
            )}
          </Box>
        </Box>
      </Paper>

      <Box sx={{ mt: 3, display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
        <Button
          variant="outlined"
          onClick={onCancel}
          disabled={loading}
        >
          Cancel
        </Button>
        <Button
          type="submit"
          variant="contained"
          disabled={loading}
          startIcon={loading ? <CircularProgress size={16} /> : null}
        >
          {loading ? 'Saving...' : (persona ? 'Update Persona' : 'Create Persona')}
        </Button>
      </Box>
    </Box>
  );
};

export default PersonaForm;
