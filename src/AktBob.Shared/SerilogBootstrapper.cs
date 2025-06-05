using Serilog;
using Serilog.Events;

namespace AktBob.Shared;

public static class SerilogBootstrapper
{
    public static void ConfigureLogging(string? instrumentationKey)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.ApplicationInsights(instrumentationKey, TelemetryConverter.Traces)
            .CreateLogger();
    }
}