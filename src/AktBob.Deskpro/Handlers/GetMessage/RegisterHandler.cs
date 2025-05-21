using AAK.Deskpro;
using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Deskpro.Handlers.GetMessage;
internal static class RegisterHandler
{
    public static IServiceCollection AddGetMessageHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetMessageHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetMessageHandler>>();

            var inner = new GetMessageHandler(provider.GetRequiredService<IDeskproClient>());
            var withCaching = new GetMessageHandlerCaching(inner, provider.GetRequiredService<ICacheService>());
            var withLogging = new GetMessageHandlerLogging(withCaching, logger);
            var withExceptionHandling = new GetMessageHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
}
