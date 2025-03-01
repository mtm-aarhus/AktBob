using AAK.Podio;
using AktBob.Podio.Contracts;
using AktBob.Podio.Handlers;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Podio;

public static class ModuleServices
{
    public static IServiceCollection AddPodioModule(this IServiceCollection services, IConfiguration configuration)
    {
        var podioAppTokens = Guard.Against.NullOrEmpty(configuration.GetSection("Podio:AppTokens").GetChildren().ToDictionary(x => x.Key, x => x.Value));
        services.AddPodioFactory(new Uri(Guard.Against.NullOrEmpty(configuration.GetValue<string>("Podio:BaseAddress"))));

        services.AddTransient<IGetPodioItemHandler, GetPodioItemHandler>();
        services.AddTransient<IPostPodioItemCommentHandler, PostPodioItemCommentHandler>();
        services.AddTransient<IUpdatePodioFieldHandler, UpdatePodioFieldHandler>();

        return services;
    }
}
