import React, { useEffect, useMemo, useState } from 'react';
import {
  Box,
  Button,
  Chip,
  Container,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  FormControlLabel,
  InputLabel,
  MenuItem,
  Select,
  Switch,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
  Paper,
  CircularProgress,
  Alert,
} from '@mui/material';
import { promptAPI, aiSettingsAPI } from '../services/api';
import { PromptTemplateDto, PromptCallLog, PromptTemplateTestResult, LlmSettings } from '../services/api.types';
import { useTranslation } from '../hooks/useTranslation';

type FormMode = 'create' | 'edit';

const defaultForm = (): PromptTemplateDto => ({
  id: '',
  key: '',
  title: '',
  locale: 'en-US',
  providerHint: '',
  content: '',
  outputFormatHint: '',
  description: '',
  isActive: true,
  updatedAt: new Date().toISOString(),
});

const locales = ['en-US', 'tr-TR', 'de-DE', 'fr-FR'];
const providerHints = ['', 'Ollama', 'OpenAI'];

const PromptTemplates: React.FC = () => {
  const { t } = useTranslation();
  const [templates, setTemplates] = useState<PromptTemplateDto[]>([]);
  const [logs, setLogs] = useState<PromptCallLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [formMode, setFormMode] = useState<FormMode>('create');
  const [form, setForm] = useState<PromptTemplateDto>(defaultForm());
  const [testResult, setTestResult] = useState<PromptTemplateTestResult | null>(null);
  const [testLoading, setTestLoading] = useState(false);
  const [testDialogOpen, setTestDialogOpen] = useState(false);
  const [testTemplate, setTestTemplate] = useState<PromptTemplateDto | null>(null);
  const [testProvider, setTestProvider] = useState<string>('Ollama');
  const [testModel, setTestModel] = useState<string>('');

  const sortedTemplates = useMemo(
    () => [...templates].sort((a, b) => a.key.localeCompare(b.key)),
    [templates]
  );

  useEffect(() => {
    loadData();
    loadDefaults();
  }, []);

  const loadData = async () => {
    setLoading(true);
    setError(null);
    try {
      const [templateRes, logRes] = await Promise.all([promptAPI.list({ includeInactive: true }), promptAPI.logs(25)]);
      const templatePayload = (templateRes.data.data ?? templateRes.data) as PromptTemplateDto[];
      const logPayload = (logRes.data.data ?? logRes.data) as PromptCallLog[];
      setTemplates(templatePayload || []);
      setLogs(logPayload || []);
    } catch (err: any) {
      setError(err?.message || 'Failed to load prompt templates');
    } finally {
      setLoading(false);
    }
  };

  const loadDefaults = async () => {
    try {
      const res = await aiSettingsAPI.getSettings();
      const settings = (res.data.data ?? res.data) as LlmSettings;
      setTestProvider(settings.defaultProvider);
      setTestModel(settings.defaultModel);
    } catch {
      // ignore defaults load errors for now
    }
  };

  const handleOpenCreate = () => {
    setForm(defaultForm());
    setFormMode('create');
    setFormOpen(true);
  };

  const handleEdit = (template: PromptTemplateDto) => {
    setForm({ ...template });
    setFormMode('edit');
    setFormOpen(true);
  };

  const handleChange = (field: keyof PromptTemplateDto, value: any) => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const handleSave = async () => {
    if (!form.key.trim() || !form.title.trim() || !form.content.trim()) {
      setError(t('prompt_templates_validation'));
      return;
    }

    setSaving(true);
    setError(null);
    try {
      if (formMode === 'create') {
        const payload = {
          key: form.key.trim(),
          title: form.title.trim(),
          locale: form.locale,
          providerHint: form.providerHint || undefined,
          content: form.content,
          outputFormatHint: form.outputFormatHint || undefined,
          description: form.description || undefined,
          isActive: form.isActive,
        };
        const response = await promptAPI.create(payload);
        const created = (response.data.data ?? response.data) as PromptTemplateDto;
        setTemplates((prev) => [...prev, created]);
      } else {
        const payload = {
          key: form.key.trim(),
          title: form.title.trim(),
          locale: form.locale,
          providerHint: form.providerHint || undefined,
          content: form.content,
          outputFormatHint: form.outputFormatHint || undefined,
          description: form.description || undefined,
          isActive: form.isActive,
        };
        const response = await promptAPI.update(form.id, payload);
        const updated = (response.data.data ?? response.data) as PromptTemplateDto;
        setTemplates((prev) => prev.map((t) => (t.id === updated.id ? updated : t)));
      }
      setFormOpen(false);
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Save failed');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm(t('prompt_templates_delete_confirm'))) return;
    try {
      await promptAPI.remove(id);
      setTemplates((prev) => prev.filter((t) => t.id !== id));
    } catch (err: any) {
      setError(err?.message || 'Delete failed');
    }
  };

  const handleClone = async (template: PromptTemplateDto) => {
    try {
      const response = await promptAPI.clone(template.id, {
        key: `${template.key}-copy`,
        title: `${template.title} Copy`,
        locale: template.locale,
        isActive: template.isActive,
      });
      const clone = (response.data.data ?? response.data) as PromptTemplateDto;
      setTemplates((prev) => [...prev, clone]);
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Clone failed');
    }
  };

  const openTestDialog = (template: PromptTemplateDto) => {
    setTestTemplate(template);
    setTestResult(null);
    setTestProvider(template.providerHint || testProvider || 'Ollama');
    setTestDialogOpen(true);
  };

  const runTest = async () => {
    if (!testTemplate) return;
    setTestLoading(true);
    setTestResult(null);
    setError(null);
    try {
      const response = await promptAPI.test(testTemplate.key, {
        provider: testProvider,
        model: testModel || undefined,
      });
      const payload = (response.data.data ?? response.data) as PromptTemplateTestResult;
      setTestResult(payload);
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Test failed');
    } finally {
      setTestLoading(false);
    }
  };

  const renderLogStatus = (success: boolean) => (
    <Chip size="small" label={success ? t('prompt_templates_success') : t('prompt_templates_error')} color={success ? 'success' : 'error'} />
  );

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 3 }}>
        <Box>
          <Typography variant="h4" gutterBottom>{t('prompt_templates_title')}</Typography>
          <Typography variant="body1" color="text.secondary">{t('prompt_templates_subtitle')}</Typography>
        </Box>
        <Button variant="contained" onClick={handleOpenCreate}>{t('prompt_templates_new')}</Button>
      </Stack>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
          <CircularProgress />
        </Box>
      ) : (
        <Paper elevation={1} sx={{ mb: 3 }}>
          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>{t('prompt_templates_key')}</TableCell>
                  <TableCell>{t('prompt_templates_title_col')}</TableCell>
                  <TableCell>{t('prompt_templates_locale')}</TableCell>
                  <TableCell>{t('prompt_templates_provider')}</TableCell>
                  <TableCell>{t('prompt_templates_updated')}</TableCell>
                  <TableCell align="right">{t('prompt_templates_actions')}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {sortedTemplates.map((template) => (
                  <TableRow key={template.id}>
                    <TableCell>{template.key}</TableCell>
                    <TableCell>
                      <Stack direction="row" spacing={1} alignItems="center">
                        <Typography variant="body2">{template.title}</Typography>
                        {!template.isActive && <Chip size="small" label={t('prompt_templates_inactive')} />}
                      </Stack>
                    </TableCell>
                    <TableCell>{template.locale}</TableCell>
                    <TableCell>{template.providerHint || '—'}</TableCell>
                    <TableCell>{new Date(template.updatedAt).toLocaleString()}</TableCell>
                    <TableCell align="right">
                      <Stack direction="row" spacing={1} justifyContent="flex-end">
                        <Button size="small" variant="text" onClick={() => handleEdit(template)}>{t('prompt_templates_edit')}</Button>
                        <Button size="small" variant="text" onClick={() => handleClone(template)}>{t('prompt_templates_clone')}</Button>
                        <Button size="small" variant="outlined" onClick={() => openTestDialog(template)} disabled={testLoading}>
                          {testLoading && testTemplate?.id === template.id ? t('prompt_templates_testing') : t('prompt_templates_test')}
                        </Button>
                        <Button size="small" color="error" onClick={() => handleDelete(template.id)}>{t('prompt_templates_delete')}</Button>
                      </Stack>
                    </TableCell>
                  </TableRow>
                ))}
                {sortedTemplates.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                      <Typography variant="body2" color="text.secondary">{t('prompt_templates_empty')}</Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </Paper>
      )}

      <Paper elevation={0} sx={{ p: 2, border: '1px solid', borderColor: 'divider' }}>
        <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 1 }}>
          <Typography variant="h6">{t('prompt_templates_logs')}</Typography>
          <Button size="small" onClick={loadData}>{t('prompt_templates_refresh')}</Button>
        </Stack>
        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>{t('prompt_templates_key')}</TableCell>
                <TableCell>{t('prompt_templates_model')}</TableCell>
                <TableCell>{t('prompt_templates_duration')}</TableCell>
                <TableCell>{t('prompt_templates_status')}</TableCell>
                <TableCell>{t('prompt_templates_created')}</TableCell>
                <TableCell>{t('prompt_templates_summary')}</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {logs.map((log) => (
                <TableRow key={log.id}>
                  <TableCell>{log.templateKey}</TableCell>
                  <TableCell>{`${log.provider}/${log.model || 'default'}`}</TableCell>
                  <TableCell>{`${log.duration.toFixed(0)} ms`}</TableCell>
                  <TableCell>{renderLogStatus(log.success)}</TableCell>
                  <TableCell>{new Date(log.createdAt).toLocaleString()}</TableCell>
                  <TableCell>{log.resultSummary || log.errorMessage || '—'}</TableCell>
                </TableRow>
              ))}
              {logs.length === 0 && (
                <TableRow>
                  <TableCell colSpan={6} align="center" sx={{ py: 3 }}>
                    <Typography variant="body2" color="text.secondary">{t('prompt_templates_logs_empty')}</Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>

      <Dialog open={formOpen} onClose={() => setFormOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>{formMode === 'create' ? t('prompt_templates_new') : t('prompt_templates_edit_title')}</DialogTitle>
        <DialogContent dividers>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <TextField
                label={t('prompt_templates_key')}
                fullWidth
                value={form.key}
                onChange={(e) => handleChange('key', e.target.value)}
                disabled={formMode === 'edit'}
              />
              <TextField
                label={t('prompt_templates_title_col')}
                fullWidth
                value={form.title}
                onChange={(e) => handleChange('title', e.target.value)}
              />
            </Stack>

            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <FormControl fullWidth>
                <InputLabel>{t('prompt_templates_locale')}</InputLabel>
                <Select
                  label={t('prompt_templates_locale')}
                  value={form.locale}
                  onChange={(e) => handleChange('locale', e.target.value)}
                >
                  {locales.map((code) => (
                    <MenuItem key={code} value={code}>{code}</MenuItem>
                  ))}
                </Select>
              </FormControl>

              <FormControl fullWidth>
                <InputLabel>{t('prompt_templates_provider')}</InputLabel>
                <Select
                  label={t('prompt_templates_provider')}
                  value={form.providerHint || ''}
                  onChange={(e) => handleChange('providerHint', e.target.value)}
                >
                  {providerHints.map((hint) => (
                    <MenuItem key={hint || 'none'} value={hint}>{hint || t('prompt_templates_provider_any')}</MenuItem>
                  ))}
                </Select>
              </FormControl>

              <FormControlLabel
                control={
                  <Switch
                    checked={form.isActive}
                    onChange={(e) => handleChange('isActive', e.target.checked)}
                  />
                }
                label={t('prompt_templates_active')}
              />
            </Stack>

            <TextField
              label={t('prompt_templates_description')}
              fullWidth
              value={form.description || ''}
              onChange={(e) => handleChange('description', e.target.value)}
            />

            <TextField
              label={t('prompt_templates_content')}
              fullWidth
              multiline
              minRows={8}
              value={form.content}
              onChange={(e) => handleChange('content', e.target.value)}
            />

            <TextField
              label={t('prompt_templates_output_hint')}
              fullWidth
              multiline
              minRows={2}
              value={form.outputFormatHint || ''}
              onChange={(e) => handleChange('outputFormatHint', e.target.value)}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setFormOpen(false)}>{t('prompt_templates_cancel')}</Button>
          <Button onClick={handleSave} variant="contained" disabled={saving}>
            {saving ? t('prompt_templates_saving') : t('prompt_templates_save')}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={testDialogOpen} onClose={() => setTestDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{t('prompt_templates_test')}</DialogTitle>
        <DialogContent dividers>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <FormControl fullWidth>
              <InputLabel>{t('prompt_templates_provider')}</InputLabel>
              <Select
                label={t('prompt_templates_provider')}
                value={testProvider}
                onChange={(e) => setTestProvider(e.target.value)}
              >
                {providerHints.map((hint) => (
                  <MenuItem key={hint || 'any'} value={hint || 'Ollama'}>
                    {hint || t('prompt_templates_provider_any')}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <TextField
              label={t('prompt_templates_model')}
              value={testModel}
              onChange={(e) => setTestModel(e.target.value)}
              helperText={t('prompt_templates_model_helper')}
              fullWidth
            />
            {testResult && (
              <Alert severity="info">
                {t('prompt_templates_duration')}: {testResult.duration.toFixed(0)} ms — {t('prompt_templates_model')}: {testResult.provider}/{testResult.model}
              </Alert>
            )}
            {testResult && (
              <>
                <Typography variant="subtitle2">{t('prompt_templates_prompt')}</Typography>
                <Paper variant="outlined" sx={{ p: 2, backgroundColor: 'grey.50' }}>
                  <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>{testResult.prompt}</Typography>
                </Paper>
                <Typography variant="subtitle2">{t('prompt_templates_response')}</Typography>
                <Paper variant="outlined" sx={{ p: 2 }}>
                  <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>{testResult.response}</Typography>
                </Paper>
              </>
            )}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setTestDialogOpen(false)}>{t('prompt_templates_close')}</Button>
          <Button onClick={runTest} variant="contained" disabled={testLoading || !testTemplate}>
            {testLoading ? t('prompt_templates_testing') : t('prompt_templates_test')}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default PromptTemplates;
