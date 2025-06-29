using AAK.Podio;
using Aktbob.Modules.Podio.Features;
using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;

namespace Aktbob.Modules.Podio;

public static class RegisterModuleServices
{
    public static IServiceCollection AddPodioModule(this IServiceCollection services, string baseAddress, Dictionary<int, string> appTokens, string clientId, string clientSecret)
    {
        services.AddSingleton<IConfigurationHelper>(_ => new ConfigurationHelper(
            Guard.Against.Null(appTokens),
            Guard.Against.NullOrEmpty(clientId),
            Guard.Against.NullOrEmpty(clientSecret)));
        
        services.AddPodioFactory(new Uri(Guard.Against.NullOrEmpty(baseAddress)));

        // Handlers
        services.AddGetItemHandler();
        services.AddPostCommentHandler();
        services.AddUpdateTextFieldHandler();
    
        return services;
    }
}
