using AAK.Deskpro;
using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Deskpro.Handlers.GetTeam;
internal static class RegisterHandler
{
    public static IServiceCollection AddGetTeamHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetTeamHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetTeamHandler>>();

            var inner = new GetTeamHandler(provider.GetRequiredService<IDeskproClient>());
            var withCaching = new GetTeamHandlerCaching(inner, provider.GetRequiredService<ICacheService>());
            var withLogging = new GetTeamHandlerLogging(withCaching, logger);
            var withExceptionHandling = new GetTeamHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
}
