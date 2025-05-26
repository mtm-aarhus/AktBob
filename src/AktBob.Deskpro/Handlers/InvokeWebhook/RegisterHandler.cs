using AAK.Deskpro;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Deskpro.Handlers.InvokeWebhook;
internal static class RegisterHandler
{
    public static IServiceCollection AddInvokeWebhookHandler(this IServiceCollection services)
    {
        services.AddScoped<IInvokeWebhookHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<InvokeWebhookHandler>>();

            var inner = new InvokeWebhookHandler(provider.GetRequiredService<IDeskproClient>());
            var withLogging = new InvokeWebhookHandlerLogging(inner, logger);
            var withExceptionHandling = new InvokeWebhookHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
}