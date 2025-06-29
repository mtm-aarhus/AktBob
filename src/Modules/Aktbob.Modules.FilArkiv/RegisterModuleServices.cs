using AAK.FilArkiv;
using Aktbob.Modules.FilArkiv.Features;
using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;

namespace Aktbob.Modules.FilArkiv;

public static class RegisterModuleServices
{
    public static IServiceCollection AddFilArkivModule(this IServiceCollection services, string baseAddress, string clientId, string clientSecret, string tokenEndpoint)
    {
        Guard.Against.NullOrEmpty(baseAddress);
        Guard.Against.NullOrEmpty(clientId);
        Guard.Against.NullOrEmpty(clientSecret);
        Guard.Against.NullOrEmpty(tokenEndpoint);

        var filArkivOptions = new FilArkivOptions(baseAddress, clientId, clientSecret, tokenEndpoint);
        services.AddFilArkiv(filArkivOptions);
        
        // Add module handlers
        services
            .AddGetDocumentsByCaseIdHandler()
            .AddGetFileProcessStatusHandler();
        
        return services;
    }
}
