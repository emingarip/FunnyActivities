import React, { createContext, useContext, useReducer, useEffect, useCallback, ReactNode } from 'react';
import { setLogoutCallback } from '../services/api';
// Logging helper function
const logAuthEvent = (level: 'log' | 'warn' | 'error', message: string, details?: any) => {
  const timestamp = new Date().toISOString();
  const logMessage = `[${timestamp}] AuthContext: ${message}`;
  if (details) {
    if (level === 'error') {
      console.error(logMessage, details);
    } else if (level === 'warn') {
      console.warn(logMessage, details);
    } else {
      console.log(logMessage, details);
    }
  } else {
    if (level === 'error') {
      console.error(logMessage);
    } else if (level === 'warn') {
      console.warn(logMessage);
    } else {
      console.log(logMessage);
    }
  }
};

// User interface
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  createdAt: string;
  updatedAt: string;
}

// Auth state interface
export interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

// Auth actions
export type AuthAction =
  | { type: 'AUTH_START' }
  | { type: 'AUTH_SUCCESS'; payload: User }
  | { type: 'AUTH_ERROR'; payload: string }
  | { type: 'AUTH_LOGOUT' }
  | { type: 'CLEAR_ERROR' };

// Auth context interface
export interface AuthContextType extends AuthState {
  login: (email: string, password: string) => Promise<void>;
  register: (data: RegisterData) => Promise<void>;
  logout: () => void;
  clearError: () => void;
}

// Register data interface
export interface RegisterData {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

// Auth reducer
const authReducer = (state: AuthState, action: AuthAction): AuthState => {
  logAuthEvent('log', `Reducer action: ${action.type}`, {
    previousState: { isAuthenticated: state.isAuthenticated, isLoading: state.isLoading, error: state.error, userId: state.user?.id },
    action
  });

  switch (action.type) {
    case 'AUTH_START':
      const startState = {
        ...state,
        isLoading: true,
        error: null,
      };
      logAuthEvent('log', 'Authentication started - loading state set', { isLoading: startState.isLoading });
      return startState;
    case 'AUTH_SUCCESS':
      const successState = {
        ...state,
        user: action.payload,
        isAuthenticated: true,
        isLoading: false,
        error: null,
      };
      logAuthEvent('log', 'Authentication successful', { userId: action.payload.id, email: action.payload.email, isAuthenticated: successState.isAuthenticated });
      return successState;
    case 'AUTH_ERROR':
      const errorState = {
        ...state,
        user: null,
        isAuthenticated: false,
        isLoading: false,
        error: action.payload,
      };
      logAuthEvent('error', 'Authentication error', { error: action.payload, isAuthenticated: errorState.isAuthenticated });
      return errorState;
    case 'AUTH_LOGOUT':
      const logoutState = {
        ...state,
        user: null,
        isAuthenticated: false,
        isLoading: false,
        error: null,
      };
      logAuthEvent('log', 'User logged out', { previousUserId: state.user?.id, isAuthenticated: logoutState.isAuthenticated });
      return logoutState;
    case 'CLEAR_ERROR':
      const clearState = {
        ...state,
        error: null,
      };
      logAuthEvent('log', 'Error cleared', { previousError: state.error });
      return clearState;
    default:
      logAuthEvent('warn', `Unknown action type: ${(action as any).type}`);
      return state;
  }
};

// Initial state
const initialState: AuthState = {
  user: null,
  isAuthenticated: false,
  isLoading: true, // Start with loading to check for existing tokens
  error: null,
};

// Create context
const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Auth provider props
interface AuthProviderProps {
  children: ReactNode;
}

// Auth provider component
export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [state, dispatch] = useReducer(authReducer, initialState);

  // Check for existing tokens on mount
    useEffect(() => {
      logAuthEvent('log', 'Checking authentication status on mount');
      const checkAuthStatus = async () => {
        const token = localStorage.getItem('accessToken');
        if (token) {
          logAuthEvent('log', 'Access token found in localStorage', { tokenPresent: true });
          try {
            // Import authAPI here to avoid circular dependencies
            const { userAPI } = await import('../services/api');
            logAuthEvent('log', 'Fetching user profile from API');

            // Create timeout promise for authentication verification
            const timeoutPromise = new Promise<never>((_, reject) => {
              setTimeout(() => {
                logAuthEvent('error', 'Authentication verification timeout after 10 seconds');
                reject(new Error('Authentication verification timeout'));
              }, 10000);
            });

            // Race between profile fetch and timeout
            const response = await Promise.race([userAPI.getProfile(), timeoutPromise]);
            const responseData = response.data;
            const user = responseData.data || responseData;
            console.log('[AuthContext] User profile received from API:', {
              userId: user.id,
              email: user.email,
              role: user.role,
              roleType: typeof user.role,
              fullUser: user
            });
            logAuthEvent('log', 'User profile fetched successfully', { userId: user.id, email: user.email, role: user.role });
            dispatch({ type: 'AUTH_SUCCESS', payload: user });
          } catch (error: any) {
            logAuthEvent('error', 'Token validation failed or timed out', { error: error.message, tokenInvalid: true });
            // Token is invalid or verification failed/hung, clear it
            localStorage.removeItem('accessToken');
            localStorage.removeItem('refreshToken');
            logAuthEvent('log', 'Invalid tokens cleared from localStorage due to verification failure');
            dispatch({ type: 'AUTH_LOGOUT' });
            // Immediate redirection to login page on hang-up or failure
            window.location.assign('/login');
          }
        } else {
          logAuthEvent('log', 'No access token found in localStorage', { tokenPresent: false });
          dispatch({ type: 'AUTH_LOGOUT' });
        }
      };
  
      checkAuthStatus();
    }, []);

  // Login function
    const login = useCallback(async (email: string, password: string): Promise<void> => {
      logAuthEvent('log', 'Login attempt started', { email, requestId: Date.now() });
      dispatch({ type: 'AUTH_START' });
      try {
        const { authAPI } = await import('../services/api');
        logAuthEvent('log', 'Making login API call', { email, apiEndpoint: 'authAPI.login' });
        const response = await authAPI.login({ email, password });
  
        // Handle new response format from AuthController
        const responseData = response.data;
        if (responseData.success && responseData.data) {
          const { user, accessToken, refreshToken } = responseData.data;

          console.log('[AuthContext] Login response user data:', {
            userId: user.id,
            email: user.email,
            role: user.role,
            roleType: typeof user.role,
            fullUser: user
          });

          // Store tokens
          localStorage.setItem('accessToken', accessToken);
          localStorage.setItem('refreshToken', refreshToken);
          logAuthEvent('log', 'Tokens stored in localStorage', { userId: user.id, email: user.email });

          dispatch({ type: 'AUTH_SUCCESS', payload: user });
          logAuthEvent('log', 'Login successful', { userId: user.id, email: user.email, role: user.role });
        } else {
          logAuthEvent('warn', 'Login API response indicates failure', { responseData });
          throw new Error(responseData.message || 'Login failed');
        }
      } catch (error: any) {
        let errorMessage = 'Login failed';
        let errorDetails = {};
  
        // Handle different error types
        if (error.response?.data) {
          const errorData = error.response.data;
          errorDetails = { responseData: errorData, statusCode: error.response.status };
  
          // Handle new error format from AuthController
          if (errorData.success === false) {
            errorMessage = errorData.message || 'Login failed';
          } else if (errorData.message) {
            errorMessage = errorData.message;
          }
        } else if (error.message) {
          errorMessage = error.message;
          errorDetails = { errorMessage: error.message };
        }
  
        // Handle specific error types
        if (error.type) {
          switch (error.type) {
            case 'Unauthorized':
              errorMessage = 'Invalid email or password';
              break;
            case 'BadRequest':
              errorMessage = 'Please check your input and try again';
              break;
            case 'NetworkError':
              errorMessage = 'Network error, please check your connection';
              break;
            case 'RateLimitExceeded':
              errorMessage = 'Too many login attempts, please try again later';
              break;
            default:
              break;
          }
          errorDetails = { ...errorDetails, errorType: error.type };
        }
  
        logAuthEvent('error', 'Login failed', { email, errorMessage, errorDetails });
        dispatch({ type: 'AUTH_ERROR', payload: errorMessage });
        throw error;
      }
    }, []);

  // Register function
    const register = useCallback(async (data: RegisterData): Promise<void> => {
      logAuthEvent('log', 'Registration attempt started', { email: data.email, firstName: data.firstName, lastName: data.lastName, requestId: Date.now() });
      dispatch({ type: 'AUTH_START' });
      try {
        const { authAPI } = await import('../services/api');
        logAuthEvent('log', 'Making registration API call', { email: data.email, apiEndpoint: 'authAPI.register' });
        const response = await authAPI.register(data);
  
        // Handle new response format from AuthController
        const responseData = response.data;
        if (responseData.success && responseData.data) {
          const { user, accessToken, refreshToken } = responseData.data;
  
          // Store tokens
          localStorage.setItem('accessToken', accessToken);
          localStorage.setItem('refreshToken', refreshToken);
          logAuthEvent('log', 'Tokens stored in localStorage after registration', { userId: user.id, email: user.email });
  
          dispatch({ type: 'AUTH_SUCCESS', payload: user });
          logAuthEvent('log', 'Registration successful', { userId: user.id, email: user.email });
        } else {
          logAuthEvent('warn', 'Registration API response indicates failure', { responseData });
          throw new Error(responseData.message || 'Registration failed');
        }
      } catch (error: any) {
        let errorMessage = 'Registration failed';
        let errorDetails = {};
  
        // Handle different error types
        if (error.response?.data) {
          const errorData = error.response.data;
          errorDetails = { responseData: errorData, statusCode: error.response.status };
  
          // Handle new error format from AuthController
          if (errorData.success === false) {
            errorMessage = errorData.message || 'Registration failed';
          } else if (errorData.message) {
            errorMessage = errorData.message;
          }
        } else if (error.message) {
          errorMessage = error.message;
          errorDetails = { errorMessage: error.message };
        }
  
        // Handle specific error types
        if (error.type) {
          switch (error.type) {
            case 'BadRequest':
              errorMessage = 'Please check your input and try again';
              break;
            case 'Conflict':
              errorMessage = 'An account with this email already exists';
              break;
            case 'NetworkError':
              errorMessage = 'Network error, please check your connection';
              break;
            case 'RateLimitExceeded':
              errorMessage = 'Too many registration attempts, please try again later';
              break;
            default:
              break;
          }
          errorDetails = { ...errorDetails, errorType: error.type };
        }
  
        logAuthEvent('error', 'Registration failed', { email: data.email, errorMessage, errorDetails });
        dispatch({ type: 'AUTH_ERROR', payload: errorMessage });
        throw error;
      }
    }, []);

  // Logout function
    const logout = useCallback((): void => {
      logAuthEvent('log', 'Logout initiated', { userId: state.user?.id, email: state.user?.email });
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      logAuthEvent('log', 'Tokens removed from localStorage');
      dispatch({ type: 'AUTH_LOGOUT' });
    }, []);

  // Set logout callback for API service
  useEffect(() => {
    setLogoutCallback(logout);
  }, [logout]);

  // Clear error function
    const clearError = useCallback((): void => {
      logAuthEvent('log', 'Clearing authentication error', { previousError: state.error });
      dispatch({ type: 'CLEAR_ERROR' });
    }, []);

  const value: AuthContextType = {
    ...state,
    login,
    register,
    logout,
    clearError,
  };

  logAuthEvent('log', 'AuthContext value updated', {
    isAuthenticated: value.isAuthenticated,
    isLoading: value.isLoading,
    userId: value.user?.id,
    error: value.error
  });

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

// Custom hook to use auth context
export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export default AuthContext;
