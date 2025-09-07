using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.Seq;
using System.Diagnostics;

namespace FunnyActivities.CrossCuttingConcerns.Logging;

public static class SerilogConfiguration
{
    public static void ConfigureSerilog(IConfiguration configuration)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.With<CorrelationIdEnricher>()
            .Enrich.With<RequestContextEnricher>()
            .Enrich.With<EnvironmentEnricher>()
            .Enrich.WithProperty("Application", "FunnyActivities")
            .Enrich.WithProperty("Environment", environment)
            .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.StaticFiles"))
            .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Hosting"))
            .Filter.ByExcluding(evt => evt.Properties.ContainsKey("RequestPath") &&
                (evt.Properties["RequestPath"].ToString().Contains("/health") ||
                 evt.Properties["RequestPath"].ToString().Contains("/metrics")))
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);

        // Environment-specific minimum levels
        if (environment == "Development")
        {
            loggerConfig.MinimumLevel.Debug();
        }
        else if (environment == "Staging")
        {
            loggerConfig.MinimumLevel.Information();
        }
        else
        {
            loggerConfig.MinimumLevel.Warning();
        }

        // Conditional sink enabling
        if (configuration.GetSection("Serilog:WriteTo:Console").Exists())
        {
            loggerConfig.WriteTo.Console();
        }

        if (configuration.GetSection("Serilog:WriteTo:File").Exists())
        {
            loggerConfig.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
        }

        if (configuration.GetSection("Serilog:WriteTo:Elasticsearch").Exists())
        {
            var esUri = configuration["Serilog:WriteTo:Elasticsearch:Args:nodes"];
            var esUser = configuration["Elasticsearch:Username"];
            var esPass = configuration["Elasticsearch:Password"];

            var esOptions = new ElasticsearchSinkOptions(new Uri(esUri))
            {
                AutoRegisterTemplate = true,
                IndexFormat = "funnyactivities-{0:yyyy.MM.dd}",
                ModifyConnectionSettings = c =>
                {
                    if (!string.IsNullOrEmpty(esUser) && !string.IsNullOrEmpty(esPass))
                    {
                        c.BasicAuthentication(esUser, esPass);
                    }
                    return c;
                }
            };
            loggerConfig.WriteTo.Elasticsearch(esOptions);
        }

        if (configuration.GetSection("Serilog:WriteTo:Seq").Exists())
        {
            var seqUrl = configuration["Serilog:WriteTo:Seq:Args:serverUrl"];
            var seqApiKey = configuration["Serilog:WriteTo:Seq:Args:apiKey"];
            loggerConfig.WriteTo.Seq(seqUrl, apiKey: seqApiKey);
        }

        // Note: Azure Application Insights integration requires Serilog.Sinks.ApplicationInsights package
        // Uncomment and install package when needed
        // var appInsightsKey = configuration["ApplicationInsights:InstrumentationKey"];
        // if (!string.IsNullOrEmpty(appInsightsKey))
        // {
        //     loggerConfig.WriteTo.ApplicationInsights(appInsightsKey, TelemetryConverter.Traces);
        // }

        Log.Logger = loggerConfig.CreateLogger();
    }
}

public class CorrelationIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationId));
    }
}

public class RequestContextEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // In a real app, you'd inject HttpContextAccessor, but for simplicity:
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", "Anonymous"));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestId", Guid.NewGuid().ToString()));
    }
}

public class EnvironmentEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var machineName = Environment.MachineName;
        var processId = Environment.ProcessId.ToString();
        var threadId = Environment.CurrentManagedThreadId.ToString();

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("MachineName", machineName));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ProcessId", processId));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ThreadId", threadId));
    }
}