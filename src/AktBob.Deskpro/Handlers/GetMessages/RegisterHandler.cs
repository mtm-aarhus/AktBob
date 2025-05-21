using AAK.Deskpro;
using AktBob.Deskpro.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Deskpro.Handlers.GetMessages;
internal static class RegisterHandler
{
    public static IServiceCollection AddGetMessagesHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetMessagesHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetMessagesHandler>>();

            var inner = new GetMessagesHandler(
                provider.GetRequiredService<IDeskproClient>(),
                provider.GetRequiredService<IGetPersonByIdHandler>());

            var withLogging = new GetMessagesHandlerLogging(inner, logger);
            var withExceptionHandling = new GetMessagesHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
}
