using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FunnyActivities.CrossCuttingConcerns.HealthMonitoring;

public class ExternalServiceHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly string _serviceUrl;
    private readonly string _serviceName;
    private readonly ILogger<ExternalServiceHealthCheck> _logger;

    public ExternalServiceHealthCheck(HttpClient httpClient, string serviceUrl, string serviceName, ILogger<ExternalServiceHealthCheck> logger)
    {
        _httpClient = httpClient;
        _serviceUrl = serviceUrl;
        _serviceName = serviceName;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(_serviceUrl, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy($"{_serviceName} is healthy.");
            }
            else
            {
                _logger.LogWarning($"{_serviceName} returned status code {response.StatusCode}.");
                return HealthCheckResult.Degraded($"{_serviceName} is degraded.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{_serviceName} health check failed.");
            return HealthCheckResult.Unhealthy($"{_serviceName} is unhealthy.", ex);
        }
    }
}