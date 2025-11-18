import React, { useState, useMemo } from 'react';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import {
  Box,
  Paper,
  Typography,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Button,
  CircularProgress,
  Alert,
  useMediaQuery,
  useTheme,
} from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { adminAPI } from '../../services/api';

interface UserGrowthDataPoint {
  date: string;
  count: number;
}

type UserGrowthResponse = UserGrowthDataPoint[];

type PeriodType = 'weekly' | 'monthly' | 'quarterly';

interface UserGrowthChartProps {
  height?: number;
  className?: string;
}

const UserGrowthChart: React.FC<UserGrowthChartProps> = ({
  height = 400,
  className,
}) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const isSmallMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const isExtraSmall = useMediaQuery(theme.breakpoints.down('xs')); // For very small screens

  const [period, setPeriod] = useState<PeriodType>('weekly');

  const { data, isLoading, error, refetch } = useQuery<UserGrowthResponse>({
    queryKey: ['userGrowth', period, 'v2'],
    queryFn: async () => {
      console.log('Fetching user growth for period:', period);
      const response = await adminAPI.getUserGrowth({ period });
      console.log('User growth response:', response);
      return response.data.data.data;
    },
    staleTime: 5 * 60 * 1000, // 5 minutes
    refetchInterval: 10 * 60 * 1000, // 10 minutes
  });

  const chartData = useMemo(() => {
    console.log('UserGrowthChart data:', data);
    if (!Array.isArray(data)) {
      console.log('data is not array');
      return [];
    }

    // For performance with large datasets, limit to last 100 points
    const limitedData = data.length > 100
      ? data.slice(-100)
      : data;

    console.log('limitedData length:', limitedData.length);
    return limitedData.map(point => ({
      ...point,
      formattedDate: new Date(point.date).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        ...(period === 'quarterly' && { year: '2-digit' }),
      }),
    }));
  }, [data, period]);



  const CustomTooltip = ({ active, payload, label }: any) => {
    if (active && payload && payload.length) {
      const data = payload[0].payload;
      return (
        <Paper
          elevation={3}
          sx={{
            p: isSmallMobile ? 1.5 : 2,
            maxWidth: isSmallMobile ? 200 : isMobile ? 220 : 250,
            fontSize: isSmallMobile ? '0.75rem' : '0.875rem'
          }}
        >
          <Typography
            variant={isSmallMobile ? "body2" : "subtitle2"}
            gutterBottom
            sx={{ fontSize: isSmallMobile ? '0.75rem' : 'inherit' }}
          >
            {new Date(data.date).toLocaleDateString('en-US', {
              weekday: isSmallMobile ? undefined : 'long',
              year: 'numeric',
              month: isSmallMobile ? 'short' : 'long',
              day: 'numeric',
            })}
          </Typography>
          <Typography
            variant="body2"
            color="primary"
            sx={{ fontSize: isSmallMobile ? '0.75rem' : 'inherit' }}
          >
            Total Users: <strong>{data.count.toLocaleString()}</strong>
          </Typography>
          {chartData.length > 1 && (
            <Typography
              variant="caption"
              color="text.secondary"
              sx={{ fontSize: isSmallMobile ? '0.7rem' : 'inherit' }}
            >
              {(() => {
                const currentIndex = chartData.findIndex(d => d.date === data.date);
                if (currentIndex > 0) {
                  const prevCount = chartData[currentIndex - 1].count;
                  const change = data.count - prevCount;
                  const changePercent = ((change / prevCount) * 100).toFixed(1);
                  return `Change: ${change > 0 ? '+' : ''}${change} (${change > 0 ? '+' : ''}${changePercent}%)`;
                }
                return 'First data point';
              })()}
            </Typography>
          )}
        </Paper>
      );
    }
    return null;
  };

  if (isLoading) {
    return (
      <Paper className={className} sx={{ p: isSmallMobile ? 2 : 3, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', height }}>
        <CircularProgress size={isSmallMobile ? 32 : 40} />
        <Typography variant={isSmallMobile ? "caption" : "body2"} sx={{ mt: 2 }}>
          Loading user growth data...
        </Typography>
      </Paper>
    );
  }

  if (error) {
    return (
      <Paper className={className} sx={{ p: isSmallMobile ? 2 : 3 }}>
        <Alert
          severity="error"
          action={
            <Button
              color="inherit"
              size={isSmallMobile ? "small" : "medium"}
              onClick={() => refetch()}
              sx={{ minHeight: isSmallMobile ? 36 : 40, fontSize: isSmallMobile ? '0.75rem' : '0.875rem' }}
            >
              Retry
            </Button>
          }
          sx={{ fontSize: isSmallMobile ? '0.75rem' : '0.875rem' }}
        >
          Failed to load user growth data. Please try again.
        </Alert>
      </Paper>
    );
  }

  return (
    <Paper className={className} sx={{ p: { xs: 1, sm: 2, md: 3 } }}>
      <Box
        sx={{
          display: 'flex',
          flexDirection: { xs: 'column', sm: 'row' },
          justifyContent: 'space-between',
          alignItems: { xs: 'flex-start', sm: 'center' },
          gap: { xs: 1, sm: 2, md: 3 },
          mb: { xs: 1, sm: 2, md: 3 }
        }}
      >
        <Typography
          variant="h6"
          component="h2"
          sx={{
            fontSize: { xs: '1rem', sm: '1.1rem', md: '1.25rem' },
            textAlign: { xs: 'center', sm: 'left' },
            width: { xs: '100%', sm: 'auto' }
          }}
        >
          User Growth ({period.charAt(0).toUpperCase() + period.slice(1)} View)
        </Typography>

        <FormControl
          size="small"
          sx={{
            minWidth: { xs: 80, sm: 100, md: 120 },
            width: { xs: '100%', sm: 'auto' },
            '& .MuiInputBase-root': {
              fontSize: { xs: '0.75rem', sm: '0.875rem', md: '1rem' }
            }
          }}
        >
          <InputLabel sx={{ fontSize: isExtraSmall ? '0.75rem' : isSmallMobile ? '0.875rem' : '1rem' }}>Period</InputLabel>
          <Select
            value={period}
            label="Period"
            onChange={(e) => setPeriod(e.target.value as PeriodType)}
            sx={{ fontSize: isExtraSmall ? '0.75rem' : isSmallMobile ? '0.875rem' : '1rem' }}
          >
            <MenuItem value="weekly" sx={{ fontSize: isExtraSmall ? '0.75rem' : isSmallMobile ? '0.875rem' : '1rem' }}>Weekly</MenuItem>
            <MenuItem value="monthly" sx={{ fontSize: { xs: '0.75rem', sm: '0.875rem', md: '1rem' } }}>Monthly</MenuItem>
            <MenuItem value="quarterly" sx={{ fontSize: { xs: '0.75rem', sm: '0.875rem', md: '1rem' } }}>Quarterly</MenuItem>
          </Select>
        </FormControl>
      </Box>

      {!chartData.length ? (
        <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', height: height - (isSmallMobile ? 80 : 100) }}>
          <Typography
            variant={isSmallMobile ? "body2" : "body1"}
            color="text.secondary"
            sx={{ fontSize: isSmallMobile ? '0.875rem' : '1rem' }}
          >
            No data available for the selected period
          </Typography>
          <Button
            variant="outlined"
            sx={{
              mt: 2,
              fontSize: isSmallMobile ? '0.75rem' : '0.875rem',
              minHeight: isSmallMobile ? 36 : 40,
              padding: isSmallMobile ? '6px 12px' : '8px 16px'
            }}
            onClick={() => refetch()}
          >
            Refresh Data
          </Button>
        </Box>
      ) : (
        <Box sx={{ width: '100%', height }}>
          <ResponsiveContainer width="100%" height="100%">
            <LineChart
              data={chartData}
              margin={{
                top: isSmallMobile ? 10 : 20,
                right: isSmallMobile ? 15 : (isMobile ? 20 : 30),
                left: isSmallMobile ? 10 : 20,
                bottom: isSmallMobile ? 40 : (isMobile ? 50 : 60),
              }}
            >
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
              <XAxis
                dataKey="formattedDate"
                stroke="#666"
                fontSize={isSmallMobile ? 10 : 12}
                angle={isSmallMobile ? -45 : (period === 'weekly' ? -45 : 0)}
                textAnchor={isSmallMobile ? 'end' : (period === 'weekly' ? 'end' : 'middle')}
                height={isSmallMobile ? 60 : 80}
                interval="preserveStartEnd"
              />
              <YAxis
                stroke="#666"
                fontSize={isSmallMobile ? 10 : 12}
                allowDecimals={false}
                tickFormatter={(value) => value.toLocaleString()}
              />
              <Tooltip content={<CustomTooltip />} />
              <Legend />
              <Line
                type="monotone"
                dataKey="count"
                stroke="#1976d2"
                strokeWidth={isSmallMobile ? 2 : 3}
                dot={{
                  fill: '#1976d2',
                  strokeWidth: 2,
                  r: isSmallMobile ? 3 : 4
                }}
                activeDot={{
                  r: isSmallMobile ? 5 : 6,
                  stroke: '#fff',
                  strokeWidth: 2
                }}
                name="Total Users"
                connectNulls={false}
              />
            </LineChart>
          </ResponsiveContainer>
        </Box>
      )}

      <Box
        sx={{
          mt: isSmallMobile ? 1.5 : 2,
          display: 'flex',
          flexDirection: isSmallMobile ? 'column' : 'row',
          justifyContent: 'space-between',
          alignItems: isSmallMobile ? 'flex-start' : 'center',
          gap: isSmallMobile ? 1 : 0
        }}
      >
        <Typography
          variant="caption"
          color="text.secondary"
          sx={{ fontSize: isSmallMobile ? '0.7rem' : '0.75rem' }}
        >
          Last updated: {new Date().toLocaleString()}
        </Typography>
        <Typography
          variant="caption"
          color="text.secondary"
          sx={{ fontSize: isSmallMobile ? '0.7rem' : '0.75rem' }}
        >
          {chartData.length} data points
        </Typography>
      </Box>
    </Paper>
  );
};

export default UserGrowthChart;