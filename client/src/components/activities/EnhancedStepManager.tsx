import React, { useState, useCallback, useRef, useEffect } from 'react';
import {
  Box,
  Paper,
  Typography,
  Button,
  IconButton,
  TextField,
  Card,
  CardContent,
  Grid,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Tooltip,
  Chip,
  Alert,
  useTheme,
  useMediaQuery,
  Fab,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Undo as UndoIcon,
  Redo as RedoIcon,
  PlayArrow as PlayIcon,
  AttachFile as AttachFileIcon,
  Image as ImageIcon,
  Videocam as VideoIcon,
  Audiotrack as AudioIcon,
  Description as DocumentIcon,
  AccessTime as TimeIcon,
} from '@mui/icons-material';
import VideoPlayerWithScrubber from './VideoPlayerWithScrubber';
import {
  EnhancedStepDto,
  TimelineMarker,
  StepHistoryState,
  StepHistoryAction,
  StepMediaAttachment,
} from '../../services/api.types';

interface EnhancedStepManagerProps {
  activityId: string;
  videoUrl?: string;
  steps: EnhancedStepDto[];
  onStepsChange: (steps: EnhancedStepDto[]) => void;
  onStepCreate?: (step: EnhancedStepDto) => Promise<void>;
  onStepUpdate?: (stepId: string, step: Partial<EnhancedStepDto>) => Promise<void>;
  onStepDelete?: (stepId: string) => Promise<void>;
  readOnly?: boolean;
}

const EnhancedStepManager: React.FC<EnhancedStepManagerProps> = ({
  activityId,
  videoUrl,
  steps,
  onStepsChange,
  onStepCreate,
  onStepUpdate,
  onStepDelete,
  readOnly = false,
}) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  // State management
  const [currentTime, setCurrentTime] = useState(0);
  const [selectedStep, setSelectedStep] = useState<EnhancedStepDto | null>(null);
  const [editingStep, setEditingStep] = useState<EnhancedStepDto | null>(null);
  const [stepDialogOpen, setStepDialogOpen] = useState(false);
  const [history, setHistory] = useState<StepHistoryState>({
    past: [],
    present: steps,
    future: [],
  });
  const videoRef = useRef<HTMLVideoElement>(null);

  // Undo/Redo functionality
  const saveToHistory = useCallback((newSteps: EnhancedStepDto[], action: StepHistoryAction) => {
    setHistory(prev => ({
      past: [...prev.past, { ...action, previousState: prev.present }],
      present: newSteps,
      future: [],
    }));
  }, []);

  const undo = useCallback(() => {
    setHistory(prev => {
      if (prev.past.length === 0) return prev;

      const lastAction = prev.past[prev.past.length - 1];
      const newPast = prev.past.slice(0, -1);

      return {
        past: newPast,
        present: lastAction.previousState || [],
        future: [lastAction, ...prev.future],
      };
    });
  }, []);

  const redo = useCallback(() => {
    setHistory(prev => {
      if (prev.future.length === 0) return prev;

      const nextAction = prev.future[0];
      const newFuture = prev.future.slice(1);

      return {
        past: [...prev.past, nextAction],
        present: nextAction.newState || [],
        future: newFuture,
      };
    });
  }, []);

  // Sync steps with history
  useEffect(() => {
    onStepsChange(history.present);
  }, [history.present, onStepsChange]);

  // Timeline markers for video player
  const timelineMarkers: TimelineMarker[] = history.present
    .filter(step => step.timestampSeconds !== undefined)
    .map(step => ({
      id: step.id || `step-${step.order}`,
      time: step.timestampSeconds!,
      type: 'step' as const,
      label: `Step ${step.order}: ${step.description.substring(0, 30)}${step.description.length > 30 ? '...' : ''}`,
      color: theme.palette.primary.main,
      data: step,
    }));

  // Video player handlers
  const handleTimeChange = useCallback((time: number) => {
    setCurrentTime(time);
  }, []);

  const handleMarkerClick = useCallback((marker: TimelineMarker) => {
    setSelectedStep(marker.data);
  }, []);

  const handleMarkerAdd = useCallback((time: number) => {
    // Pause the video if it's playing
    if (videoRef.current && !videoRef.current.paused) {
      videoRef.current.pause();
    }

    const newStep: EnhancedStepDto = {
      activityId,
      order: history.present.length + 1,
      description: `Step at ${Math.floor(time / 60)}:${(time % 60).toString().padStart(2, '0')}`,
      timestampSeconds: time,
    };

    const newSteps = [...history.present, newStep];
    saveToHistory(newSteps, {
      type: 'create',
      stepId: newStep.id,
      newState: newSteps,
      timestamp: Date.now(),
    });

    setEditingStep(newStep);
    setStepDialogOpen(true);
  }, [activityId, history.present, saveToHistory]);

  // Step CRUD operations
  const handleCreateStep = useCallback(async (stepData: Partial<EnhancedStepDto>) => {
    const newStep: EnhancedStepDto = {
      activityId,
      order: stepData.order || history.present.length + 1,
      description: stepData.description || '',
      timestampSeconds: stepData.timestampSeconds,
      durationSeconds: stepData.durationSeconds,
      pauseTimeSeconds: stepData.pauseTimeSeconds,
      mediaAttachments: stepData.mediaAttachments || [],
    };

    try {
      if (onStepCreate) {
        await onStepCreate(newStep);
      }

      const newSteps = [...history.present, newStep];
      saveToHistory(newSteps, {
        type: 'create',
        stepId: newStep.id,
        newState: newSteps,
        timestamp: Date.now(),
      });

      setStepDialogOpen(false);
      setEditingStep(null);
    } catch (error) {
      console.error('Failed to create step:', error);
    }
  }, [activityId, history.present, onStepCreate, saveToHistory]);

  const handleUpdateStep = useCallback(async (stepId: string, updates: Partial<EnhancedStepDto>) => {
    const stepIndex = history.present.findIndex(s => s.id === stepId);
    if (stepIndex === -1) return;

    const oldStep = history.present[stepIndex];
    const updatedStep = { ...oldStep, ...updates };
    const newSteps = [...history.present];
    newSteps[stepIndex] = updatedStep;

    try {
      if (onStepUpdate) {
        await onStepUpdate(stepId, updates);
      }

      saveToHistory(newSteps, {
        type: 'update',
        stepId,
        previousState: history.present,
        newState: newSteps,
        timestamp: Date.now(),
      });

      setStepDialogOpen(false);
      setEditingStep(null);
    } catch (error) {
      console.error('Failed to update step:', error);
    }
  }, [history.present, onStepUpdate, saveToHistory]);

  const handleDeleteStep = useCallback(async (stepId: string) => {
    const stepIndex = history.present.findIndex(s => s.id === stepId);
    if (stepIndex === -1) return;

    const stepToDelete = history.present[stepIndex];
    const newSteps = history.present.filter(s => s.id !== stepId);

    // Reorder remaining steps
    newSteps.forEach((step, index) => {
      step.order = index + 1;
    });

    try {
      if (onStepDelete) {
        await onStepDelete(stepId);
      }

      saveToHistory(newSteps, {
        type: 'delete',
        stepId,
        previousState: history.present,
        newState: newSteps,
        timestamp: Date.now(),
      });
    } catch (error) {
      console.error('Failed to delete step:', error);
    }
  }, [history.present, onStepDelete, saveToHistory]);

  // Step editing dialog
  const StepDialog: React.FC = () => (
    <Dialog
      open={stepDialogOpen}
      onClose={() => {
        setStepDialogOpen(false);
        setEditingStep(null);
      }}
      maxWidth="md"
      fullWidth
      fullScreen={isMobile}
    >
      <DialogTitle>
        {editingStep?.id ? 'Edit Step' : 'Create New Step'}
      </DialogTitle>
      <DialogContent>
        <StepForm
          step={editingStep}
          currentTime={currentTime}
          onSave={(stepData) => {
            if (editingStep?.id) {
              handleUpdateStep(editingStep.id, stepData);
            } else {
              handleCreateStep(stepData);
            }
          }}
          onCancel={() => {
            setStepDialogOpen(false);
            setEditingStep(null);
          }}
        />
      </DialogContent>
    </Dialog>
  );

  // Step form component
  const StepForm: React.FC<{
    step: EnhancedStepDto | null;
    currentTime: number;
    onSave: (step: Partial<EnhancedStepDto>) => void;
    onCancel: () => void;
  }> = ({ step, currentTime, onSave, onCancel }) => {
    const [formData, setFormData] = useState<Partial<EnhancedStepDto>>(step || {
      description: '',
      timestampSeconds: currentTime,
      durationSeconds: 0,
      pauseTimeSeconds: 0,
    });

    const [mediaFiles, setMediaFiles] = useState<File[]>([]);
    const fileInputRef = useRef<HTMLInputElement>(null);

    const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
      const files = Array.from(event.target.files || []);
      setMediaFiles(prev => [...prev, ...files]);
    };

    const handleSave = () => {
      const mediaAttachments: StepMediaAttachment[] = mediaFiles.map(file => ({
        type: file.type.startsWith('image/') ? 'image' :
              file.type.startsWith('video/') ? 'video' :
              file.type.startsWith('audio/') ? 'audio' : 'document',
        filename: file.name,
        file,
        isNew: true,
      }));

      onSave({
        ...formData,
        mediaAttachments: [...(formData.mediaAttachments || []), ...mediaAttachments],
      });
    };

    return (
      <Box sx={{ pt: 2, display: 'flex', flexDirection: 'column', gap: 3 }}>
        <TextField
          fullWidth
          label="Description"
          multiline
          rows={3}
          value={formData.description || ''}
          onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
          required
        />

        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <TextField
            fullWidth
            label="Timestamp (seconds)"
            type="number"
            value={formData.timestampSeconds || 0}
            onChange={(e) => setFormData(prev => ({
              ...prev,
              timestampSeconds: parseFloat(e.target.value) || 0
            }))}
            InputProps={{
              endAdornment: (
                <Button
                  size="small"
                  onClick={() => setFormData(prev => ({ ...prev, timestampSeconds: currentTime }))}
                  sx={{ minHeight: 44 }}
                >
                  Current
                </Button>
              ),
            }}
          />
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              fullWidth
              label="Duration (seconds)"
              type="number"
              value={formData.durationSeconds || 0}
              onChange={(e) => setFormData(prev => ({
                ...prev,
                durationSeconds: parseFloat(e.target.value) || 0
              }))}
            />
            <TextField
              fullWidth
              label="Pause After (seconds)"
              type="number"
              value={formData.pauseTimeSeconds || 0}
              onChange={(e) => setFormData(prev => ({
                ...prev,
                pauseTimeSeconds: parseFloat(e.target.value) || 0
              }))}
            />
          </Box>
        </Box>

        {/* Media Attachments */}
        <Box>
          <Typography variant="h6" gutterBottom>
            Media Attachments
          </Typography>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
            <Button
              variant="outlined"
              component="label"
              startIcon={<AttachFileIcon />}
              sx={{ minHeight: 44 }}
            >
              Add Files
              <input
                ref={fileInputRef}
                type="file"
                multiple
                accept="image/*,video/*,audio/*,.pdf,.doc,.docx"
                hidden
                onChange={handleFileSelect}
              />
            </Button>
            <Typography variant="caption" color="text.secondary">
              Supports images, videos, audio files, and documents
            </Typography>
          </Box>

          {/* Existing and new media files */}
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
            {mediaFiles.map((file, index) => (
              <Chip
                key={index}
                label={file.name}
                onDelete={() => setMediaFiles(prev => prev.filter((_, i) => i !== index))}
                icon={
                  file.type.startsWith('image/') ? <ImageIcon /> :
                  file.type.startsWith('video/') ? <VideoIcon /> :
                  file.type.startsWith('audio/') ? <AudioIcon /> :
                  <DocumentIcon />
                }
              />
            ))}
          </Box>
        </Box>

        <DialogActions>
          <Button onClick={onCancel} sx={{ minHeight: 44 }}>Cancel</Button>
          <Button
            onClick={handleSave}
            variant="contained"
            disabled={!formData.description?.trim()}
            sx={{ minHeight: 44 }}
          >
            {step?.id ? 'Update' : 'Create'} Step
          </Button>
        </DialogActions>
      </Box>
    );
  };

  return (
    <Box sx={{ width: '100%' }}>
      {/* Video Player Section */}
      {videoUrl && (
        <Paper sx={{ p: { xs: 1, sm: 2 }, mb: 3 }}>
          <Typography variant="h6" gutterBottom sx={{ fontSize: { xs: '1.1rem', sm: '1.25rem' } }}>
            Video Preview & Step Creation
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2, fontSize: { xs: '1rem', sm: '0.875rem' } }}>
            Play the video and click the "+" button or use timeline markers to create steps at specific timestamps.
          </Typography>

          <Box sx={{ position: 'relative', width: '100%', height: { xs: 250, sm: 400 } }}>
            <VideoPlayerWithScrubber
              ref={videoRef}
              src={videoUrl}
              markers={timelineMarkers}
              onTimeChange={handleTimeChange}
              onMarkerClick={handleMarkerClick}
              onMarkerAdd={handleMarkerAdd}
              width="100%"
              height="100%"
            />
          </Box>

          <Box
            sx={{
              mt: 2,
              display: 'flex',
              gap: 1,
              flexDirection: { xs: 'column', sm: 'row' },
              alignItems: { xs: 'stretch', sm: 'center' }
            }}
          >
            {(() => {
              const hours = Math.floor(currentTime / 3600);
              const minutes = Math.floor((currentTime % 3600) / 60);
              const seconds = Math.floor(currentTime % 60);
              const formattedTime = `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
              return (
                <Typography
                  variant="body2"
                  sx={{ fontSize: { xs: '1rem', sm: '0.875rem' } }}
                  aria-live="polite"
                  aria-label={`Current video playback time: ${formattedTime}`}
                >
                  Current Time: {formattedTime}
                </Typography>
              );
            })()}
            <Button
              size="small"
              startIcon={<AddIcon />}
              onClick={() => handleMarkerAdd(currentTime)}
              variant="outlined"
              fullWidth={isMobile}
              sx={{ minWidth: { xs: '100%', sm: 'auto' }, minHeight: 44 }}
            >
              Add Step Here
            </Button>
          </Box>
        </Paper>
      )}

      {/* Steps Management Section */}
      <Paper sx={{ p: { xs: 1, sm: 2 } }}>
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            mb: 2,
            flexDirection: { xs: 'column', sm: 'row' },
            gap: { xs: 2, sm: 0 }
          }}
        >
          <Typography
            variant="h6"
            sx={{ fontSize: { xs: '1.1rem', sm: '1.25rem' } }}
            aria-live="polite"
          >
            Steps ({history.present.length})
          </Typography>

          <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
            <Tooltip title="Undo last action">
              <span>
                <IconButton
                  onClick={undo}
                  disabled={history.past.length === 0}
                  color="primary"
                  aria-label={`Undo (${history.past.length} actions available)`}
                >
                  <UndoIcon />
                </IconButton>
              </span>
            </Tooltip>

            <Tooltip title="Redo last undone action">
              <span>
                <IconButton
                  onClick={redo}
                  disabled={history.future.length === 0}
                  color="primary"
                  aria-label={`Redo (${history.future.length} actions available)`}
                >
                  <RedoIcon />
                </IconButton>
              </span>
            </Tooltip>

            {!readOnly && (
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={() => {
                  setEditingStep(null);
                  setStepDialogOpen(true);
                }}
                size={isMobile ? "small" : "medium"}
                fullWidth={isMobile}
                sx={{ minHeight: 44 }}
              >
                Add Step
              </Button>
            )}
          </Box>
        </Box>

        {history.present.length === 0 ? (
          <Alert severity="info">
            No steps created yet. {videoUrl ? 'Play the video above and add steps at specific timestamps.' : 'Click "Add Step" to create your first step.'}
          </Alert>
        ) : (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            {history.present
              .sort((a, b) => (a.timestampSeconds || 0) - (b.timestampSeconds || 0))
              .map((step) => (
                <Card
                  key={step.id || `step-${step.order}`}
                  variant="outlined"
                  sx={{
                    transition: 'all 0.2s',
                    '&:hover': {
                      boxShadow: 2,
                    }
                  }}
                >
                  <CardContent sx={{ p: { xs: 2, sm: 3 } }}>
                    <Box
                      sx={{
                        display: 'flex',
                        justifyContent: 'space-between',
                        alignItems: 'flex-start',
                        flexDirection: { xs: 'column', sm: 'row' },
                        gap: { xs: 2, sm: 0 }
                      }}
                    >
                      <Box sx={{ flex: 1, width: '100%' }}>
                        <Box
                          sx={{
                            display: 'flex',
                            alignItems: 'center',
                            gap: 1,
                            mb: 1,
                            flexWrap: 'wrap'
                          }}
                        >
                          <Chip
                            label={`Step ${step.order}`}
                            color="primary"
                            size="small"
                            aria-label={`Step number ${step.order}`}
                          />
                          {step.timestampSeconds !== undefined && (
                            <Chip
                              icon={<TimeIcon />}
                              label={`${Math.floor(step.timestampSeconds / 60)}:${(step.timestampSeconds % 60).toString().padStart(2, '0')}`}
                              variant="outlined"
                              size="small"
                              aria-label={`Timestamp: ${Math.floor(step.timestampSeconds / 60)} minutes ${step.timestampSeconds % 60} seconds`}
                            />
                          )}
                        </Box>

                        <Typography
                          variant="body1"
                          sx={{ mb: 1, fontSize: { xs: '1rem', sm: '1rem' } }}
                        >
                          {step.description}
                        </Typography>

                        {(step.durationSeconds || step.pauseTimeSeconds) && (
                          <Box sx={{ display: 'flex', gap: 2, mb: 1, flexWrap: 'wrap' }}>
                            {step.durationSeconds && (
                              <Typography
                                variant="caption"
                                color="text.secondary"
                                aria-label={`Step duration: ${step.durationSeconds} seconds`}
                              >
                                Duration: {step.durationSeconds}s
                              </Typography>
                            )}
                            {step.pauseTimeSeconds && (
                              <Typography
                                variant="caption"
                                color="text.secondary"
                                aria-label={`Pause after step: ${step.pauseTimeSeconds} seconds`}
                              >
                                Pause: {step.pauseTimeSeconds}s
                              </Typography>
                            )}
                          </Box>
                        )}

                        {step.mediaAttachments && step.mediaAttachments.length > 0 && (
                          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                            {step.mediaAttachments.map((attachment, index) => (
                              <Chip
                                key={index}
                                size="small"
                                label={attachment.filename}
                                aria-label={`${attachment.type} attachment: ${attachment.filename}`}
                                icon={
                                  attachment.type === 'image' ? <ImageIcon /> :
                                  attachment.type === 'video' ? <VideoIcon /> :
                                  attachment.type === 'audio' ? <AudioIcon /> :
                                  <DocumentIcon />
                                }
                              />
                            ))}
                          </Box>
                        )}
                      </Box>

                      {!readOnly && (
                        <Box sx={{ display: 'flex', gap: 1, alignSelf: { xs: 'flex-end', sm: 'flex-start' } }}>
                          <Tooltip title="Edit step">
                            <IconButton
                              size="small"
                              onClick={() => {
                                setEditingStep(step);
                                setStepDialogOpen(true);
                              }}
                              color="primary"
                              aria-label={`Edit step ${step.order}`}
                            >
                              <EditIcon />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete step">
                            <IconButton
                              size="small"
                              onClick={() => handleDeleteStep(step.id!)}
                              color="error"
                              aria-label={`Delete step ${step.order}`}
                            >
                              <DeleteIcon />
                            </IconButton>
                          </Tooltip>
                        </Box>
                      )}
                    </Box>
                  </CardContent>
                </Card>
              ))}
          </Box>
        )}
      </Paper>

      {/* Step Dialog */}
      <StepDialog />
    </Box>
  );
};

export default EnhancedStepManager;