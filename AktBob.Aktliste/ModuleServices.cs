using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Aktliste;

public static class ModuleServices
{
    public static IServiceCollection AddAktlisteModule(this IServiceCollection services, IConfiguration configuration)
    {
        Guard.Against.NullOrEmpty(configuration.GetConnectionString("AzureStorage"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("AktlisteModule:QueueName"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("AktlisteModule:UiPathQueueName"));

        return services;
    }
}
