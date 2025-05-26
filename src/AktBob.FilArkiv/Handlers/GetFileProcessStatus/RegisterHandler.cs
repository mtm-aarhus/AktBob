using AAK.FilArkiv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.FilArkiv.Handlers.GetFileProcessStatus;

internal static class RegisterHandler
{
    public static IServiceCollection AddGetFileProcessStatusHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetFileProcessStatusHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetFileProcessStatusHandler>>();

            var inner = new GetFileProcessStatusHandler(provider.GetRequiredService<IFilArkiv>());
            var withLogging = new GetFileProcessStatusHandlerLogging(inner, logger);
            var withException = new GetFileProcessStatusHandlerException(withLogging, logger);

            return withException;
        });
        
        return services;
    }
}