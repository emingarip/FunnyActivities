/**
 * Formatting utilities for materials components
 */

/**
 * Formats stock quantity based on unit type
 * @param quantity - The numeric quantity
 * @param unit - The unit string
 * @returns Formatted quantity string
 */
export const formatStockQuantity = (quantity: number, unit: string): string => {
  // Handle cases where unit is not a string (null, undefined, etc.)
  if (typeof unit !== 'string' || !unit) {
    return quantity.toString();
  }

  // Format based on unit type
  const unitLower = unit.toLowerCase();
  if (unitLower.includes('pieces') || unitLower.includes('pcs')) {
    return Math.floor(quantity).toString();
  } else if (unitLower.includes('kg') || unitLower.includes('kilograms') ||
             unitLower.includes('liters') || unitLower.includes('l') ||
             unitLower.includes('meters') || unitLower.includes('m') ||
             unitLower.includes('square meters') || unitLower.includes('m²')) {
    return quantity.toFixed(2);
  } else {
    return quantity.toString();
  }
};

/**
 * Gets display name for unit abbreviations
 * @param unit - The unit string
 * @returns Display name for the unit
 */
export const getUnitDisplayName = (unit: string): string => {
  // Handle cases where unit is not a string (null, undefined, etc.)
  if (typeof unit !== 'string' || !unit) {
    return 'N/A'; // Return a default value for non-string units
  }

  const unitLower = unit.toLowerCase();
  if (unitLower.includes('pieces') || unitLower.includes('pcs')) return 'pcs';
  if (unitLower.includes('kilograms') || unitLower.includes('kg')) return 'kg';
  if (unitLower.includes('liters') || unitLower.includes('l')) return 'L';
  if (unitLower.includes('meters') || unitLower.includes('m')) return 'm';
  if (unitLower.includes('square meters') || unitLower.includes('m²')) return 'm²';
  return unit; // Return the original unit if no match
};

/**
 * Gets stock status based on quantity
 * @param quantity - The stock quantity
 * @returns CSS class name for stock status
 */
export const getStockStatus = (quantity: number): string => {
  if (quantity <= 0) return 'out-of-stock';
  if (quantity < 10) return 'low-stock';
  return 'in-stock';
};

/**
 * Formats currency values
 * @param value - The numeric value
 * @param currency - The currency code (default: USD)
 * @returns Formatted currency string
 */
export const formatCurrency = (value: number, currency: string = 'USD'): string => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency
  }).format(value);
};

/**
 * Formats timestamps for display
 * @param timestamp - ISO timestamp string
 * @param options - Intl.DateTimeFormatOptions
 * @returns Formatted timestamp string
 */
export const formatTimestamp = (
  timestamp: string,
  options: Intl.DateTimeFormatOptions = {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  }
): string => {
  return new Date(timestamp).toLocaleString('en-US', options);
};