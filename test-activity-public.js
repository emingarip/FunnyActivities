// Test script to mark activity as public and test public access
const activityId = 'ec5d4067-18d9-4564-89c7-f2d17604239e';

async function markActivityAsPublic() {
    try {
        console.log('🔄 Attempting to mark activity as public...');

        // For demo purposes, we'll assume the activity exists and try to update it
        // In a real scenario, you would need authentication for this
        const updateResponse = await fetch(`http://localhost:8080/api/activities/${activityId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                // Note: This would require authentication in a real scenario
                // 'Authorization': 'Bearer YOUR_ADMIN_TOKEN_HERE'
            },
            body: JSON.stringify({
                name: 'Sample Public Activity',
                description: 'This is a sample public activity for testing',
                videoUrl: null,
                durationHours: 0,
                durationMinutes: 30,
                durationSeconds: 0,
                isPublic: true
            })
        });

        if (updateResponse.ok) {
            console.log('✅ Activity marked as public successfully');
            return true;
        } else if (updateResponse.status === 401) {
            console.log('❌ Authentication required to update activity');
            console.log('💡 For testing, you can manually mark the activity as public in the database');
            return false;
        } else {
            console.log(`❌ Failed to update activity: ${updateResponse.status}`);
            return false;
        }
    } catch (error) {
        console.error('❌ Error updating activity:', error);
        return false;
    }
}

// Test the public endpoint (no authentication required)
async function testPublicEndpoint() {
    try {
        console.log('🔍 Testing public endpoint access...');
        const response = await fetch(`http://localhost:8080/api/activities/public/${activityId}`);

        console.log(`📊 Public endpoint response status: ${response.status}`);

        if (response.ok) {
            const activity = await response.json();
            console.log('✅ Public activity retrieved successfully!');
            console.log('📋 Activity data:', {
                id: activity.data.id,
                name: activity.data.name,
                isPublic: activity.data.isPublic,
                description: activity.data.description
            });
            return true;
        } else if (response.status === 404) {
            console.log('❌ Activity not found or not marked as public');
            console.log('💡 The activity either does not exist or is not marked as public');
            return false;
        } else {
            console.log(`❌ Unexpected response: ${response.status}`);
            return false;
        }
    } catch (error) {
        console.error('❌ Error testing public endpoint:', error);
        return false;
    }
}

// Test the protected endpoint (should return 401)
async function testProtectedEndpoint() {
    try {
        console.log('🔒 Testing protected endpoint (should require authentication)...');
        const response = await fetch(`http://localhost:8080/api/activities/${activityId}`);

        console.log(`📊 Protected endpoint response status: ${response.status}`);

        if (response.status === 401) {
            console.log('✅ Protected endpoint correctly requires authentication');
            return true;
        } else {
            console.log(`❌ Unexpected response from protected endpoint: ${response.status}`);
            return false;
        }
    } catch (error) {
        console.error('❌ Error testing protected endpoint:', error);
        return false;
    }
}

async function runTests() {
    console.log('🚀 Starting activity access tests...\n');

    // Test 1: Check if protected endpoint requires authentication
    console.log('=== TEST 1: Protected Endpoint ===');
    await testProtectedEndpoint();

    console.log('\n=== TEST 2: Public Endpoint ===');
    const publicTestPassed = await testPublicEndpoint();

    if (!publicTestPassed) {
        console.log('\n=== TEST 3: Mark Activity as Public ===');
        const markedAsPublic = await markActivityAsPublic();

        if (markedAsPublic) {
            console.log('\n=== TEST 4: Re-test Public Endpoint ===');
            await testPublicEndpoint();
        }
    }

    console.log('\n✨ Tests completed!');
    console.log('\n📝 Summary:');
    console.log('- Protected endpoint: /api/activities/{id} (requires authentication)');
    console.log('- Public endpoint: /api/activities/public/{id} (no authentication required)');
    console.log('- Frontend now automatically uses public endpoint for non-authenticated users');
}

console.log('🔧 Activity Public Access Implementation Complete!');
console.log('📋 Testing the implementation...\n');

// Test the current status
async function testCurrentStatus() {
    console.log('🔍 Testing current implementation status...\n');

    // Test 1: Public endpoint (should work)
    console.log('=== TEST 1: Public Endpoint ===');
    const publicTestPassed = await testPublicEndpoint();

    // Test 2: Protected endpoint (should return 401/500 as expected)
    console.log('\n=== TEST 2: Protected Endpoint ===');
    await testProtectedEndpoint();

    console.log('\n✅ IMPLEMENTATION STATUS:');
    if (publicTestPassed) {
        console.log('🎉 SUCCESS: Public activity access is working!');
        console.log('📝 Frontend will now automatically use the public endpoint for non-authenticated users');
        console.log('🔗 Public URL: /api/activities/public/{id}');
        console.log('🔒 Protected URL: /api/activities/{id} (requires authentication)');
    } else {
        console.log('⚠️  ISSUE: Public endpoint not working as expected');
    }
}

console.log('🚀 Starting comprehensive tests...\n');
testCurrentStatus();