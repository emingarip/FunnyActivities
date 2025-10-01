# Authentication Context

This directory contains the authentication context and provider for managing user authentication state throughout the React application.

## Features

- JWT token management
- Login/logout functionality
- User state persistence
- Automatic token refresh
- TypeScript support
- Error handling
- Loading states

## Files

- `AuthContext.tsx` - Main authentication context with provider and hooks

## Usage

### 1. Wrap your app with AuthProvider

```tsx
import { AuthProvider } from './contexts/AuthContext';

function App() {
  return (
    <AuthProvider>
      {/* Your app components */}
    </AuthProvider>
  );
}
```

### 2. Use the useAuth hook in components

```tsx
import { useAuth } from '../contexts/AuthContext';

function MyComponent() {
  const { user, login, logout, isAuthenticated, isLoading, error } = useAuth();

  // Use authentication state and methods
  if (isLoading) return <div>Loading...</div>;

  if (!isAuthenticated) {
    return <LoginForm />;
  }

  return (
    <div>
      <h1>Welcome, {user?.firstName}!</h1>
      <button onClick={logout}>Logout</button>
    </div>
  );
}
```

### 3. Authentication Methods

#### Login
```tsx
const handleLogin = async () => {
  try {
    await login(email, password);
    // Redirect to dashboard or home
  } catch (error) {
    // Handle login error
  }
};
```

#### Register
```tsx
const handleRegister = async () => {
  try {
    await register({
      email,
      password,
      firstName,
      lastName,
      role: 'user' // optional
    });
    // Redirect to dashboard
  } catch (error) {
    // Handle registration error
  }
};
```

#### Logout
```tsx
const handleLogout = () => {
  logout();
  // Redirect to login page
};
```

### 4. Authentication State

The `useAuth` hook provides:

- `user`: Current user object or null
- `isAuthenticated`: Boolean indicating if user is logged in
- `isLoading`: Boolean indicating if auth operation is in progress
- `error`: Error message string or null
- `login`: Function to log in user
- `register`: Function to register new user
- `logout`: Function to log out user
- `clearError`: Function to clear error state

### 5. User Object Structure

```tsx
interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}
```

### 6. Protected Routes

You can create protected route components:

```tsx
import { useAuth } from '../contexts/AuthContext';
import { Navigate } from 'react-router-dom';

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) return <div>Loading...</div>;

  return isAuthenticated ? <>{children}</> : <Navigate to="/login" />;
}

// Usage
<Route path="/dashboard" element={
  <ProtectedRoute>
    <Dashboard />
  </ProtectedRoute>
} />
```

### 7. API Integration

The authentication context automatically handles:

- Adding auth tokens to API requests
- Token refresh on 401 responses
- Clearing tokens on logout
- Redirecting to login on auth failure

The API service (`services/api.ts`) is already configured to work with this context.

## Configuration

### Environment Variables

Make sure to set the API URL:

```env
REACT_APP_API_URL=https://your-api-url.com/api
```

### Token Storage

Tokens are stored in localStorage:
- `accessToken`: JWT access token
- `refreshToken`: JWT refresh token

## Error Handling

The context provides comprehensive error handling:

- Login/registration errors
- Token refresh failures
- Network errors
- Invalid token errors

Always check the `error` state and display appropriate messages to users.

## Security Notes

- Tokens are stored in localStorage (consider using httpOnly cookies for production)
- Automatic token refresh helps maintain session
- Failed refresh attempts trigger logout
- All API calls include authentication headers when tokens are present