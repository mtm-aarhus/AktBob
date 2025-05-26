using AAK.GetOrganized;
using AktBob.GetOrganized.Handlers.RelateDocuments;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.UpdateCaseMetadata;

internal static class RegisterHandler
{
    public static IServiceCollection AddUpdateCaseMetadataHandler(this IServiceCollection services)
    {
        services.AddScoped<IUpdateCaseMetadataHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<UpdateCaseMetadataHandler>>();

            var inner = new UpdateCaseMetadataHandler(provider.GetRequiredService<IGetOrganizedClient>());
            var withLogging = new UpdateCaseMetadataHandlerLogging(inner, logger);
            var withException = new UpdateCaseMetadataHandlerException(withLogging, logger);

            return withException;
        });

        return services;
    }
}