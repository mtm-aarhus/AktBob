using AAK.FilArkiv;
using Aktbob.Modules.FilArkiv.Features;
using Ardalis.GuardClauses;

namespace Aktbob.Modules.FilArkiv;

internal static class RegisterModuleServices
{
    public static IServiceCollection AddModuleServices(this IServiceCollection services, IConfiguration configuration)
    {
        var baseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:BaseAddress"));
        var clientId = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:ClientId"));
        var clientSecret = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:ClientSecret"));
        var tokenEndpoint = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:TokenEndpoint"));

        var filArkivOptions = new FilArkivOptions(baseAddress, clientId, clientSecret, tokenEndpoint);
        services.AddFilArkiv(filArkivOptions);
        
        // Add module handlers
        services
            .AddGetDocumentsByCaseIdHandler()
            .AddGetFileProcessStatusHandler();
        
        return services;
    }
}
