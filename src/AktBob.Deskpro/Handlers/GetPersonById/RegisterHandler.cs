using AAK.Deskpro;
using AktBob.Deskpro.Contracts;
using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Deskpro.Handlers.GetPersonById;
internal static class RegisterHandler
{
    public static IServiceCollection AddGetPersonByIdHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetPersonByIdHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetPersonByIdHandler>>();

            var inner = new GetPersonByIdHandler(provider.GetRequiredService<IDeskproClient>());
            var withCaching = new GetPersonByIdHandlerCaching(inner, provider.GetRequiredService<ICacheService>());
            var withLogging = new GetPersonByIdHandlerLogging(withCaching, logger);
            var withExceptionHandling = new GetPersonByIdHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
}
