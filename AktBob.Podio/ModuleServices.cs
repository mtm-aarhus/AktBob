using AAK.Podio;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Podio;

public static class ModuleServices
{
    public static IServiceCollection AddPodioModule(this IServiceCollection services, IConfiguration configuration, List<Type> mediatorHandlers)
    {
        Guard.Against.NullOrEmpty(configuration.GetConnectionString("AzureStorage"));

        var podioAppTokens = Guard.Against.NullOrEmpty(configuration.GetSection("Podio:AppTokens").GetChildren().ToDictionary(x => x.Key, x => x.Value));

        services.AddPodio(new PodioOptions(
            BaseAddress: Guard.Against.NullOrEmpty(configuration.GetValue<string>("Podio:BaseAddress")),
            ClientId: Guard.Against.NullOrEmpty(configuration.GetValue<string>("Podio:ClientId")),
            ClientSecret: Guard.Against.NullOrEmpty(configuration.GetValue<string>("Podio:ClientSecret")),
            AppTokens: podioAppTokens.Select(p => new KeyValuePair<int, string>(int.Parse(p.Key), p.Value)).ToDictionary().AsReadOnly())
        );

        mediatorHandlers.AddRange([
            typeof(PostItemCommentCommandHandler),
            typeof(UpdateItemFieldCommandHandler),
            typeof(GetItemQueryHandler)]);

        return services;
    }
}
