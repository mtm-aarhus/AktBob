using Microsoft.Extensions.DependencyInjection;

namespace AktBob.CloudConvert.Handlers.ConvertHtmlToPdf;
internal static class RegisterHandler
{
    public static IServiceCollection AddConvertHtmlToPdfHandler(this IServiceCollection services)
    {
        services.AddScoped<IConvertHtmlToPdfHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<ConvertHtmlToPdfHandler>>();

            var inner = new ConvertHtmlToPdfHandler(provider.GetRequiredService<ICloudConvertClient>());
            var withLogging = new ConvertHtmlToPdfHandlerLogging(inner, logger);
            var withExceptionHandling = new ConvertHtmlToPdfHandlerException(withLogging, logger);

            return withExceptionHandling;
        });

        return services;
    }
}