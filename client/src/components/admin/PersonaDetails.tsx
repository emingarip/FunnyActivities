import React from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Chip,
  Avatar,
  Divider,
  Card,
  CardContent,
} from '@mui/material';
import {
  Person as PersonIcon,
} from '@mui/icons-material';

interface PersonaCharacteristic {
  id: string;
  name: string;
  value: string;
  type?: string;
  order: number;
}

interface PersonaActivityAssociation {
  id: string;
  activityId: string;
  activityName: string;
  preferenceLevel: number;
}

interface Persona {
  id: string;
  userId: string;
  name: string;
  displayName?: string;
  description?: string;
  avatarImageUrl?: string;
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
   images?: PersonaImage[];
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

interface PersonaDetailsProps {
  persona: Persona;
}

const PersonaDetails: React.FC<PersonaDetailsProps> = ({ persona }) => {
  const formatCharacteristics = (characteristics: PersonaCharacteristic[]) => {
    if (!characteristics || characteristics.length === 0) return 'No characteristics';
    return characteristics.map(c => `${c.name}: ${c.value}`).join(', ');
  };

  const getActivityCount = (activityAssociations: PersonaActivityAssociation[]) => {
    return activityAssociations?.length || 0;
  };

  const getPersonalityTraits = () => {
    const traits = [];
    if (persona.openness !== undefined) traits.push({ name: 'Openness', value: persona.openness });
    if (persona.conscientiousness !== undefined) traits.push({ name: 'Conscientiousness', value: persona.conscientiousness });
    if (persona.extraversion !== undefined) traits.push({ name: 'Extraversion', value: persona.extraversion });
    if (persona.agreeableness !== undefined) traits.push({ name: 'Agreeableness', value: persona.agreeableness });
    if (persona.neuroticism !== undefined) traits.push({ name: 'Neuroticism', value: persona.neuroticism });
    return traits;
  };

  const images = persona.images ?? [];
  const avatarUrl = persona.avatarImageUrl
    || images.find(img => img.imageType === 'original')?.preSignedUrl
    || images[0]?.preSignedUrl
    || '';

  return (
    <Box sx={{ p: 2 }}>
      <Typography variant="h5" gutterBottom>
        Persona Details
      </Typography>

      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
        {/* Basic Information */}
        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <Avatar
                src={avatarUrl}
                sx={{ width: 64, height: 64, mr: 2 }}
              >
                <PersonIcon sx={{ fontSize: 32 }} />
              </Avatar>
              <Box>
                <Typography variant="h6">
                  {persona.displayName || persona.name}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {persona.name}
                </Typography>
                {persona.type && (
                  <Chip
                    label={persona.type}
                    size="small"
                    variant="outlined"
                    sx={{ mt: 1 }}
                  />
                )}
              </Box>
            </Box>

            {images.length > 0 && (
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 2 }}>
                {images
                  .filter(img => img.imageType === 'original')
                  .map(img => (
                    <Box key={img.id} sx={{ width: 72, height: 72, borderRadius: 1, overflow: 'hidden', border: '1px solid', borderColor: 'divider' }}>
                      <img src={img.preSignedUrl} alt={img.fileName} style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
                    </Box>
                  ))}
              </Box>
            )}

            {persona.description && (
              <Typography variant="body1" sx={{ mb: 2 }}>
                {persona.description}
              </Typography>
            )}

            {/* Demographic Information */}
            {(persona.age || persona.gender || persona.nationality) && (
              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>
                  Demographics
                </Typography>
                <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                  {persona.age && (
                    <Typography variant="body2">
                      <strong>Age:</strong> {persona.age}
                    </Typography>
                  )}
                  {persona.gender && (
                    <Typography variant="body2">
                      <strong>Gender:</strong> {persona.gender}
                    </Typography>
                  )}
                  {persona.nationality && (
                    <Typography variant="body2">
                      <strong>Nationality:</strong> {persona.nationality}
                    </Typography>
                  )}
                </Box>
              </Box>
            )}

            {/* Biography */}
            {persona.biography && (
              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>
                  Biography
                </Typography>
                <Typography variant="body1" sx={{ whiteSpace: 'pre-wrap' }}>
                  {persona.biography}
                </Typography>
              </Box>
            )}

            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
              <Typography variant="body2" color="text.secondary">
                Created: {new Date(persona.createdAt).toLocaleDateString()}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Updated: {new Date(persona.updatedAt).toLocaleDateString()}
              </Typography>
            </Box>
          </CardContent>
        </Card>

        {/* Personality Traits */}
        {getPersonalityTraits().length > 0 && (
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Personality Traits (Big Five)
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Scale: 0 (low) to 100 (high)
              </Typography>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                {getPersonalityTraits().map((trait) => (
                  <Box key={trait.name} sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Typography variant="body2" sx={{ minWidth: '120px' }}>
                      {trait.name}:
                    </Typography>
                    <Box sx={{ flex: 1, display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Box
                        sx={{
                          flex: 1,
                          height: 8,
                          bgcolor: 'grey.300',
                          borderRadius: 1,
                          overflow: 'hidden',
                        }}
                      >
                        <Box
                          sx={{
                            width: `${trait.value}%`,
                            height: '100%',
                            bgcolor: 'primary.main',
                            borderRadius: 1,
                          }}
                        />
                      </Box>
                      <Typography variant="body2" sx={{ minWidth: '30px' }}>
                        {trait.value}
                      </Typography>
                    </Box>
                  </Box>
                ))}
              </Box>
            </CardContent>
          </Card>
        )}

        {/* Characteristics */}
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Characteristics
            </Typography>
            {persona.characteristics && persona.characteristics.length > 0 ? (
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                {persona.characteristics
                  .sort((a, b) => a.order - b.order)
                  .map((characteristic) => (
                    <Box
                      key={characteristic.id}
                      sx={{
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between',
                        p: 1,
                        border: 1,
                        borderColor: 'divider',
                        borderRadius: 1,
                      }}
                    >
                      <Box>
                        <Typography variant="body1" fontWeight="medium">
                          {characteristic.name}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          {characteristic.value}
                        </Typography>
                      </Box>
                      {characteristic.type && (
                        <Chip
                          label={characteristic.type}
                          size="small"
                          variant="outlined"
                        />
                      )}
                    </Box>
                  ))}
              </Box>
            ) : (
              <Typography variant="body2" color="text.secondary">
                No characteristics defined
              </Typography>
            )}
          </CardContent>
        </Card>

        {/* Activity Associations */}
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Activity Preferences
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              {getActivityCount(persona.activityAssociations)} associated activities
            </Typography>
            {persona.activityAssociations && persona.activityAssociations.length > 0 ? (
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                {persona.activityAssociations
                  .sort((a, b) => b.preferenceLevel - a.preferenceLevel)
                  .map((association) => (
                    <Box
                      key={association.id}
                      sx={{
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between',
                        p: 1,
                        border: 1,
                        borderColor: 'divider',
                        borderRadius: 1,
                      }}
                    >
                      <Typography variant="body1">
                        {association.activityName}
                      </Typography>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography variant="body2" color="text.secondary">
                          Preference:
                        </Typography>
                        <Chip
                          label={`${association.preferenceLevel}/5`}
                          size="small"
                          color={
                            association.preferenceLevel >= 4 ? 'success' :
                            association.preferenceLevel >= 3 ? 'primary' :
                            association.preferenceLevel >= 2 ? 'warning' : 'error'
                          }
                        />
                        <Typography variant="body2" color="text.secondary">
                          ({association.preferenceLevel >= 4 ? 'High' :
                            association.preferenceLevel >= 3 ? 'Medium' :
                            association.preferenceLevel >= 2 ? 'Low' : 'Very Low'})
                        </Typography>
                      </Box>
                    </Box>
                  ))}
              </Box>
            ) : (
              <Typography variant="body2" color="text.secondary">
                No activity associations defined
              </Typography>
            )}
          </CardContent>
        </Card>
      </Box>
    </Box>
  );
};

export default PersonaDetails;
