using AAK.GetOrganized;
using AktBob.GetOrganized.Handlers.GetAggregatedCase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.GetCaseMetadata;

internal static class RegisterHandler
{
    public static IServiceCollection AddGetCaseMetadataHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetCaseMetadataHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetCaseMetadataHandler>>();

            var inner = new GetCaseMetadataHandler(provider.GetRequiredService<IGetOrganizedClient>());
            var withLogging = new GetCaseMetadataHandlerLogging(inner, logger);
            var withException = new GetCaseMetadataHandlerException(withLogging, logger);

            return withException;
        });

        return services;
    }
}