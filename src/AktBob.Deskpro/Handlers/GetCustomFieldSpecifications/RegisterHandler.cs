using AAK.Deskpro;
using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Deskpro.Handlers.GetCustomFieldSpecifications;
internal static class RegisterHandler
{
    public static IServiceCollection AddGetCustomFieldSpecificationsHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetCustomFieldSpecificationsHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetCustomFieldSpecificationsHandler>>();

            var inner = new GetCustomFieldSpecificationsHandler(provider.GetRequiredService<IDeskproClient>());
            var withCaching = new GetCustomFieldSpecificationsHandlerCaching(inner, provider.GetRequiredService<ICacheService>());
            var withLogging = new GetCustomFieldSpecificationsHandlerLogging(withCaching, logger);
            var withExceptionHandling = new GetCustomFieldSpecificationsHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
}
