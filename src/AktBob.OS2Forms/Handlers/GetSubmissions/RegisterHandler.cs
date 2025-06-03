using AAK.OS2Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.OS2Forms.Handlers.GetSubmissions; 

internal static class RegisterHandler
{
    public static IServiceCollection AddGetSubmissionsHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetSubmissionsHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetSubmissionsHandler>>();

            var inner = new GetSubmissionsHandler(provider.GetRequiredService<IOS2FormsClient>());
            var withLogging = new GetSubmissionsHandlerLogging(inner, logger);
            var withException = new GetSubmissionsHandlerException(withLogging, logger);

            return withException;
        });
        
        return services;
    }
}