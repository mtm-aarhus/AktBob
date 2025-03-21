using AktBob.OpenOrchestrator.Contracts;
using AktBob.OpenOrchestrator.Decorators;
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

        services.AddSingleton<IOpenOrchestratorSqlConnectionFactory, OpenOrchestratorSqlConnectionFactory>();
        services.AddScoped<ICreateQueueItemHandler, CreateQueueItemHandler>();
        services.AddScoped<IJobHandler<CreateQueueItemJob>, CreateQueueItem>();

        services.AddScoped<IOpenOrchestratorModule>(provider =>
        {
            var inner = new OpenOrchestratorModule(provider.GetRequiredService<IJobDispatcher>());

            var withLogging = new ModuleLoggingDecorator(
                inner,
                provider.GetRequiredService<ILogger<OpenOrchestratorModule>>());

            var withExceptionHandling = new ModuleExceptionDecorator(
                withLogging,
                provider.GetRequiredService<ILogger<OpenOrchestratorModule>>());

            return withExceptionHandling;
        });

        return services;
    }
}
