using AAK.GetOrganized;
using AktBob.GetOrganized.Handlers.CreateCase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.GetAggregatedCase;

internal static class RegisterHandler
{
    public static IServiceCollection AddGetAggregatedCaseHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetAggregatedCaseHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetAggregatedCaseHandler>>();

            var inner = new GetAggregatedCaseHandler(provider.GetRequiredService<IGetOrganizedClient>());
            var withLogging = new GetAggregatedCaseHandlerLogging(inner, logger);
            var withException = new GetAggregatedCaseHandlerException(withLogging, logger);

            return withException;
        });

        return services;
    }
}