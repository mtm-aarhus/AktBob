using AktBob.CloudConvert.Client;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.CloudConvert.Handlers.DownloadFile;
internal static class RegisterHandler
{
    public static IServiceCollection AddDownloadFileHandler(this IServiceCollection services)
    {
        services.AddScoped<IDownloadFileHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<DownloadFileHandler>>();

            var inner = new DownloadFileHandler(provider.GetRequiredService<ICloudConvertClient>());
            var withLogging = new DownloadFileHandlerLogging(inner, logger);
            var withExceptionHandling = new DownloadFileHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
}
