using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.PodioHookProcessor;

public static class ModuleServices
{
    public static IServiceCollection AddPodioHookProcessorModule(this IServiceCollection services, IConfiguration configuration)
    {
        Guard.Against.NullOrEmpty(configuration.GetConnectionString("AzureStorage"));
        var tenancyName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("UiPath:TenancyName"));

        Guard.Against.NullOrEmpty(configuration.GetValue<string>($"DocumentListTrigger:{tenancyName}:AzureQueueName"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>($"DocumentListTrigger:{tenancyName}:UiPathQueueName"));

        Guard.Against.NullOrEmpty(configuration.GetValue<string>($"OCRScreeningTrigger:{tenancyName}:AzureQueueName"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>($"OCRScreeningTrigger:{tenancyName}:UiPathQueueName"));

        services.AddHostedService<UseCases.DocumentListTrigger.BackgroundWorker>();
        services.AddHostedService<UseCases.OCRScreeningTrigger.BackgroundWorker>();
        services.AddHostedService<UseCases.MoveToSharepointTrigger.BackgroundWorker>();

        return services;
    }
}
