using AAK.Podio;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AktBob.Podio;

public static class ModuleServices
{
    public static IServiceCollection AddPodioModule(this IServiceCollection services, IConfiguration configuration, List<Assembly> cqrsHandlersAssemblies)
    {
        var podioAppTokens = Guard.Against.NullOrEmpty(configuration.GetSection("Podio:AppTokens").GetChildren().ToDictionary(x => x.Key, x => x.Value));
        services.AddPodioFactory(new Uri(Guard.Against.NullOrEmpty(configuration.GetValue<string>("Podio:BaseAddress"))));

        cqrsHandlersAssemblies.Add(typeof(ModuleServices).Assembly);

        return services;
    }
}
