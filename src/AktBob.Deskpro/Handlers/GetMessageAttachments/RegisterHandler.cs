using AAK.Deskpro;
using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Deskpro.Handlers.GetMessageAttachments;
internal static class RegisterHandler
{
    public static IServiceCollection AddGetMessageAttachmentsHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetMessageAttachmentsHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetMessageAttachmentsHandler>>();

            var inner = new GetMessageAttachmentsHandler(provider.GetRequiredService<IDeskproClient>());
            var withCaching = new GetMessageAttachmentsHandlerCaching(inner, provider.GetRequiredService<ICacheService>());
            var withLogging = new GetMessageAttachmentsHandlerLogging(withCaching, logger);
            var withExceptionHandling = new GetMessageAttachmentsHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
}
