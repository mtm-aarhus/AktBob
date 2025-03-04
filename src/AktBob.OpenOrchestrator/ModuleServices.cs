using AktBob.OpenOrchestrator.Contracts;
using AktBob.Shared;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.OpenOrchestrator;
public static class ModuleServices
{
    public static IServiceCollection AddOpenOrchestratorModule(this IServiceCollection services, IConfiguration configuration)
    {
        Guard.Against.NullOrEmpty(configuration.GetConnectionString("OpenOrchestratorDb"));
        services.AddScoped<ICreateOpenOrchestratorQueueItemHandler, CreateQueueItemHandler>();
        services.AddScoped<IJobHandler<CreateQueueItemJob>, CreateQueueItem>();
        services.AddScoped<IOpenOrchestratorModule, Module>();
        return services;
    }
}
