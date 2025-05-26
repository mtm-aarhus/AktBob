using AktBob.Email.Client;
using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.Email.Handler;

internal static class RegisterHandler
{
    public static IServiceCollection AddSendEmailHandler(this IServiceCollection services)
    {
        services.AddScoped<ISendEmailHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<SendEmailHandler>>();
            
            var inner = new SendEmailHandler(
                provider.GetRequiredService<IAppConfig>(),
                provider.GetRequiredService<ISmtpClient>(),
                logger);

            var withLogging = new SendEmailHandlerLogging(inner, logger);
            var withException = new SendEmailHandlerException(withLogging, logger);
            
            return withException;
        });
        
        return services;
    }
}