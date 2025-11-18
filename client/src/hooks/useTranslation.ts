import { useLocalization } from '../contexts/LocalizationContext';

export const useTranslation = () => {
  const { t, locale, setLocale } = useLocalization();
  return { t, locale, setLocale };
};
