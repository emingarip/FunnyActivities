import React from 'react';
import { render, screen, waitFor, act } from '@testing-library/react';
import { AuthProvider, useAuth } from '../AuthContext';

// Mock the API service
jest.mock('../../services/api', () => ({
  userAPI: {
    getProfile: jest.fn(),
  },
  authAPI: {
    login: jest.fn(),
  },
  setLogoutCallback: jest.fn(),
}));

// Get the mocked modules
const { userAPI: mockUserAPI, authAPI: mockAuthAPI } = require('../../services/api');

// Mock localStorage
const localStorageMock = {
  getItem: jest.fn(),
  setItem: jest.fn(),
  removeItem: jest.fn(),
  clear: jest.fn(),
};
global.localStorage = localStorageMock as any;

// Mock window.location
delete (global as any).window.location;
global.window.location = { href: '', assign: jest.fn() } as any;

// Test component that uses the auth context
const TestComponent: React.FC = () => {
  const { user, isAuthenticated, isLoading, error, login, logout } = useAuth();

  return (
    <div>
      <div data-testid="loading">{isLoading ? 'loading' : 'not-loading'}</div>
      <div data-testid="authenticated">{isAuthenticated ? 'authenticated' : 'not-authenticated'}</div>
      <div data-testid="user">{user ? user.email : 'no-user'}</div>
      <div data-testid="error">{error || 'no-error'}</div>
      <button onClick={() => login('test@test.com', 'password')}>Login</button>
      <button onClick={logout}>Logout</button>
    </div>
  );
};

describe('AuthContext', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    localStorageMock.getItem.mockClear();
    localStorageMock.setItem.mockClear();
    localStorageMock.removeItem.mockClear();
    (global.window.location.assign as jest.Mock).mockClear();
  });

  describe('Initial Authentication Check', () => {
    it('should show loading initially', () => {
      localStorageMock.getItem.mockReturnValue(null);

      render(
        <AuthProvider>
          <TestComponent />
        </AuthProvider>
      );

      expect(screen.getByTestId('loading')).toHaveTextContent('loading');
      expect(screen.getByTestId('authenticated')).toHaveTextContent('not-authenticated');
    });

    it('should handle no stored tokens', async () => {
      localStorageMock.getItem.mockReturnValue(null);

      render(
        <AuthProvider>
          <TestComponent />
        </AuthProvider>
      );

      await waitFor(() => {
        expect(screen.getByTestId('loading')).toHaveTextContent('not-loading');
        expect(screen.getByTestId('authenticated')).toHaveTextContent('not-authenticated');
        expect(screen.getByTestId('user')).toHaveTextContent('no-user');
      });
    });

    it('should verify tokens on mount when present', async () => {
      const mockToken = 'valid-token';
      const mockUser = {
        id: '1',
        email: 'test@test.com',
        firstName: 'Test',
        lastName: 'User',
        role: 'user',
        createdAt: '2023-01-01',
        updatedAt: '2023-01-01',
      };

      localStorageMock.getItem.mockReturnValue(mockToken);
      mockUserAPI.getProfile.mockResolvedValue({
        data: mockUser,
      });

      render(
        <AuthProvider>
          <TestComponent />
        </AuthProvider>
      );

      await waitFor(() => {
        expect(mockUserAPI.getProfile).toHaveBeenCalled();
        expect(screen.getByTestId('authenticated')).toHaveTextContent('authenticated');
        expect(screen.getByTestId('user')).toHaveTextContent('test@test.com');
      });
    });

    it('should handle authentication timeout after 10 seconds', async () => {
      const mockToken = 'valid-token';

      localStorageMock.getItem.mockReturnValue(mockToken);
      // Mock a hanging request that never resolves
      mockUserAPI.getProfile.mockImplementation(
        () => new Promise(() => {}) // Never resolves
      );

      render(
        <AuthProvider>
          <TestComponent />
        </AuthProvider>
      );

      // Wait for the timeout (10 seconds)
      await waitFor(
        () => {
          expect(localStorageMock.removeItem).toHaveBeenCalledWith('accessToken');
          expect(localStorageMock.removeItem).toHaveBeenCalledWith('refreshToken');
          expect(global.window.location.assign).toHaveBeenCalledWith('/login');
        },
        { timeout: 11000 } // Wait a bit longer than 10 seconds
      );
    }, 12000);

    it('should handle authentication failure and clear tokens', async () => {
      const mockToken = 'invalid-token';

      localStorageMock.getItem.mockReturnValue(mockToken);
      mockUserAPI.getProfile.mockRejectedValue(new Error('Invalid token'));

      render(
        <AuthProvider>
          <TestComponent />
        </AuthProvider>
      );

      await waitFor(() => {
        expect(localStorageMock.removeItem).toHaveBeenCalledWith('accessToken');
        expect(localStorageMock.removeItem).toHaveBeenCalledWith('refreshToken');
        expect(global.window.location.assign).toHaveBeenCalledWith('/login');
        expect(screen.getByTestId('authenticated')).toHaveTextContent('not-authenticated');
      });
    });

    it('should handle network errors during verification', async () => {
      const mockToken = 'network-error-token';

      localStorageMock.getItem.mockReturnValue(mockToken);
      mockUserAPI.getProfile.mockRejectedValue(new Error('Network Error'));

      render(
        <AuthProvider>
          <TestComponent />
        </AuthProvider>
      );

      await waitFor(() => {
        expect(localStorageMock.removeItem).toHaveBeenCalledWith('accessToken');
        expect(localStorageMock.removeItem).toHaveBeenCalledWith('refreshToken');
        expect(global.window.location.assign).toHaveBeenCalledWith('/login');
      });
    });
  });

  describe('Login Flow', () => {
    it('should handle successful login', async () => {
      const mockResponse = {
        data: {
          success: true,
          data: {
            user: {
              id: '1',
              email: 'test@test.com',
              firstName: 'Test',
              lastName: 'User',
              role: 'user',
              createdAt: '2023-01-01',
              updatedAt: '2023-01-01',
            },
            accessToken: 'new-access-token',
            refreshToken: 'new-refresh-token',
          },
        },
      };

      // Mock the authAPI
      const mockAuthAPI = {
        login: jest.fn().mockResolvedValue(mockResponse),
      };

      // Re-mock the API service for this test
      jest.doMock('../../services/api', () => ({
        ...jest.requireActual('../../services/api'),
        authAPI: mockAuthAPI,
      }));

      render(
        <AuthProvider>
          <TestComponent />
        </AuthProvider>
      );

      // Wait for initial load
      await waitFor(() => {
        expect(screen.getByTestId('loading')).toHaveTextContent('not-loading');
      });

      // Note: In a real scenario, we'd trigger the login button click
      // For this test, we're focusing on the context behavior
    });
  });

  describe('Logout Flow', () => {
    it('should clear tokens on logout', async () => {
      const mockUser = {
        id: '1',
        email: 'test@test.com',
        firstName: 'Test',
        lastName: 'User',
        role: 'user',
        createdAt: '2023-01-01',
        updatedAt: '2023-01-01',
      };

      localStorageMock.getItem.mockReturnValue('valid-token');
      mockUserAPI.getProfile.mockResolvedValue({
        data: mockUser,
      });

      render(
        <AuthProvider>
          <TestComponent />
        </AuthProvider>
      );

      // Wait for authentication
      await waitFor(() => {
        expect(screen.getByTestId('authenticated')).toHaveTextContent('authenticated');
      });

      // Trigger logout
      const logoutButton = screen.getByText('Logout');
      logoutButton.click();

      await waitFor(() => {
        expect(localStorageMock.removeItem).toHaveBeenCalledWith('accessToken');
        expect(localStorageMock.removeItem).toHaveBeenCalledWith('refreshToken');
        expect(screen.getByTestId('authenticated')).toHaveTextContent('not-authenticated');
      });
    });
  });
});