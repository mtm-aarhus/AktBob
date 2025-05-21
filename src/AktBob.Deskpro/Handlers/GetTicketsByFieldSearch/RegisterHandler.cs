using AAK.Deskpro;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Deskpro.Handlers.GetTicketsByFieldSearch;
internal static class RegisterHandler
{
    public static IServiceCollection AddGetTicketsByFieldSearchHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetTicketsByFieldSearchHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetTicketsByFieldSearchHandler>>();

            var inner = new GetTicketsByFieldSearchHandler(provider.GetRequiredService<IDeskproClient>());
            var withLogging = new GetTicketsByFieldSearchHandlerLogging(inner, logger);
            var withExceptionHandling = new GetTicketsByFieldSearchHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
}