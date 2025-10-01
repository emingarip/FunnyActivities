import { useQuery } from '@tanstack/react-query';

interface OnlineUsersResponse {
  onlineUsers: number;
}

const apiUrl = process.env.REACT_APP_API_URL || 'http://localhost:8080/api';

export const useOnlineUsers = () => {
  const token = localStorage.getItem('accessToken');

  const { data, isLoading, error, refetch } = useQuery<OnlineUsersResponse>({
    queryKey: ['onlineUsers'],
    queryFn: async () => {
      if (!token) {
        throw new Error('No authentication token found');
      }

      const response = await fetch(`${apiUrl}/users/admin/online-count`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const result = await response.json();
      return result;
    },
    enabled: !!token,
    staleTime: 30000,
    refetchInterval: 30000, // Update every 30 seconds
  });

  return {
    data: data?.onlineUsers || 0,
    isLoading,
    error,
    isMockMode: false,
    refetch,
  };
};