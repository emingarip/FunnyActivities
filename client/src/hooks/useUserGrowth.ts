import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useEffect, useState } from 'react';

interface UserGrowthData {
  date: string;
  count: number;
}

interface UserGrowthResponse {
  data: UserGrowthData[];
}

// Generate initial mock data for last 7 days
const generateMockGrowthData = (): UserGrowthData[] => {
  const today = new Date();
  const data: UserGrowthData[] = [];
  let cumulativeCount = 150; // Starting base

  for (let i = 6; i >= 0; i--) {
    const date = new Date(today);
    date.setDate(today.getDate() - i);
    const dateStr = date.toISOString().split('T')[0];
    const dailyGrowth = Math.floor(Math.random() * 11) + 5; // 5-15 new users
    cumulativeCount += dailyGrowth;
    data.push({ date: dateStr, count: cumulativeCount });
  }
  return data;
};

export const useUserGrowth = () => {
  const queryClient = useQueryClient();
  const [mockData, setMockData] = useState<UserGrowthData[]>(generateMockGrowthData());

  const { data, isLoading, error, refetch } = useQuery<UserGrowthResponse>({
    queryKey: ['userGrowth'],
    queryFn: async () => {
      // Always return mock data - no API call needed
      return { data: mockData };
    },
    enabled: true,
    staleTime: 30000,
    refetchInterval: 60000, // Refetch every minute for growth data
  });

  // Simulate growth for demo - update mock data every minute
  useEffect(() => {
    const interval = setInterval(() => {
      setMockData(prev => {
        const newData = [...prev];
        // Update today's count with +1-3 users
        const today = new Date().toISOString().split('T')[0];
        const todayIndex = newData.findIndex(d => d.date === today);
        if (todayIndex !== -1) {
          newData[todayIndex] = {
            ...newData[todayIndex],
            count: newData[todayIndex].count + Math.floor(Math.random() * 3) + 1
          };
        } else {
          // If today not in data, add it
          const newToday = { date: today, count: (newData[0]?.count || 150) + Math.floor(Math.random() * 3) + 1 };
          newData.unshift(newToday);
          // Keep only last 7 days
          if (newData.length > 7) newData.pop();
        }
        return newData;
      });
      // Invalidate query to trigger re-render
      queryClient.invalidateQueries({ queryKey: ['userGrowth'] });
    }, 60000); // Every minute

    return () => clearInterval(interval);
  }, [queryClient]);

  return {
    data: data?.data || mockData,
    isLoading,
    error,
    isMockMode: true,
    refetch,
  };
};