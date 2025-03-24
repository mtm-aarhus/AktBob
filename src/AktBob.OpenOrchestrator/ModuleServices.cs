using AktBob.Database.Decorators;
using AktBob.OpenOrchestrator.Contracts;
using AktBob.OpenOrchestrator.Decorators;
using AktBob.Shared;
using AktBob.Shared.DataAccess;
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

        services.AddScoped<IOpenOrchestratorSqlConnection, OpenOrchestratorSqlConnection>();
        services.AddScoped<ISqlDataAccess<IOpenOrchestratorSqlConnection>>(provider =>
        {
            var inner = new SqlDataAccess<IOpenOrchestratorSqlConnection>(provider.GetRequiredService<IOpenOrchestratorSqlConnection>());

            var withLogging = new SqlDataAccessLoggingDecorator<IOpenOrchestratorSqlConnection>(
                inner,
                provider.GetRequiredService<ILogger<SqlDataAccess<IOpenOrchestratorSqlConnection>>>());

            var withException = new SqlDataAccessExceptionDecorator<IOpenOrchestratorSqlConnection>(
                withLogging,
                provider.GetRequiredService<ILogger<SqlDataAccess<IOpenOrchestratorSqlConnection>>>());

            return withException;
        });


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
