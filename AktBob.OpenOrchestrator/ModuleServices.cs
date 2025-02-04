using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.OpenOrchestrator;
public static class ModuleServices
{
    public static IServiceCollection AddOpenOrchestratorModule(this IServiceCollection services, IConfiguration configuration, List<Type> mediatorHandlers)
    {
        // Make sure we have a connection string til the Open Orchestrator database
        Guard.Against.NullOrEmpty(configuration.GetConnectionString("OpenOrchestratorDb"));

        mediatorHandlers.Add(typeof(CreateQueueItemCommandHandler));
        return services;
    }
}
