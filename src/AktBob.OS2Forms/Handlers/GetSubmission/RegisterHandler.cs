using AAK.OS2Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.OS2Forms.Handlers.GetSubmission; 

internal static class RegisterHandler
{
    public static IServiceCollection AddGetSubmissionHandler(this IServiceCollection services)
    {
        services.AddScoped<IGetSubmissionHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GetSubmissionHandler>>();

            var inner = new GetSubmissionHandler(provider.GetRequiredService<IOS2FormsClient>());
            var withLogging = new GetSubmissionHandlerLogging(inner, logger);
            var withException = new GetSubmissionHandlerException(withLogging, logger);

            return withException;
        });
        
        return services;
    }
}