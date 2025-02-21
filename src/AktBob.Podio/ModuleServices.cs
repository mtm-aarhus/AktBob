using AAK.Podio;
using AktBob.Podio.UseCases;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Podio;

public static class ModuleServices
{
    public static IServiceCollection AddPodioModule(this IServiceCollection services, IConfiguration configuration, List<Type> mediatorHandlers)
    {
        var podioAppTokens = Guard.Against.NullOrEmpty(configuration.GetSection("Podio:AppTokens").GetChildren().ToDictionary(x => x.Key, x => x.Value));
        services.AddPodioFactory(new Uri(Guard.Against.NullOrEmpty(configuration.GetValue<string>("Podio:BaseAddress"))));

        mediatorHandlers.AddRange([
            typeof(PostItemCommentCommandHandler),
            typeof(UpdateFieldCommandHandler),
            typeof(GetItemQueryHandler)]);

        return services;
    }
}
