import React, { useEffect, useMemo, useState } from 'react';
import {
  Box,
  Button,
  Chip,
  Container,
  Divider,
  FormControl,
  FormControlLabel,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  SelectChangeEvent,
  Stack,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Tooltip,
  Typography,
  IconButton,
} from '@mui/material';
import { Add as AddIcon, Save as SaveIcon, Delete as DeleteIcon } from '@mui/icons-material';
import { useTranslation } from '../hooks/useTranslation';
import { builtInLocales, defaultLocale } from '../i18n/resources';

type DraftMap = Record<string, string>;

const TranslationAdmin: React.FC = () => {
  const {
    t,
    translations,
    availableLocales,
    updateTranslation,
    addLocale,
    locale,
    setLocale,
    removeLocale,
    localeMeta,
    updateLocaleMeta,
  } = useTranslation();

  const [selectedLocale, setSelectedLocale] = useState<string>(locale);
  const [searchTerm, setSearchTerm] = useState('');
  const [newLocale, setNewLocale] = useState('');
  const [newLocaleLabel, setNewLocaleLabel] = useState('');
  const [seedLocale, setSeedLocale] = useState('');
  const [newKey, setNewKey] = useState('');
  const [newValue, setNewValue] = useState('');
  const [drafts, setDrafts] = useState<DraftMap>({});
  const [showMissingOnly, setShowMissingOnly] = useState(false);
  const [flagInput, setFlagInput] = useState('');
  const [flagSvgInput, setFlagSvgInput] = useState('');
  const [labelInput, setLabelInput] = useState('');
  const [flagImagePreview, setFlagImagePreview] = useState('');

  useEffect(() => {
    setSelectedLocale(locale);
  }, [locale]);

  useEffect(() => {
    setDrafts(translations[selectedLocale] || {});
  }, [selectedLocale, translations]);
  useEffect(() => {
    setFlagInput(localeMeta[selectedLocale]?.flag || '');
    setLabelInput(localeMeta[selectedLocale]?.label || '');
    setFlagImagePreview(localeMeta[selectedLocale]?.flagImage || '');
    setFlagSvgInput('');
  }, [selectedLocale, localeMeta]);

  const allKeys = useMemo(() => {
    const keySet = new Set<string>();
    Object.values(translations).forEach((dict) => {
      Object.keys(dict || {}).forEach((key) => keySet.add(key));
    });
    return Array.from(keySet).sort();
  }, [translations]);

  const filteredKeys = useMemo(() => {
    const normalizedSearch = searchTerm.toLowerCase();
    return allKeys.filter((key) => {
      const currentValue = drafts[key] || '';
      if (showMissingOnly && currentValue.trim()) {
        return false;
      }
      if (!normalizedSearch) return true;
      return (
        key.toLowerCase().includes(normalizedSearch) ||
        currentValue.toLowerCase().includes(normalizedSearch)
      );
    });
  }, [allKeys, searchTerm, drafts, showMissingOnly]);

  const missingCount = useMemo(
    () =>
      allKeys.reduce((count, key) => {
        const value = drafts[key];
        return !value || !value.trim() ? count + 1 : count;
      }, 0),
    [allKeys, drafts]
  );

  const setActiveLocale = (code: string) => {
    setSelectedLocale(code);
    setLocale(code);
  };

  const handleLocaleChange = (event: SelectChangeEvent) => {
    const code = event.target.value as string;
    setActiveLocale(code);
  };

  const handleDraftChange = (key: string, value: string) => {
    setDrafts((prev) => ({ ...prev, [key]: value }));
  };

  const handleSaveKey = (key: string) => {
    updateTranslation(selectedLocale, key, drafts[key] || '');
  };

  const handleAddLocale = () => {
    const code = newLocale.trim();
    if (!code) return;
    addLocale(code, seedLocale || undefined);
    setSelectedLocale(code);
    setLocale(code);
    const trimmedLabel = newLocaleLabel.trim();
    if (trimmedLabel) {
      updateLocaleMeta(code, { label: trimmedLabel });
      setLabelInput(trimmedLabel);
    }
    setNewLocale('');
    setNewLocaleLabel('');
    setSeedLocale('');
  };

  const handleAddKey = () => {
    const key = newKey.trim();
    if (!key) return;
    updateTranslation(selectedLocale, key, newValue);
    if (selectedLocale !== defaultLocale) {
      updateTranslation(defaultLocale, key, newValue);
    }
    setDrafts((prev) => ({ ...prev, [key]: newValue }));
    setNewKey('');
    setNewValue('');
  };

  const handleDeleteLocale = (code: string) => {
    if (builtInLocales.includes(code)) {
      return;
    }
    const confirmMessage = t('translation_admin_confirm_delete')
      .replace('{0}', code.toUpperCase());
    if (window.confirm(confirmMessage)) {
      removeLocale(code);
    }
  };

  const handleSaveFlag = () => {
    const trimmed = flagInput.trim();
    if (trimmed) {
      updateLocaleMeta(selectedLocale, { flag: trimmed });
    } else {
      updateLocaleMeta(selectedLocale, { flag: undefined });
    }
    if (!flagImagePreview) {
      setFlagInput(trimmed);
    }
  };

  const handleSaveLabel = () => {
    const trimmed = labelInput.trim();
    if (trimmed) {
      updateLocaleMeta(selectedLocale, { label: trimmed });
    } else {
      updateLocaleMeta(selectedLocale, { label: undefined });
    }
  };

  const handleClearFlagImage = () => {
    updateLocaleMeta(selectedLocale, { flagImage: undefined });
    setFlagImagePreview('');
    setFlagSvgInput('');
  };

  const convertSvgToDataUrl = (svg: string) => {
    if (typeof window === 'undefined') {
      throw new Error('encoding_unavailable');
    }
    const trimmed = svg.trim();
    if (!trimmed.startsWith('<svg')) {
      throw new Error('invalid_svg');
    }
    const encoded = window.btoa(unescape(encodeURIComponent(trimmed)));
    return `data:image/svg+xml;base64,${encoded}`;
  };

  const handleSaveSvg = () => {
    const svgInput = flagSvgInput.trim();
    if (!svgInput) return;
    try {
      const dataUrl = convertSvgToDataUrl(svgInput);
      updateLocaleMeta(selectedLocale, { flagImage: dataUrl, flag: undefined });
      setFlagImagePreview(dataUrl);
      setFlagInput('');
      setFlagSvgInput('');
    } catch (error) {
      alert(t('translation_admin_svg_error'));
    }
  };

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>
        {t('translation_admin_title')}
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
        {t('translation_admin_description')}
      </Typography>

      <Stack spacing={3}>
        <Paper sx={{ p: 3 }}>
          <Stack spacing={2}>
            <Typography variant="h6">{t('translation_admin_selected_locale')}</Typography>
            <Box
              sx={{
                display: 'flex',
                flexWrap: 'wrap',
                gap: 2,
                alignItems: 'center',
              }}
            >
              <FormControl size="small" sx={{ minWidth: 160 }}>
                <InputLabel id="active-language-label">{t('translation_admin_selected_locale')}</InputLabel>
                <Select
                  labelId="active-language-label"
                  label={t('translation_admin_selected_locale')}
                  value={selectedLocale}
                  onChange={handleLocaleChange}
                >
                  {availableLocales.map((code) => (
                    <MenuItem key={code} value={code}>
                      {code.toUpperCase()}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              <Chip
                label={`${t('translation_admin_locale_chip')}: ${selectedLocale.toUpperCase()}`}
                color="primary"
                variant="outlined"
              />
              <Chip label={`${t('translation_admin_total_keys')}: ${allKeys.length}`} variant="outlined" />
              <Chip
                label={`${t('translation_admin_missing')}: ${missingCount}`}
                color={missingCount ? 'warning' : 'success'}
                variant="outlined"
              />
            </Box>
            <Typography variant="body2" color="text.secondary">
              {t('translation_admin_overview')}
            </Typography>
            <Divider />
            <Box>
              <Typography variant="subtitle2" color="text.secondary">
                {t('translation_admin_locale_list')}
              </Typography>
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mt: 1 }}>
                {availableLocales.map((code) => {
                  const isActive = code === selectedLocale;
                  const isBuiltIn = builtInLocales.includes(code);
                  return (
                    <Box key={code} sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                      <Chip
                        label={code.toUpperCase()}
                        color={isActive ? 'primary' : 'default'}
                        variant={isActive ? 'filled' : 'outlined'}
                        onClick={!isActive ? () => setActiveLocale(code) : undefined}
                        sx={{ cursor: isActive ? 'default' : 'pointer' }}
                      />
                      {!isBuiltIn && (
                        <Tooltip title={t('translation_admin_delete_language')}>
                          <IconButton
                            size="small"
                            onClick={() => handleDeleteLocale(code)}
                            aria-label={t('translation_admin_delete_language')}
                          >
                            <DeleteIcon fontSize="inherit" />
                          </IconButton>
                        </Tooltip>
                      )}
                    </Box>
                  );
                })}
              </Box>
              <Typography variant="caption" color="text.secondary">
                {t('translation_admin_delete_hint')}
              </Typography>
            </Box>
            <Divider />
            <Box>
              <Typography variant="subtitle2" color="text.secondary">
                {t('translation_admin_flag_label')}
              </Typography>
              <Box
                sx={{
                  display: 'flex',
                  flexWrap: 'wrap',
                  gap: 1,
                  alignItems: 'center',
                  mt: 1,
                }}
              >
                <TextField
                  value={flagInput}
                  onChange={(e) => setFlagInput(e.target.value)}
                  placeholder={t('translation_admin_flag_placeholder')}
                  size="small"
                  sx={{ minWidth: 120, flex: 1, maxWidth: 200 }}
                  disabled={!!flagImagePreview}
                />
                <Button
                  variant="outlined"
                  onClick={handleSaveFlag}
                  disabled={
                    flagInput === (localeMeta[selectedLocale]?.flag || '') || !!flagImagePreview
                  }
                >
                  {t('translation_admin_save_flag')}
                </Button>
                {flagImagePreview && (
                  <Button color="error" onClick={handleClearFlagImage}>
                    {t('translation_admin_clear_flag')}
                  </Button>
                )}
              </Box>
              <Stack direction="row" spacing={2} alignItems="center" sx={{ mt: 1 }}>
                {flagImagePreview && (
                  <img
                    src={flagImagePreview}
                    alt=""
                    style={{ width: 32, height: 32, borderRadius: 4, objectFit: 'cover' }}
                  />
                )}
                <Typography variant="caption" color="text.secondary">
                  {t('translation_admin_flag_hint')}
                </Typography>
              </Stack>
              <Box
                sx={{
                  mt: 2,
                  display: 'flex',
                  flexDirection: 'column',
                  gap: 1,
                }}
              >
                <TextField
                  label={t('translation_admin_svg_label')}
                  placeholder={t('translation_admin_svg_placeholder')}
                  multiline
                  minRows={3}
                  value={flagSvgInput}
                  onChange={(e) => setFlagSvgInput(e.target.value)}
                  disabled={!!flagImagePreview}
                />
                <Button
                  variant="outlined"
                  onClick={handleSaveSvg}
                  disabled={!flagSvgInput.trim() || !!flagImagePreview}
                >
                  {t('translation_admin_save_svg')}
                </Button>
                <Typography variant="caption" color="text.secondary">
                  {t('translation_admin_svg_hint')}
                </Typography>
              </Box>
            </Box>
            <Divider />
            <Box>
              <Typography variant="subtitle2" color="text.secondary">
                {t('translation_admin_label_label')}
              </Typography>
              <Box
                sx={{
                  display: 'flex',
                  flexWrap: 'wrap',
                  gap: 1,
                  alignItems: 'center',
                  mt: 1,
                }}
              >
                <TextField
                  value={labelInput}
                  onChange={(e) => setLabelInput(e.target.value)}
                  placeholder={t('translation_admin_language_label_placeholder')}
                  size="small"
                  sx={{ minWidth: 120, flex: 1, maxWidth: 240 }}
                />
                <Button
                  variant="outlined"
                  onClick={handleSaveLabel}
                  disabled={labelInput === (localeMeta[selectedLocale]?.label || '')}
                >
                  {t('translation_admin_save_label')}
                </Button>
              </Box>
              <Typography variant="caption" color="text.secondary">
                {t('translation_admin_label_hint')}
              </Typography>
            </Box>
          </Stack>
        </Paper>

        <Paper sx={{ p: 3 }}>
          <Stack spacing={2}>
            <Box>
              <Typography variant="h6">{t('translation_admin_add_language')}</Typography>
              <Typography variant="body2" color="text.secondary">
                {t('translation_admin_new_language_help')}
              </Typography>
            </Box>
            <Box
              sx={{
                display: 'grid',
                gap: 2,
                gridTemplateColumns: { xs: '1fr', sm: 'repeat(3, minmax(0, 1fr))' },
              }}
            >
              <TextField
                fullWidth
                label={t('translation_admin_language_code')}
                placeholder={t('translation_admin_language_placeholder')}
                value={newLocale}
                onChange={(e) => setNewLocale(e.target.value)}
              />
              <TextField
                fullWidth
                label={t('translation_admin_language_label')}
                placeholder={t('translation_admin_language_label_placeholder')}
                value={newLocaleLabel}
                onChange={(e) => setNewLocaleLabel(e.target.value)}
              />
              <FormControl fullWidth>
                <InputLabel id="seed-language-label">{t('translation_admin_seed_language')}</InputLabel>
                <Select
                  labelId="seed-language-label"
                  label={t('translation_admin_seed_language')}
                  value={seedLocale}
                  onChange={(e) => setSeedLocale(e.target.value)}
                >
                  <MenuItem value="">{t('translation_admin_seed_empty')}</MenuItem>
                  {availableLocales.map((code) => (
                    <MenuItem key={code} value={code}>
                      {code.toUpperCase()}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Box>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={handleAddLocale}
              disabled={!newLocale.trim()}
              sx={{ alignSelf: { xs: 'stretch', sm: 'flex-start' } }}
            >
              {t('translation_admin_add_language')}
            </Button>
          </Stack>
        </Paper>

        <Paper sx={{ p: 3 }}>
          <Stack spacing={2}>
            <Box>
              <Typography variant="h6">{t('translation_admin_add_key')}</Typography>
              <Typography variant="body2" color="text.secondary">
                {t('translation_admin_add_key_helper')}
              </Typography>
            </Box>
            <Box
              sx={{
                display: 'grid',
                gap: 2,
                gridTemplateColumns: { xs: '1fr', md: '2fr 3fr 1fr' },
                alignItems: 'center',
              }}
            >
              <TextField
                fullWidth
                label={t('translation_admin_key')}
                placeholder="common.new_key"
                value={newKey}
                onChange={(e) => setNewKey(e.target.value)}
              />
              <TextField
                fullWidth
                label={t('translation_admin_value')}
                placeholder={t('translation_admin_value_placeholder')}
                value={newValue}
                onChange={(e) => setNewValue(e.target.value)}
              />
              <Button
                fullWidth
                variant="outlined"
                startIcon={<AddIcon />}
                onClick={handleAddKey}
                disabled={!newKey.trim()}
              >
                {t('translation_admin_create_key')}
              </Button>
            </Box>
          </Stack>
        </Paper>

        <Paper sx={{ p: 3 }}>
          <Stack spacing={2}>
            <Box
              sx={{
                display: 'flex',
                flexWrap: 'wrap',
                gap: 2,
                alignItems: 'center',
                justifyContent: 'space-between',
              }}
            >
              <TextField
                label={t('translation_admin_search')}
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                size="small"
                sx={{ flex: 1, minWidth: 220 }}
              />
              <FormControlLabel
                control={
                  <Switch
                    checked={showMissingOnly}
                    onChange={(e) => setShowMissingOnly(e.target.checked)}
                  />
                }
                label={t('translation_admin_show_missing')}
              />
            </Box>
            <Typography variant="body2" color="text.secondary">
              {t('translation_admin_missing_hint').replace('{0}', missingCount.toString())}
            </Typography>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>{t('translation_admin_key')}</TableCell>
                    <TableCell>{t('translation_admin_value')}</TableCell>
                    <TableCell>{t('translation_admin_default')}</TableCell>
                    <TableCell align="right">{t('translation_admin_actions')}</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {filteredKeys.map((key) => {
                    const fallbackValue = translations[defaultLocale]?.[key] || '';
                    const localeValue = translations[selectedLocale]?.[key] ?? '';
                    const currentValue = drafts[key] ?? '';
                    const isDirty = currentValue !== localeValue;
                    const isMissing = !currentValue || !currentValue.trim();
                    return (
                      <TableRow
                        key={key}
                        sx={{
                          backgroundColor: isMissing ? 'rgba(255, 193, 7, 0.08)' : undefined,
                        }}
                      >
                        <TableCell sx={{ minWidth: 200, fontFamily: 'monospace' }}>{key}</TableCell>
                        <TableCell>
                          <TextField
                            fullWidth
                            size="small"
                            value={currentValue}
                            placeholder={fallbackValue || t('translation_admin_value_placeholder')}
                            onChange={(e) => handleDraftChange(key, e.target.value)}
                            error={isMissing}
                          />
                        </TableCell>
                        <TableCell sx={{ minWidth: 200 }}>
                          <Typography variant="body2" color="text.secondary">
                            {fallbackValue}
                          </Typography>
                        </TableCell>
                        <TableCell align="right" sx={{ minWidth: 100 }}>
                          <Tooltip
                            title={
                              isDirty ? t('translation_admin_save') : t('translation_admin_saved_state')
                            }
                          >
                            <span>
                              <IconButton
                                color="primary"
                                onClick={() => handleSaveKey(key)}
                                disabled={!isDirty}
                              >
                                <SaveIcon />
                              </IconButton>
                            </span>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    );
                  })}
                  {filteredKeys.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={4} align="center">
                        <Typography variant="body2" color="text.secondary">
                          {allKeys.length === 0
                            ? t('translation_admin_no_keys')
                            : t('translation_admin_no_results')}
                        </Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </Stack>
        </Paper>
      </Stack>
    </Container>
  );
};

export default TranslationAdmin;
