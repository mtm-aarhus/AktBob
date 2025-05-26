using AAK.FilArkiv;
using AktBob.FilArkiv.Contracts;
using AktBob.FilArkiv.Handlers.GetDocumentsByCaseId;
using AktBob.FilArkiv.Handlers.GetFileProcessStatus;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.FilArkiv;

public static class ModuleServices
{
    public static IServiceCollection AddFilArkivModule(this IServiceCollection services, IConfiguration configuration)
    {
        var baseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:BaseAddress"));
        var clientId = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:ClientId"));
        var clientSecret = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:ClientSecret"));
        var tokenEndpoint = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:TokenEndpoint"));

        var filArkivOptions = new FilArkivOptions(baseAddress, clientId, clientSecret, tokenEndpoint);
        services.AddFilArkiv(filArkivOptions);

        services.AddGetDocumentsByCaseIdHandler();
        services.AddGetFileProcessStatusHandler();

        services.AddScoped<IFilArkivModule, FilArkivModule>();

        return services;
    }
}
