using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

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
            logger.WriteTo.ApplicationInsights(
                applicationInsightsConnectionString,
                new TemplateTraceTelemetryConverter());
        }       
            
        Log.Logger = logger.CreateLogger();
        services.AddSerilog();
    }
}

public class TemplateTraceTelemetryConverter : TraceTelemetryConverter
{
    public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
    {
        var templateParser = new MessageTemplateParser();
        LogEvent newLogEvent = new LogEvent(
            logEvent.Timestamp,
            logEvent.Level,
            logEvent.Exception,
            logEvent.MessageTemplate,
            logEvent.Properties.Select(p => new LogEventProperty(p.Key, p.Value)));
        return base.Convert(newLogEvent, formatProvider);
    }
}