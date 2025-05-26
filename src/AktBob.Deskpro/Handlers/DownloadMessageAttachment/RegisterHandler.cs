using AAK.Deskpro;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Deskpro.Handlers.DownloadMessageAttachment;
internal static class RegisterHandler
{
    public static IServiceCollection AddDownloadMessageAttachmentHandler(this IServiceCollection services)
    {
        services.AddScoped<IDownloadMessageAttachmentHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<DownloadMessageAttachmentHandler>>();

            var inner = new DownloadMessageAttachmentHandler(provider.GetRequiredService<IDeskproClient>());
            var withLogging = new DownloadMessageAttachmentHandlerLogging(inner, logger);
            var withExceptionHandling = new DownloadMessageAttachmentHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
}
