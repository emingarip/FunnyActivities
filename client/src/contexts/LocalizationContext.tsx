import React, { createContext, useContext, useMemo, useState, useEffect, ReactNode } from 'react';
import { resources, SupportedLocale, defaultLocale } from '../i18n/resources';

type LocalizationContextValue = {
  locale: SupportedLocale;
  t: (key: string) => string;
  setLocale: (locale: SupportedLocale) => void;
};

const LocalizationContext = createContext<LocalizationContextValue | undefined>(undefined);

type Props = {
  children: ReactNode;
};

const STORAGE_KEY = 'app-locale';

export const LocalizationProvider: React.FC<Props> = ({ children }) => {
  const [locale, setLocaleState] = useState<SupportedLocale>(defaultLocale);

  useEffect(() => {
    const stored = localStorage.getItem(STORAGE_KEY) as SupportedLocale | null;
    if (stored && resources[stored]) {
      setLocaleState(stored);
    }
  }, []);

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, locale);
  }, [locale]);

  const t = useMemo(() => {
    const dictionary = resources[locale];
    return (key: string) => dictionary[key] ?? key;
  }, [locale]);

  const setLocale = (newLocale: SupportedLocale) => {
    if (resources[newLocale]) {
      setLocaleState(newLocale);
    }
  };

  const value: LocalizationContextValue = { locale, t, setLocale };

  return <LocalizationContext.Provider value={value}>{children}</LocalizationContext.Provider>;
};

export const useLocalization = (): LocalizationContextValue => {
  const ctx = useContext(LocalizationContext);
  if (!ctx) {
    throw new Error('useLocalization must be used within LocalizationProvider');
  }
  return ctx;
};
