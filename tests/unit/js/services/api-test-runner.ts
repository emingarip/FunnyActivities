// API Test Runner for Materials Creation
// Run this in browser console or use with Postman to test API payload structures

import { materialsAPI } from '../api';

// Test data
const testData = {
  name: "Copper Wire 14-gauge",
  description: "High-conductivity copper wire for electrical applications",
  stockQuantity: 500.0,
  category: "Electrical",
  unit: "Meters",
  usageNotes: "UL rated, suitable for indoor wiring",
  dynamicProperties: {
    color: "red",
    weight: 5.5
  }
};

// Test function for current payload format
export const testCurrentPayloadFormat = async () => {
  console.log('🧪 Testing CURRENT payload format...');
  console.log('Payload:', JSON.stringify(testData, null, 2));

  try {
    const response = await materialsAPI.createMaterial(testData);
    console.log('✅ SUCCESS - Current format worked');
    console.log('Response:', response.data);
    return { success: true, response: response.data };
  } catch (error: any) {
    console.log('❌ FAILED - Current format error');
    console.log('Error:', error.response?.data || error.message);
    return { success: false, error: error.response?.data || error.message };
  }
};

// Test function for corrected payload format (wrapped in "request")
export const testCorrectedPayloadFormat = async () => {
  console.log('🧪 Testing CORRECTED payload format (wrapped in request)...');
  const correctedPayload = { request: testData };
  console.log('Payload:', JSON.stringify(correctedPayload, null, 2));

  try {
    const response = await materialsAPI.createMaterialCorrected(testData);
    console.log('✅ SUCCESS - Corrected format worked');
    console.log('Response:', response.data);
    return { success: true, response: response.data };
  } catch (error: any) {
    console.log('❌ FAILED - Corrected format error');
    console.log('Error:', error.response?.data || error.message);
    return { success: false, error: error.response?.data || error.message };
  }
};

// Comprehensive test runner
export const runApiPayloadTests = async () => {
  console.log('🚀 Starting API Payload Structure Tests');
  console.log('=' .repeat(50));

  // Test current format
  const currentResult = await testCurrentPayloadFormat();
  console.log('');

  // Test corrected format
  const correctedResult = await testCorrectedPayloadFormat();
  console.log('');

  // Summary
  console.log('📊 Test Summary:');
  console.log(`Current Format: ${currentResult.success ? '✅ PASS' : '❌ FAIL'}`);
  console.log(`Corrected Format: ${correctedResult.success ? '✅ PASS' : '❌ FAIL'}`);

  if (currentResult.success && !correctedResult.success) {
    console.log('💡 Recommendation: Current format is working, no changes needed');
  } else if (!currentResult.success && correctedResult.success) {
    console.log('💡 Recommendation: Use corrected format (wrapped in request)');
  } else if (currentResult.success && correctedResult.success) {
    console.log('💡 Both formats work - check which one is preferred by backend');
  } else {
    console.log('❌ Both formats failed - check API configuration and authentication');
  }

  return {
    current: currentResult,
    corrected: correctedResult
  };
};

// Unit field validation test
export const testUnitFieldValidation = () => {
  console.log('🔍 Testing Unit Field Validation');

  const testUnits = [
    "Meters",
    "kg",
    "pieces",
    "liters",
    "Invalid Unit 123!@#",
    "",
    "a".repeat(25), // Too long
    "123startsWithNumber"
  ];

  testUnits.forEach(unit => {
    const isValid = /^[a-zA-Z][a-zA-Z0-9\s]*$/.test(unit) && unit.length <= 20 && unit.length > 0;
    console.log(`Unit: "${unit}" -> ${isValid ? '✅ Valid' : '❌ Invalid'}`);
  });
};

// Export for browser console usage
if (typeof window !== 'undefined') {
  (window as any).runApiPayloadTests = runApiPayloadTests;
  (window as any).testCurrentPayloadFormat = testCurrentPayloadFormat;
  (window as any).testCorrectedPayloadFormat = testCorrectedPayloadFormat;
  (window as any).testUnitFieldValidation = testUnitFieldValidation;

  console.log('🎯 API Test functions loaded!');
  console.log('Run these commands in browser console:');
  console.log('- runApiPayloadTests() // Run all tests');
  console.log('- testCurrentPayloadFormat() // Test current format only');
  console.log('- testCorrectedPayloadFormat() // Test corrected format only');
  console.log('- testUnitFieldValidation() // Test unit field validation');
}