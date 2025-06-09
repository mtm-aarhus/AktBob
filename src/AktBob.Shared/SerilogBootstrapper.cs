using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace AktBob.Shared;

public static class SerilogBootstrapper
{
    public static void ConfigureLogging(this IServiceCollection services)
    {
        var applicationInsightsConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
        
        var logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console();

        if (!string.IsNullOrEmpty(applicationInsightsConnectionString))
        {
            services.AddApplicationInsightsTelemetry();
            logger.WriteTo.ApplicationInsights(applicationInsightsConnectionString, TelemetryConverter.Traces);
        }       
            
        Log.Logger = logger.CreateLogger();
        services.AddSerilog();
    }
}