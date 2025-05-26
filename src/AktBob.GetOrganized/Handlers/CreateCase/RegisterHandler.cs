using AAK.GetOrganized;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.GetOrganized.Handlers.CreateCase;

internal static class RegisterHandler
{
    public static IServiceCollection AddCreateCaseHandler(this IServiceCollection services)
    {
        services.AddScoped<ICreateCaseHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<CreateCaseHandler>>();

            var inner = new CreateCaseHandler(provider.GetRequiredService<IGetOrganizedClient>());
            var withLogging = new CreateCaseHandlerLogging(inner, logger);
            var withException = new CreateCaseHandlerException(withLogging, logger);

            return withException;
        });

        return services;
    }
}