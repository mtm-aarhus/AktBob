using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Dokument;

public static class ModuleServices
{
    public static IServiceCollection AddDokumentModule(this IServiceCollection services, IConfiguration configuration)
    {
        Guard.Against.NullOrEmpty(configuration.GetConnectionString("AzureStorage"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("AktlisteModule:QueueName"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("AktlisteModule:UiPathQueueName"));

        services.AddHostedService<Worker>();

        return services;
    }
}
