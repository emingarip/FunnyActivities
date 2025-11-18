import React from 'react';
import { useTranslation } from '../hooks/useTranslation';

const Settings: React.FC = () => {
  const { t } = useTranslation();
  return (
    <div className="page-container">
      <h1>{t('settings_title')}</h1>
      <p>{t('settings_description')}</p>
    </div>
  );
};

export default Settings;
