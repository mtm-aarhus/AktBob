using AAK.Deskpro;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Deskpro.Handlers.GetTicket;
internal static class RegisterHandler
{
    public static IServiceCollection AddGetTicketHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetTicketHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetTicketHandler>>();

            var inner = new GetTicketHandler(provider.GetRequiredService<IDeskproClient>());
            var withLogging = new GetTicketHandlerLogging(inner, logger);
            var withExceptionHandling = new GetTicketHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
}