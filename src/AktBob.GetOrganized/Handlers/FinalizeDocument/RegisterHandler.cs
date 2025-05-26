using AAK.GetOrganized;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.FinalizeDocument;

internal static class RegisterHandler
{
    public static IServiceCollection AddFinalizeDocumentHandler(this IServiceCollection services)
    {
        services.AddScoped<IFinalizeDocumentHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<FinalizeDocumentHandler>>();

            var inner = new FinalizeDocumentHandler(provider.GetRequiredService<IGetOrganizedClient>());
            var withLogging = new FinalizeDocumentHandlerLogging(inner, logger);
            var withException = new FinalizeDocumentHandlerException(withLogging, logger);

            return withException;
        });

        return services;
    }
}