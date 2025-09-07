using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace FunnyActivities.CrossCuttingConcerns.HealthMonitoring;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(string connectionString, ILogger<DatabaseHealthCheck> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await connection.CloseAsync();

            return HealthCheckResult.Healthy("Database is healthy.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed.");
            return HealthCheckResult.Unhealthy("Database is unhealthy.", ex);
        }
    }
}