import React, { useCallback, useEffect, useState, FormEvent, useRef } from 'react';
import {
  Alert,
  AlertColor,
  Box,
  Button,
  CircularProgress,
  Divider,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  SelectChangeEvent,
  Snackbar,
  Stack,
  TextField,
  Typography,
  Checkbox,
  ListItemText,
} from '@mui/material';
import { aiSettingsAPI } from '../services/api';
import { LlmSettings, UpdateLlmSettingsPayload } from '../services/api.types';
import { useTranslation } from '../hooks/useTranslation';

interface AiSettingsForm {
  defaultProvider: string;
  defaultModel: string;
  ollamaBaseUrl: string;
  ollamaHealthCheckModel: string;
  ollamaPreferredModels: string[];
  openAiBaseUrl: string;
  openAiDefaultModel: string;
  openAiAllowedModelsInput: string;
  openAiOrganizationId: string;
  openAiApiKey: string;
  hasOpenAiApiKey: boolean;
  modelCacheSeconds: number;
}

const normalizeList = (value: string) =>
  value
    .split(',')
    .map((item) => item.trim())
    .filter((item) => item.length > 0);

const mapSettingsToForm = (settings: LlmSettings): AiSettingsForm => ({
  defaultProvider: settings.defaultProvider,
  defaultModel: settings.defaultModel,
  ollamaBaseUrl: settings.ollamaBaseUrl,
  ollamaHealthCheckModel: settings.ollamaHealthCheckModel,
  ollamaPreferredModels: settings.ollamaPreferredModels,
  openAiBaseUrl: settings.openAiBaseUrl,
  openAiDefaultModel: settings.openAiDefaultModel,
  openAiAllowedModelsInput: settings.openAiAllowedModels.join(', '),
  openAiOrganizationId: settings.openAiOrganizationId,
  openAiApiKey: '',
  hasOpenAiApiKey: settings.hasOpenAiApiKey,
  modelCacheSeconds: settings.modelCacheSeconds,
});

const AdminAiSettings: React.FC = () => {
  const { t } = useTranslation();
  const [form, setForm] = useState<AiSettingsForm | null>(null);
  const formRef = useRef<AiSettingsForm | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [apiKeyEdited, setApiKeyEdited] = useState(false);
  const [ollamaModels, setOllamaModels] = useState<string[]>([]);
  const [ollamaModelsLoading, setOllamaModelsLoading] = useState(false);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: AlertColor }>({
    open: false,
    message: '',
    severity: 'success',
  });
  const loadOllamaModels = useCallback(
    async (forceRefresh = false) => {
      setOllamaModelsLoading(true);
      try {
        const response = await aiSettingsAPI.getProviderModels('Ollama', forceRefresh);
        const payload = (response.data.data ?? response.data) as { models: { name: string }[] };
        const fetched = payload.models?.map((model) => model.name).filter(Boolean) ?? [];
        const snapshot = formRef.current;
        const extras = snapshot ? [snapshot.defaultModel, ...snapshot.ollamaPreferredModels] : [];
        const merged = Array.from(new Set([...fetched, ...extras.filter(Boolean)]));
        setOllamaModels(merged);
      } catch (error) {
        setSnackbar({
          open: true,
          message: t('ai_settings_models_error'),
          severity: 'error',
        });
      } finally {
        setOllamaModelsLoading(false);
      }
    },
    [t]
  );

  const fetchSettings = useCallback(async () => {
    setLoading(true);
    try {
      const response = await aiSettingsAPI.getSettings();
      const payload = (response.data.data ?? response.data) as LlmSettings;
      setForm(mapSettingsToForm(payload));
      setApiKeyEdited(false);
    } catch (error) {
      setSnackbar({
        open: true,
        message: t('ai_settings_error'),
        severity: 'error',
      });
    } finally {
      setLoading(false);
    }
  }, [t]);

  useEffect(() => {
    fetchSettings();
  }, [fetchSettings]);

  useEffect(() => {
    formRef.current = form;
  }, [form]);

  useEffect(() => {
    loadOllamaModels(false);
  }, [loadOllamaModels]);

  const handleFieldChange = (field: keyof AiSettingsForm, value: string | number | string[]) => {
    setForm((prev) => (prev ? { ...prev, [field]: value } : prev));
  };

  const handleProviderChange = (event: SelectChangeEvent<string>) => {
    handleFieldChange('defaultProvider', event.target.value);
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (!form) {
      return;
    }

    setSaving(true);
    try {
      const payload: UpdateLlmSettingsPayload = {
        defaultProvider: form.defaultProvider,
        defaultModel: form.defaultModel,
        ollamaBaseUrl: form.ollamaBaseUrl,
        ollamaHealthCheckModel: form.ollamaHealthCheckModel,
        ollamaPreferredModels: form.ollamaPreferredModels.length
          ? form.ollamaPreferredModels
          : [],
        openAiBaseUrl: form.openAiBaseUrl,
        openAiDefaultModel: form.openAiDefaultModel,
        openAiAllowedModels: normalizeList(form.openAiAllowedModelsInput),
        openAiOrganizationId: form.openAiOrganizationId,
        modelCacheSeconds: form.modelCacheSeconds,
      };

      if (apiKeyEdited) {
        payload.openAiApiKey = form.openAiApiKey ?? '';
      }

      const response = await aiSettingsAPI.updateSettings(payload);
      const updated = (response.data.data ?? response.data) as LlmSettings;
      setForm(mapSettingsToForm(updated));
      setApiKeyEdited(false);
      setSnackbar({ open: true, message: t('ai_settings_success'), severity: 'success' });
    } catch (error) {
      setSnackbar({ open: true, message: t('ai_settings_error'), severity: 'error' });
    } finally {
      setSaving(false);
    }
  };

  const handleReset = () => {
    fetchSettings();
  };

  const twoColumnLayout = {
    display: 'grid',
    gap: 2,
    gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' },
  };

  if (loading || !form) {
    return (
      <div className="page-container">
        <Box display="flex" alignItems="center" gap={2}>
          <CircularProgress size={24} />
          <Typography>{t('ai_settings_loading')}</Typography>
        </Box>
      </div>
    );
  }

  return (
    <div className="page-container">
      <Typography variant="h4" gutterBottom>
        {t('ai_settings_title')}
      </Typography>
      <Typography variant="body1" color="text.secondary" paragraph>
        {t('ai_settings_description')}
      </Typography>

      <Paper elevation={1} sx={{ p: 3 }}>
        <Box component="form" onSubmit={handleSubmit}>
          <Box sx={twoColumnLayout}>
            <Box>
              <FormControl fullWidth>
                <InputLabel>{t('ai_settings_default_provider')}</InputLabel>
                <Select
                  label={t('ai_settings_default_provider')}
                  value={form.defaultProvider}
                  onChange={handleProviderChange}
                >
                  <MenuItem value="Ollama">{t('ai_settings_provider_ollama')}</MenuItem>
                  <MenuItem value="OpenAI">{t('ai_settings_provider_openai')}</MenuItem>
                </Select>
              </FormControl>
            </Box>
            <Box>
              {form.defaultProvider === 'Ollama' ? (
                <FormControl fullWidth>
                  <InputLabel>{t('ai_settings_default_model')}</InputLabel>
                  <Select
                    label={t('ai_settings_default_model')}
                    value={form.defaultModel}
                    onChange={(e) => handleFieldChange('defaultModel', e.target.value)}
                  >
                    {(ollamaModels.length > 0 ? ollamaModels : [form.defaultModel || ''])
                      .filter(Boolean)
                      .map((model) => (
                        <MenuItem key={model} value={model}>
                          {model}
                        </MenuItem>
                      ))}
                  </Select>
                </FormControl>
              ) : (
                <TextField
                  fullWidth
                  label={t('ai_settings_default_model')}
                  value={form.defaultModel}
                  onChange={(e) => handleFieldChange('defaultModel', e.target.value)}
                />
              )}
            </Box>
          </Box>

          <Divider sx={{ my: 4 }} />

          <Typography variant="h6" gutterBottom>
            {t('ai_settings_ollama_section')}
          </Typography>
          <Box sx={twoColumnLayout}>
            <Box>
              <TextField
                fullWidth
                label={t('ai_settings_ollama_base_url')}
                value={form.ollamaBaseUrl}
                onChange={(e) => handleFieldChange('ollamaBaseUrl', e.target.value)}
              />
            </Box>
            <Box>
              <TextField
                fullWidth
                label={t('ai_settings_ollama_health_model')}
                value={form.ollamaHealthCheckModel}
                onChange={(e) => handleFieldChange('ollamaHealthCheckModel', e.target.value)}
              />
            </Box>
            <Box sx={{ gridColumn: { xs: 'span 1', md: 'span 2' } }}>
              <FormControl fullWidth>
                <InputLabel>{t('ai_settings_ollama_models')}</InputLabel>
                <Select
                  label={t('ai_settings_ollama_models')}
                  multiple
                  value={form.ollamaPreferredModels}
                  onChange={(event: SelectChangeEvent<string[]>) => {
                    const rawValue = event.target.value;
                    const value = typeof rawValue === 'string' ? rawValue.split(',') : rawValue;
                    handleFieldChange(
                      'ollamaPreferredModels',
                      value.map((item) => item.trim()).filter(Boolean)
                    );
                  }}
                  renderValue={(selected) => (selected as string[]).join(', ')}
                >
                  {ollamaModels.map((model) => (
                    <MenuItem key={model} value={model}>
                      <Checkbox checked={form.ollamaPreferredModels.indexOf(model) > -1} />
                      <ListItemText primary={model} />
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              <Stack direction="row" spacing={1} alignItems="center" sx={{ mt: 1 }}>
                <Button size="small" variant="outlined" onClick={() => loadOllamaModels(true)} disabled={ollamaModelsLoading}>
                  {ollamaModelsLoading ? t('ai_settings_ollama_models_loading') : t('ai_settings_ollama_models_refresh')}
                </Button>
                {ollamaModels.length === 0 && !ollamaModelsLoading && (
                  <Typography variant="body2" color="text.secondary">
                    {t('ai_settings_ollama_models_empty')}
                  </Typography>
                )}
              </Stack>
              <Typography variant="caption" color="text.secondary">
                {t('ai_settings_ollama_models_helper')}
              </Typography>
            </Box>
          </Box>

          <Divider sx={{ my: 4 }} />

          <Typography variant="h6" gutterBottom>
            {t('ai_settings_openai_section')}
          </Typography>
          <Box sx={twoColumnLayout}>
            <Box>
              <TextField
                fullWidth
                label={t('ai_settings_openai_base_url')}
                value={form.openAiBaseUrl}
                onChange={(e) => handleFieldChange('openAiBaseUrl', e.target.value)}
              />
            </Box>
            <Box>
              <TextField
                fullWidth
                label={t('ai_settings_openai_default_model')}
                value={form.openAiDefaultModel}
                onChange={(e) => handleFieldChange('openAiDefaultModel', e.target.value)}
              />
            </Box>
            <Box sx={{ gridColumn: { xs: 'span 1', md: 'span 2' } }}>
              <TextField
                fullWidth
                label={t('ai_settings_openai_allowed_models')}
                value={form.openAiAllowedModelsInput}
                onChange={(e) => handleFieldChange('openAiAllowedModelsInput', e.target.value)}
                helperText={t('ai_settings_openai_allowed_models_helper')}
              />
            </Box>
            <Box>
              <TextField
                fullWidth
                label={t('ai_settings_openai_org_id')}
                value={form.openAiOrganizationId}
                onChange={(e) => handleFieldChange('openAiOrganizationId', e.target.value)}
              />
            </Box>
            <Box>
              <TextField
                fullWidth
                type="password"
                label={t('ai_settings_openai_api_key')}
                value={form.openAiApiKey}
                onChange={(e) => {
                  setApiKeyEdited(true);
                  handleFieldChange('openAiApiKey', e.target.value);
                }}
                helperText={t('ai_settings_openai_key_helper')}
              />
              {form.hasOpenAiApiKey && !apiKeyEdited && (
                <Alert sx={{ mt: 1 }} severity="info">
                  {t('ai_settings_openai_key_configured')}
                </Alert>
              )}
              <Button
                sx={{ mt: 1 }}
                variant="outlined"
                onClick={() => {
                  setApiKeyEdited(true);
                  handleFieldChange('openAiApiKey', '');
                }}
              >
                {t('ai_settings_openai_key_clear')}
              </Button>
            </Box>
          </Box>

          <Divider sx={{ my: 4 }} />

          <Box sx={twoColumnLayout}>
            <Box>
              <TextField
                type="number"
                fullWidth
                label={t('ai_settings_cache_seconds')}
                value={form.modelCacheSeconds}
                onChange={(e) => handleFieldChange('modelCacheSeconds', Number(e.target.value))}
                inputProps={{ min: 30 }}
              />
            </Box>
          </Box>

          <Stack direction="row" spacing={2} sx={{ mt: 4 }}>
            <Button type="submit" variant="contained" color="primary" disabled={saving}>
              {saving ? t('ai_settings_saving') : t('ai_settings_submit')}
            </Button>
            <Button variant="text" color="secondary" onClick={handleReset} disabled={saving}>
              {t('ai_settings_reset')}
            </Button>
          </Stack>
        </Box>
      </Paper>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}
      >
        <Alert severity={snackbar.severity} onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </div>
  );
};

export default AdminAiSettings;
