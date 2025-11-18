import { useQuery } from '@tanstack/react-query';

interface UserCountResponse {
  totalUsers: number;
}

export const useUserCount = () => {
  const token = localStorage.getItem('accessToken');
  const apiUrl = process.env.REACT_APP_API_URL || 'http://localhost:8080/api';

  return useQuery<UserCountResponse>({
    queryKey: ['userCount'],
    queryFn: async () => {
      if (!token) {
        throw new Error('No authentication token found');
      }

      const response = await fetch(`${apiUrl}/users/admin/count`, {
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
      return result.data || result;
    },
    enabled: !!token,
    staleTime: 30000,
    refetchInterval: 30000,
  });
};