using AAK.FilArkiv;
using AktBob.FilArkiv.Contracts;
using AktBob.FilArkiv.Decorators;
using AktBob.FilArkiv.Handlers;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        services.AddScoped<IGetDocumentsHandler, GetDocumentsHandler>();
        services.AddScoped<IGetFileProcessStatusHandler, GetFileProcessStatusHandler>();

        services.AddScoped<IFilArkivModule>(serviceProvider =>
        {
            var inner = new FilArkivModule(
                serviceProvider.GetRequiredService<IGetDocumentsHandler>(),
                serviceProvider.GetRequiredService<IGetFileProcessStatusHandler>());

            var withLogging = new ModuleLoggingDecorator(
                inner,
                serviceProvider.GetRequiredService<ILogger<FilArkivModule>>());

            var withExceptionHandling = new ModuleExceptionDecorator(
                withLogging,
                serviceProvider.GetRequiredService<ILogger<FilArkivModule>>());

            return withExceptionHandling;
        });

        return services;
    }
}
