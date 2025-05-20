using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.CloudConvert.Handlers.GetDownloadUrl;

internal static class RegisterHandler
{
    public static IServiceCollection AddGetDownloadUrlHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetDownloadUrlHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetDownloadUrlHandler>>();

            var inner = new GetDownloadUrlHandler(
                provider.GetRequiredService<ICloudConvertClient>(),
                provider.GetRequiredService<ITimeProvider>());

            var withLogging = new GetDownloadUrlHandlerLogging(inner, logger);
            var withExceptionHandling = new GetDownloadUrlHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
}
