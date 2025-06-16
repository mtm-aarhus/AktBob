using AAK.Podio;
using Aktbob.Modules.Podio.Features;
using Ardalis.GuardClauses;

namespace Aktbob.Modules.Podio;

public static class RegisterModuleServices
{
    public static IServiceCollection AddModuleServices(this IServiceCollection services, IConfiguration configuration)
    {
        var podioAppTokens = Guard.Against.NullOrEmpty(configuration.GetSection("AppTokens").GetChildren().ToDictionary(x => x.Key, x => x.Value));
        services.AddPodioFactory(new Uri(Guard.Against.NullOrEmpty(configuration.GetValue<string>("BaseAddress"))));

        // Handlers
        services.AddGetItemHandler();
        services.AddPostCommentHandler();
        services.AddUpdateTextFieldHandler();
    
        return services;
    }
}
