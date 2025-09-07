using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace FunnyActivities.CrossCuttingConcerns.HealthMonitoring;

public class TwilioHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TwilioHealthCheck> _logger;

    public TwilioHealthCheck(HttpClient httpClient, ILogger<TwilioHealthCheck> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("https://api.twilio.com/2010-04-01/Accounts.json", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("Twilio is healthy.");
            }
            else
            {
                _logger.LogWarning("Twilio returned status code {StatusCode}.", response.StatusCode);
                return HealthCheckResult.Degraded("Twilio is degraded.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Twilio health check failed.");
            return HealthCheckResult.Unhealthy("Twilio is unhealthy.", ex);
        }
    }
}