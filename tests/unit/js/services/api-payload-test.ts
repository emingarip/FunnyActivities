// API Payload Structure Test for Materials Creation
// This file demonstrates the correct payload structure for testing the API

export interface CorrectedCreateMaterialPayload {
  request: {
    name: string;
    description?: string;
    stockQuantity: number;
    category?: string;
    unit: string; // Properly formatted as string, trimmed, validated
    usageNotes?: string;
    dynamicProperties?: Record<string, any>;
    photos?: string[];
  };
}

// Legacy implementation payload (deprecated)
export interface LegacyCreateMaterialPayload {
  name: string;
  description?: string;
  stockQuantity: number;
  category?: string;
  unit: string;
  usageNotes?: string;
  dynamicProperties?: Record<string, any>;
  photos?: string[];
}

// Current implementation payload (what the client currently sends)
export interface CurrentCreateMaterialPayload {
  request: {
    name: string;
    description?: string;
    stockQuantity: number;
    category?: string;
    unit: string;
    usageNotes?: string;
    dynamicProperties?: Record<string, any>;
    photos?: string[];
  };
}

// Test data for API testing
export const testMaterialData = {
  name: "Copper Wire 14-gauge",
  description: "High-conductivity copper wire for electrical applications",
  stockQuantity: 500.0,
  category: "Electrical",
  unit: "Meters", // Properly formatted unit
  usageNotes: "UL rated, suitable for indoor wiring",
  dynamicProperties: {
    color: "red",
    weight: 5.5
  }
};

// Corrected payload structure (wrapped in "request")
export const getCorrectedPayload = (data: typeof testMaterialData): CorrectedCreateMaterialPayload => ({
  request: {
    ...data,
    unit: String(data.unit).trim(), // Ensure unit is properly formatted
    stockQuantity: Number(data.stockQuantity) // Ensure stockQuantity is a number
  }
});

// Current payload structure (wrapped in request)
export const getCurrentPayload = (data: typeof testMaterialData): CurrentCreateMaterialPayload => ({
  request: {
    ...data,
    unit: String(data.unit).trim(),
    stockQuantity: Number(data.stockQuantity)
  }
});

// Postman/Browser Dev Tools test script
export const generatePostmanTest = () => {
  const correctedPayload = getCorrectedPayload(testMaterialData);
  const currentPayload = getCurrentPayload(testMaterialData);

  return {
    correctedFormat: {
      method: 'POST',
      url: 'http://localhost:8080/api/materials',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer YOUR_JWT_TOKEN_HERE'
      },
      body: JSON.stringify(correctedPayload, null, 2)
    },
    currentFormat: {
      method: 'POST',
      url: 'http://localhost:8080/api/materials',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer YOUR_JWT_TOKEN_HERE'
      },
      body: JSON.stringify(currentPayload, null, 2)
    }
  };
};

// Unit field validation helper
export const validateUnitField = (unit: any): { isValid: boolean; formatted: string; errors: string[] } => {
  const errors: string[] = [];

  if (!unit) {
    errors.push('Unit is required');
    return { isValid: false, formatted: '', errors };
  }

  const formatted = String(unit).trim();

  if (formatted.length === 0) {
    errors.push('Unit cannot be empty');
  } else if (formatted.length > 20) {
    errors.push('Unit must be 20 characters or less');
  } else if (!/^[a-zA-Z][a-zA-Z0-9\s]*$/.test(formatted)) {
    errors.push('Unit must start with a letter and contain only letters, numbers, and spaces');
  }

  return {
    isValid: errors.length === 0,
    formatted,
    errors
  };
};

// Console logging for testing
export const logPayloadComparison = () => {
  const corrected = getCorrectedPayload(testMaterialData);
  const current = getCurrentPayload(testMaterialData);

  console.log('=== API Payload Structure Test ===');
  console.log('Test Data:', testMaterialData);
  console.log('');

  console.log('Current Payload (what client sends):');
  console.log(JSON.stringify(current, null, 2));
  console.log('');

  console.log('Corrected Payload (wrapped in request):');
  console.log(JSON.stringify(corrected, null, 2));
  console.log('');

  console.log('Unit Field Validation:');
  const unitValidation = validateUnitField(testMaterialData.unit);
  console.log('Unit:', testMaterialData.unit);
  console.log('Is Valid:', unitValidation.isValid);
  console.log('Formatted:', unitValidation.formatted);
  if (unitValidation.errors.length > 0) {
    console.log('Errors:', unitValidation.errors);
  }
};