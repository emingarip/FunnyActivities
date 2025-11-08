const canUseStorage = (): boolean => {
  return typeof window !== 'undefined' && typeof window.localStorage !== 'undefined';
};

/**
 * Safely read and parse a JSON value from localStorage.
 * Automatically falls back and removes the key if the stored value is invalid.
 */
export const getJsonItem = <T>(key: string, fallback: T): T => {
  if (!canUseStorage()) {
    return fallback;
  }

  const rawValue = window.localStorage.getItem(key);
  if (!rawValue) {
    return fallback;
  }

  try {
    return JSON.parse(rawValue) as T;
  } catch (error) {
    console.warn(`Invalid JSON in localStorage for key "${key}". Resetting to fallback.`, error);
    window.localStorage.removeItem(key);
    return fallback;
  }
};

/**
 * Stringify and persist a value in localStorage with basic error handling.
 */
export const setJsonItem = <T>(key: string, value: T): void => {
  if (!canUseStorage()) {
    return;
  }

  try {
    window.localStorage.setItem(key, JSON.stringify(value));
  } catch (error) {
    console.error(`Failed to persist localStorage key "${key}".`, error);
  }
};
