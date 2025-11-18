import React, { useEffect, useRef, useState } from 'react';
import { useTranslation } from '../hooks/useTranslation';

const LanguageSelector: React.FC = () => {
  const { locale, setLocale, t } = useTranslation();
  const [open, setOpen] = useState(false);
  const wrapperRef = useRef<HTMLDivElement | null>(null);

  const flagIcons: Record<'en' | 'tr', React.ReactElement> = {
    en: (
      <svg width="16" height="16" viewBox="0 0 16 16" aria-hidden="true">
        <rect width="16" height="16" fill="#00247d" />
        <path d="M0 6h16v4H0z" fill="#fff" />
        <path d="M6 0h4v16H6z" fill="#fff" />
        <path d="M0 7h16v2H0z" fill="#cf142b" />
        <path d="M7 0h2v16H7z" fill="#cf142b" />
      </svg>
    ),
    tr: (
      <svg width="16" height="16" viewBox="0 0 16 16" aria-hidden="true">
        <rect width="16" height="16" fill="#e30a17" />
        <circle cx="7" cy="8" r="4" fill="#fff" />
        <circle cx="8" cy="8" r="3" fill="#e30a17" />
        <polygon points="11,8 13,9 12,7 13,5 11,6 9,5 10,7 9,9" fill="#fff" />
      </svg>
    ),
  };

  const languages: Array<{ code: 'en' | 'tr'; label: string }> = [
    { code: 'en', label: t('language_en') },
    { code: 'tr', label: t('language_tr') },
  ];

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (wrapperRef.current && !wrapperRef.current.contains(event.target as Node)) {
        setOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  return (
    <div className="language-dropdown-wrapper" ref={wrapperRef}>
      <button
        type="button"
        className="language-selector"
        onClick={() => setOpen((prev) => !prev)}
        aria-haspopup="listbox"
        aria-expanded={open}
      >
        <span className="language-flag" aria-hidden="true">
          {flagIcons[locale]}
        </span>
        <span className="language-label">
          {languages.find((l) => l.code === locale)?.label ?? locale.toUpperCase()}
        </span>
        <span className="language-toggle" aria-hidden="true">▼</span>
        <span className="sr-only">{t('language_select')}</span>
      </button>

      {open && (
        <ul className="language-dropdown-menu" role="listbox">
          {languages.map((lang) => (
            <li key={lang.code}>
              <button
                type="button"
                className={`language-option ${locale === lang.code ? 'selected' : ''}`}
                onClick={() => {
                  setLocale(lang.code);
                  setOpen(false);
                }}
                role="option"
                aria-selected={locale === lang.code}
              >
                <span className="language-flag" aria-hidden="true">{flagIcons[lang.code]}</span>
                <span className="language-label">{lang.label}</span>
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};

export default LanguageSelector;
