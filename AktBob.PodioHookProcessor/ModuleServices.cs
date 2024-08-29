using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.PodioHookProcessor;

public static class ModuleServices
{
    public static IServiceCollection AddPodioHookProcessorModule(this IServiceCollection services, IConfiguration configuration)
    {
        Guard.Against.NullOrEmpty(configuration.GetConnectionString("AzureStorage"));

        //services.AddHostedService<UseCases.DocumentListTrigger.BackgroundWorker>();
        //services.AddHostedService<UseCases.GoToFilArkivTrigger.BackgroundWorker>();
        services.AddHostedService<UseCases.ToSharepointTrigger.BackgroundWorker>();

        return services;
    }
}
