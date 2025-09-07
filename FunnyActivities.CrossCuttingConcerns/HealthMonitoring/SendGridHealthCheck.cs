using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace FunnyActivities.CrossCuttingConcerns.HealthMonitoring;

public class SendGridHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SendGridHealthCheck> _logger;

    public SendGridHealthCheck(HttpClient httpClient, ILogger<SendGridHealthCheck> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("https://api.sendgrid.com/v3/healthcheck", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("SendGrid is healthy.");
            }
            else
            {
                _logger.LogWarning("SendGrid returned status code {StatusCode}.", response.StatusCode);
                return HealthCheckResult.Degraded("SendGrid is degraded.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SendGrid health check failed.");
            return HealthCheckResult.Unhealthy("SendGrid is unhealthy.", ex);
        }
    }
}