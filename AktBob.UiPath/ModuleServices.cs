using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.UiPath;
public static class ModuleServices
{
    public static IServiceCollection AddUiPathModule(this IServiceCollection services, IConfiguration configuration, List<Type> mediatorHandlers)
    {
        var tenancyName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("UiPath:TenancyName"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>($"UiPath:{tenancyName}:Username"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>($"UiPath:{tenancyName}:Password"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>($"UiPath:{tenancyName}:OrganizationUnitId"));

        services.AddMemoryCache();

        services.AddTransient<IUiPathOrchestratorApi, UiPathOrchestratorApi>();
        services.AddHttpClient<IUiPathOrchestratorApi, UiPathOrchestratorApi>(client =>
        {
            var url = Guard.Against.NullOrEmpty(configuration.GetValue<string>("UiPath:Url"));
            client.BaseAddress = new Uri(url);
        });

        mediatorHandlers.Add(typeof(AddQueueItemCommandHandler));

        return services;
    }
}
