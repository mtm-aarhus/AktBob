using AAK.Podio;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.PodioHookProcessor;

public static class ModuleServices
{
    public static IServiceCollection AddPodioHookProcessorModule(this IServiceCollection services, IConfiguration configuration)
    {
        Guard.Against.NullOrEmpty(configuration.GetConnectionString("AzureStorage"));

        var podioAppTokens = Guard.Against.NullOrEmpty(configuration.GetSection("Podio:AppTokens").GetChildren().ToDictionary(x => x.Key, x => x.Value));

        services.AddPodio(new PodioOptions(
            BaseAddress: Guard.Against.NullOrEmpty(configuration.GetValue<string>("Podio:BaseAddress")),
            ClientId: Guard.Against.NullOrEmpty(configuration.GetValue<string>("Podio:ClientId")),
            ClientSecret: Guard.Against.NullOrEmpty(configuration.GetValue<string>("Podio:ClientSecret")),
            AppTokens: podioAppTokens.Select(p => new KeyValuePair<int, string>(int.Parse(p.Key), p.Value)).ToDictionary().AsReadOnly())
        );

        services.AddHostedService<UseCases.DocumentListTrigger.BackgroundWorker>();
        services.AddHostedService<UseCases.GoToFilArkivTrigger.BackgroundWorker>();
        services.AddHostedService<UseCases.ToSharepointTrigger.BackgroundWorker>();

        return services;
    }
}
