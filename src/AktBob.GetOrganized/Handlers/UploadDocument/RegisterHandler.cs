using AAK.GetOrganized;
using AktBob.GetOrganized.Handlers.UpdateCaseMetadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.UploadDocument;

internal static class RegisterHandler
{
    public static IServiceCollection AddUploadDocumentHandler(this IServiceCollection services)
    {
        services.AddScoped<IUploadDocumentHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<UploadDocumentHandler>>();

            var inner = new UploadDocumentHandler(
                provider.GetRequiredService<IConfiguration>(),
                provider.GetRequiredService<IGetOrganizedClient>());
            
            var withLogging = new UploadDocumentHandlerLogging(inner, logger);
            var withException = new UploadDocumentHandlerException(withLogging, logger);

            return withException;
        });

        return services;
    }
}