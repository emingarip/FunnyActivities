using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Filters;
using System.Diagnostics;
using System.Linq;

namespace FunnyActivities.CrossCuttingConcerns.Logging;

public static class SerilogConfiguration
{
    public static void ConfigureSerilog(IConfiguration configuration)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var loggerConfig = new LoggerConfiguration()
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

        var configuredSinks = configuration.GetSection("Serilog:WriteTo").GetChildren().ToList();
        if (configuredSinks.Count > 0)
        {
            loggerConfig.ReadFrom.Configuration(configuration);
        }
        else
        {
            // Production deployments can run with an external appsettings.Production.json
            // that omits Serilog configuration entirely. Keep console and rolling files on
            // by default so journalctl and log shippers always have data to read.
            loggerConfig
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}> {CorrelationId}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    fileSizeLimitBytes: 104857600,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/error-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    fileSizeLimitBytes: 104857600,
                    restrictedToMinimumLevel: LogEventLevel.Error,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
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
