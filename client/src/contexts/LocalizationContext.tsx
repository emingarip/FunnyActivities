import React, {
  createContext,
  useContext,
  useMemo,
  useState,
  useEffect,
  ReactNode,
  useCallback,
} from 'react';
import {
  resources as baseResources,
  SupportedLocale,
  defaultLocale,
  builtInLocales,
} from '../i18n/resources';

type TranslationMap = Record<SupportedLocale, Record<string, string>>;
type LocaleMeta = {
  flag?: string;
  flagImage?: string;
  label?: string;
};
type LocaleMetaMap = Record<SupportedLocale, LocaleMeta>;

type LocalizationContextValue = {
  locale: SupportedLocale;
  t: (key: string) => string;
  setLocale: (locale: SupportedLocale) => void;
  translations: TranslationMap;
  availableLocales: SupportedLocale[];
  updateTranslation: (locale: SupportedLocale, key: string, value: string) => void;
  addLocale: (locale: SupportedLocale, seedFrom?: SupportedLocale) => void;
  removeLocale: (locale: SupportedLocale) => void;
  localeMeta: LocaleMetaMap;
  updateLocaleMeta: (locale: SupportedLocale, meta: LocaleMeta) => void;
};

const LocalizationContext = createContext<LocalizationContextValue | undefined>(undefined);

type Props = {
  children: ReactNode;
};

const STORAGE_KEY = 'app-locale';
const CUSTOM_TRANSLATIONS_KEY = 'custom-translations';
const LOCALE_META_KEY = 'locale-meta';

const mergeTranslations = (base: TranslationMap, custom: TranslationMap): TranslationMap => {
  const merged: TranslationMap = {};
  const locales = new Set([...Object.keys(base), ...Object.keys(custom)]);
  locales.forEach((locale) => {
    merged[locale] = { ...(base[locale] || {}), ...(custom[locale] || {}) };
  });
  return merged;
};

const loadCustomTranslations = (): TranslationMap => {
  const stored = localStorage.getItem(CUSTOM_TRANSLATIONS_KEY);
  if (!stored) return {};
  try {
    const parsed = JSON.parse(stored);
    if (parsed && typeof parsed === 'object') {
      return parsed as TranslationMap;
    }
  } catch (error) {
    console.error('Failed to parse stored translations', error);
  }
  return {};
};

const loadLocaleMeta = (): LocaleMetaMap => {
  const stored = localStorage.getItem(LOCALE_META_KEY);
  if (!stored) return {};
  try {
    const parsed = JSON.parse(stored);
    if (parsed && typeof parsed === 'object') {
      return parsed as LocaleMetaMap;
    }
  } catch (error) {
    console.error('Failed to parse stored locale meta', error);
  }
  return {};
};

export const LocalizationProvider: React.FC<Props> = ({ children }) => {
  const [locale, setLocaleState] = useState<SupportedLocale>(defaultLocale);
  const [customTranslations, setCustomTranslations] = useState<TranslationMap>({});
  const [localeMeta, setLocaleMeta] = useState<LocaleMetaMap>({});

  useEffect(() => {
    const storedLocale = localStorage.getItem(STORAGE_KEY) as SupportedLocale | null;
    const storedTranslations = loadCustomTranslations();
    if (storedLocale) {
      setLocaleState(storedLocale);
    }
    if (storedTranslations) {
      setCustomTranslations(storedTranslations);
    }
    const storedMeta = loadLocaleMeta();
    if (storedMeta) {
      setLocaleMeta(storedMeta);
    }
  }, []);

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, locale);
  }, [locale]);

  const translations = useMemo(
    () => mergeTranslations(baseResources, customTranslations),
    [customTranslations]
  );

  const availableLocales = useMemo(
    () => Array.from(new Set(Object.keys(translations))) as SupportedLocale[],
    [translations]
  );

  const persistCustomTranslations = useCallback((entries: TranslationMap) => {
    localStorage.setItem(CUSTOM_TRANSLATIONS_KEY, JSON.stringify(entries));
  }, []);
  const persistLocaleMeta = useCallback((meta: LocaleMetaMap) => {
    localStorage.setItem(LOCALE_META_KEY, JSON.stringify(meta));
  }, []);

  const ensureLocaleEntry = useCallback(
    (targetLocale: SupportedLocale) => {
      if (translations[targetLocale]) return;
      setCustomTranslations((prev) => {
        if (prev[targetLocale]) return prev;
        const next = { ...prev, [targetLocale]: {} };
        persistCustomTranslations(next);
        return next;
      });
    },
    [persistCustomTranslations, translations]
  );

  const updateTranslation = useCallback(
    (targetLocale: SupportedLocale, key: string, value: string) => {
      setCustomTranslations((prev) => {
        const nextLocale = { ...(prev[targetLocale] || {}) };
        nextLocale[key] = value;
        const next = { ...prev, [targetLocale]: nextLocale };
        persistCustomTranslations(next);
        return next;
      });
    },
    [persistCustomTranslations]
  );

  const addLocale = useCallback(
    (newLocale: SupportedLocale, seedFrom?: SupportedLocale) => {
      setCustomTranslations((prev) => {
        if (prev[newLocale] || translations[newLocale]) {
          return prev;
        }
        const seedLocale = seedFrom && translations[seedFrom] ? translations[seedFrom] : undefined;
        const seeded = seedLocale ? { ...seedLocale } : {};
        const next = { ...prev, [newLocale]: seeded };
        persistCustomTranslations(next);
        return next;
      });
      setLocaleState(newLocale);
    },
    [persistCustomTranslations, translations]
  );

  const removeLocale = useCallback(
    (targetLocale: SupportedLocale) => {
      if (builtInLocales.includes(targetLocale)) {
        console.warn(`Cannot remove built-in locale: ${targetLocale}`);
        return;
      }
      setCustomTranslations((prev) => {
        if (!prev[targetLocale]) {
          return prev;
        }
        const next = { ...prev };
        delete next[targetLocale];
        persistCustomTranslations(next);
        return next;
      });
      setLocaleState((current) => (current === targetLocale ? defaultLocale : current));
      setLocaleMeta((prev) => {
        if (!prev[targetLocale]) {
          return prev;
        }
        const next = { ...prev };
        delete next[targetLocale];
        persistLocaleMeta(next);
        return next;
      });
    },
    [persistCustomTranslations, persistLocaleMeta]
  );

  const updateLocaleMeta = useCallback(
    (targetLocale: SupportedLocale, meta: LocaleMeta) => {
      setLocaleMeta((prev) => {
        const nextLocaleMeta = { ...(prev[targetLocale] || {}), ...meta };
        if (!nextLocaleMeta.flag) {
          delete nextLocaleMeta.flag;
        }
        if (!nextLocaleMeta.label) delete nextLocaleMeta.label;
        if (!nextLocaleMeta.flag) delete nextLocaleMeta.flag;
        if (!nextLocaleMeta.flagImage) delete nextLocaleMeta.flagImage;
        const next = { ...prev };
        if (Object.keys(nextLocaleMeta).length === 0) {
          delete next[targetLocale];
        } else {
          next[targetLocale] = nextLocaleMeta;
        }
        persistLocaleMeta(next);
        return next;
      });
    },
    [persistLocaleMeta]
  );

  const t = useCallback(
    (key: string) => {
      const dictionary = translations[locale];
      if (dictionary && dictionary[key] !== undefined) {
        return dictionary[key];
      }
      const fallback = translations[defaultLocale];
      return fallback?.[key] ?? key;
    },
    [locale, translations]
  );

  const setLocale = (newLocale: SupportedLocale) => {
    setLocaleState(newLocale);
    ensureLocaleEntry(newLocale);
  };

  const value: LocalizationContextValue = {
    locale,
    t,
    setLocale,
    translations,
    availableLocales,
    updateTranslation,
    addLocale,
    removeLocale,
    localeMeta,
    updateLocaleMeta,
  };

  return <LocalizationContext.Provider value={value}>{children}</LocalizationContext.Provider>;
};

export const useLocalization = (): LocalizationContextValue => {
  const ctx = useContext(LocalizationContext);
  if (!ctx) {
    throw new Error('useLocalization must be used within LocalizationProvider');
  }
  return ctx;
};
