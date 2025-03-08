using AktBob.OpenOrchestrator.Contracts;
using AktBob.Shared;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.OpenOrchestrator;
public static class ModuleServices
{
    public static IServiceCollection AddOpenOrchestratorModule(this IServiceCollection services, IConfiguration configuration)
    {
        Guard.Against.NullOrEmpty(configuration.GetConnectionString("OpenOrchestratorDb"));

        services.AddScoped<ICreateQueueItemHandler, CreateQueueItemHandler>();
        services.AddScoped<IJobHandler<CreateQueueItemJob>, CreateQueueItem>();

        services.AddScoped<IOpenOrchestratorModule>(provider =>
        {
            var inner = new Module(provider.GetRequiredService<IJobDispatcher>());

            var withLogging = new ModuleLoggingDecorator(
                inner,
                provider.GetRequiredService<ILogger<ModuleLoggingDecorator>>());

            var withExceptionHandling = new ModuleExceptionDecorator(
                withLogging,
                provider.GetRequiredService<ILogger<ModuleExceptionDecorator>>());

            return withExceptionHandling;
        });
        return services;
    }
}
