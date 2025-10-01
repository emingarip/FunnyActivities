import React, { useState } from 'react';
import {
  Box,
  TextField,
  Button,
  IconButton,
  Typography,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Grid,
  Paper,
  Chip
} from '@mui/material';
import { Add as AddIcon, Delete as DeleteIcon, Edit as EditIcon } from '@mui/icons-material';

interface DynamicPropertiesInputProps {
  properties: Record<string, any>;
  onChange: (properties: Record<string, any>) => void;
  disabled?: boolean;
  error?: string;
}

const DynamicPropertiesInput: React.FC<DynamicPropertiesInputProps> = ({
  properties,
  onChange,
  disabled = false,
  error
}) => {
  const [newKey, setNewKey] = useState('');
  const [newValue, setNewValue] = useState('');
  const [newType, setNewType] = useState<'string' | 'number' | 'boolean'>('string');
  const [editingKey, setEditingKey] = useState<string | null>(null);
  const [editValue, setEditValue] = useState('');
  const [editType, setEditType] = useState<'string' | 'number' | 'boolean'>('string');

  const handleAddProperty = () => {
    if (!newKey.trim()) return;

    // Validate key format
    if (!/^[a-zA-Z_][a-zA-Z0-9_]*$/.test(newKey.trim())) {
      return; // Invalid key format
    }

    // Validate key length
    if (newKey.trim().length > 50) {
      return; // Key too long
    }

    // Validate value length for strings
    if (newType === 'string' && newValue.length > 500) {
      return; // Value too long
    }

    // Check property limit
    if (Object.keys(properties).length >= 20) {
      return; // Too many properties
    }

    const updatedProperties = { ...properties };

    // Convert value based on type
    let value: any = newValue;
    if (newType === 'number') {
      value = parseFloat(newValue) || 0;
    } else if (newType === 'boolean') {
      value = newValue.toLowerCase() === 'true';
    }

    updatedProperties[newKey.trim()] = value;
    onChange(updatedProperties);

    // Reset form
    setNewKey('');
    setNewValue('');
    setNewType('string');
  };

  const handleRemoveProperty = (key: string) => {
    const updatedProperties = { ...properties };
    delete updatedProperties[key];
    onChange(updatedProperties);
  };

  const handleStartEdit = (key: string, value: any) => {
    setEditingKey(key);
    setEditValue(String(value));
    setEditType(typeof value === 'boolean' ? 'boolean' : typeof value === 'number' ? 'number' : 'string');
  };

  const handleSaveEdit = () => {
    if (!editingKey) return;

    const updatedProperties = { ...properties };

    // Convert value based on type
    let value: any = editValue;
    if (editType === 'number') {
      value = parseFloat(editValue) || 0;
    } else if (editType === 'boolean') {
      value = editValue.toLowerCase() === 'true';
    }

    updatedProperties[editingKey] = value;
    onChange(updatedProperties);

    // Reset edit state
    setEditingKey(null);
    setEditValue('');
    setEditType('string');
  };

  const handleCancelEdit = () => {
    setEditingKey(null);
    setEditValue('');
    setEditType('string');
  };

  const renderValueInput = (
    value: string,
    onChange: (value: string) => void,
    type: 'string' | 'number' | 'boolean',
    onTypeChange: (type: 'string' | 'number' | 'boolean') => void,
    placeholder: string,
    disabled: boolean = false
  ) => {
    return (
      <Grid container spacing={1} alignItems="center">
        <Grid size={{ xs: 8 }}>
          <TextField
            fullWidth
            size="small"
            value={value}
            onChange={(e) => onChange(e.target.value)}
            placeholder={placeholder}
            type={type === 'number' ? 'number' : 'text'}
            disabled={disabled}
          />
        </Grid>
        <Grid size={{ xs: 4 }}>
          <FormControl fullWidth size="small">
            <InputLabel>Type</InputLabel>
            <Select
              value={type}
              label="Type"
              onChange={(e) => onTypeChange(e.target.value as 'string' | 'number' | 'boolean')}
              disabled={disabled}
            >
              <MenuItem value="string">String</MenuItem>
              <MenuItem value="number">Number</MenuItem>
              <MenuItem value="boolean">Boolean</MenuItem>
            </Select>
          </FormControl>
        </Grid>
      </Grid>
    );
  };

  const getValueDisplay = (value: any): string => {
    if (typeof value === 'boolean') {
      return value ? 'true' : 'false';
    }
    return String(value);
  };

  const getTypeColor = (value: any): 'default' | 'primary' | 'secondary' => {
    if (typeof value === 'boolean') return 'secondary';
    if (typeof value === 'number') return 'primary';
    return 'default';
  };

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Dynamic Properties
      </Typography>

      {error && (
        <Typography color="error" variant="body2" sx={{ mb: 2 }}>
          {error}
        </Typography>
      )}

      {/* Existing Properties */}
      {Object.keys(properties).length > 0 && (
        <Box sx={{ mb: 3 }}>
          <Typography variant="subtitle2" gutterBottom>
            Current Properties:
          </Typography>
          <Grid container spacing={2}>
            {Object.entries(properties).map(([key, value]) => (
              <Grid size={{ xs: 12, sm: 6, md: 4 }} key={key}>
                <Paper variant="outlined" sx={{ p: 2 }}>
                  {editingKey === key ? (
                    <Box>
                      <Typography variant="body2" sx={{ mb: 1, fontWeight: 'bold' }}>
                        {key}
                      </Typography>
                      {renderValueInput(
                        editValue,
                        setEditValue,
                        editType,
                        setEditType,
                        'Enter value',
                        disabled
                      )}
                      <Box sx={{ mt: 1, display: 'flex', gap: 1 }}>
                        <Button
                          size="small"
                          variant="contained"
                          onClick={handleSaveEdit}
                          disabled={disabled}
                        >
                          Save
                        </Button>
                        <Button
                          size="small"
                          variant="outlined"
                          onClick={handleCancelEdit}
                          disabled={disabled}
                        >
                          Cancel
                        </Button>
                      </Box>
                    </Box>
                  ) : (
                    <Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                        <Typography variant="body2" sx={{ fontWeight: 'bold' }}>
                          {key}
                        </Typography>
                        <Box>
                          <IconButton
                            size="small"
                            onClick={() => handleStartEdit(key, value)}
                            disabled={disabled}
                          >
                            <EditIcon fontSize="small" />
                          </IconButton>
                          <IconButton
                            size="small"
                            onClick={() => handleRemoveProperty(key)}
                            disabled={disabled}
                            color="error"
                          >
                            <DeleteIcon fontSize="small" />
                          </IconButton>
                        </Box>
                      </Box>
                      <Chip
                        label={getValueDisplay(value)}
                        size="small"
                        color={getTypeColor(value)}
                        variant="outlined"
                      />
                    </Box>
                  )}
                </Paper>
              </Grid>
            ))}
          </Grid>
        </Box>
      )}

      {/* Add New Property */}
      <Box sx={{ p: 2, border: '1px dashed', borderColor: 'grey.300', borderRadius: 1 }}>
        <Typography variant="subtitle2" gutterBottom>
          Add New Property:
        </Typography>
        <Grid container spacing={2} alignItems="center">
          <Grid size={{ xs: 12, sm: 3 }}>
            <TextField
              fullWidth
              size="small"
              label="Property Key"
              value={newKey}
              onChange={(e) => setNewKey(e.target.value)}
              disabled={disabled}
              placeholder="Enter key name"
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            {renderValueInput(
              newValue,
              setNewValue,
              newType,
              setNewType,
              'Enter value',
              disabled
            )}
          </Grid>
          <Grid size={{ xs: 12, sm: 3 }}>
            <Button
              fullWidth
              variant="contained"
              startIcon={<AddIcon />}
              onClick={handleAddProperty}
              disabled={disabled || !newKey.trim()}
            >
              Add Property
            </Button>
          </Grid>
        </Grid>
      </Box>
    </Box>
  );
};

export default DynamicPropertiesInput;