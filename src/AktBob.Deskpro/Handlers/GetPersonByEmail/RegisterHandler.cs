using AAK.Deskpro;
using AktBob.Deskpro.Handlers.GetPerson;
using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Deskpro.Handlers.GetPersonByEmail;
internal static class RegisterHandler
{
    public static IServiceCollection AddGetPersonByEmailHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetPersonByEmailHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetPersonByEmailHandler>>();

            var inner = new GetPersonByEmailHandler(
                provider.GetRequiredService<IDeskproClient>(),
                provider.GetRequiredService<IAppConfig>());

            var withCaching = new GetPersonByEmailHandlerCaching(inner, provider.GetRequiredService<ICacheService>());
            var withLogging = new GetPersonByEmailHandlerLogging(withCaching, logger);
            var withExceptionHandling = new GetPersonByEmailHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
}
