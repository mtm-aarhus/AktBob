using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AktBob.OpenOrchestrator;
public static class ModuleServices
{
    public static IServiceCollection AddOpenOrchestratorModule(this IServiceCollection services, IConfiguration configuration, List<Assembly> mediatrAssemblies)
    {
        // Make sure we have a connection string til the Open Orchestrator database
        Guard.Against.NullOrEmpty(configuration.GetConnectionString("OpenOrchestratorDb"));

        mediatrAssemblies.Add(typeof(ModuleServices).Assembly);
        return services;
    }
}
