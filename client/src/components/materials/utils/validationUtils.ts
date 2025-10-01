/**
 * Validation and sanitization utilities for materials components
 */

/**
 * Sanitizes search terms by trimming whitespace and removing dangerous characters
 * @param term - The search term to sanitize
 * @returns Sanitized search term
 */
export const sanitizeSearchTerm = (term: string): string => {
  return term.trim().replace(/[<>]/g, '').substring(0, 100);
};

/**
 * Sanitizes numeric values with optional min/max constraints
 * @param value - The string value to sanitize
 * @param min - Optional minimum value
 * @param max - Optional maximum value
 * @returns Sanitized number or undefined if invalid
 */
export const sanitizeNumber = (value: string, min?: number, max?: number): number | undefined => {
  const num = parseFloat(value);
  if (isNaN(num)) return undefined;
  if (min !== undefined && num < min) return min;
  if (max !== undefined && num > max) return max;
  return num;
};

/**
 * Sanitizes date strings and ensures valid format
 * @param dateStr - The date string to sanitize
 * @returns Sanitized date string in YYYY-MM-DD format or undefined if invalid
 */
export const sanitizeDate = (dateStr: string): string | undefined => {
  const date = new Date(dateStr);
  if (isNaN(date.getTime())) return undefined;
  return date.toISOString().split('T')[0];
};

/**
 * Validates filter values and sanitizes them
 * @param filters - Partial filter values to validate
 * @returns Validated and sanitized filter values
 */
export const validateFilters = (filters: Partial<any>): Partial<any> => {
  const validated: Partial<any> = {};

  if (filters.search) {
    validated.search = sanitizeSearchTerm(filters.search);
  }

  if (filters.category) {
    validated.category = filters.category.trim().substring(0, 50);
  }

  if (filters.minStock !== undefined) {
    validated.minStock = sanitizeNumber(filters.minStock, 0)?.toString();
  }

  if (filters.maxStock !== undefined) {
    validated.maxStock = sanitizeNumber(filters.maxStock, 0)?.toString();
  }

  if (filters.unit) {
    validated.unit = filters.unit.trim().substring(0, 20);
  }

  if (filters.createdFrom) {
    validated.createdFrom = sanitizeDate(filters.createdFrom);
  }

  if (filters.createdTo) {
    validated.createdTo = sanitizeDate(filters.createdTo);
  }

  if (filters.updatedFrom) {
    validated.updatedFrom = sanitizeDate(filters.updatedFrom);
  }

  if (filters.updatedTo) {
    validated.updatedTo = sanitizeDate(filters.updatedTo);
  }

  return validated;
};