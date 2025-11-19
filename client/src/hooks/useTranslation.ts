import { useLocalization } from '../contexts/LocalizationContext';

export const useTranslation = () => {
  return useLocalization();
};
