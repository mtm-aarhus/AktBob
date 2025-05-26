using AAK.GetOrganized;
using AktBob.GetOrganized.Handlers.GetCaseMetadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.RelateDocuments;

internal static class RegisterHandler
{
    public static IServiceCollection AddRelateDocumentsHandler(this IServiceCollection services)
    {
        services.AddScoped<IRelateDocumentsHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<RelateDocumentsHandler>>();

            var inner = new RelateDocumentsHandler(provider.GetRequiredService<IGetOrganizedClient>());
            var withLogging = new RelateDocumentsHandlerLogging(inner, logger);
            var withException = new RelateDocumentsHandlerException(withLogging, logger);

            return withException;
        });

        return services;
    }
}