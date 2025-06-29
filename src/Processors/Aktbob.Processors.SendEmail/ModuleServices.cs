using Aktbob.Processors.SendEmail.Client;
using Aktbob.Processors.SendEmail.Handler;
using AktBob.Shared;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.SendEmail;

public static class ModuleServices
{
    public static IServiceCollection AddSendEmailProcessor(this IServiceCollection services, string from, string smtp, int port)
    {
        Guard.Against.NullOrEmpty(from);
        Guard.Against.NullOrEmpty(smtp);
        Guard.Against.Zero(port);

        services.AddHostedService<SendEmailBackgroundService>();
        
        services.AddTransient<ISmtpClient, SmtpClientWrapper>();
        services.AddScoped<ISendEmailHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<SendEmailHandler>>();
            
            var inner = new SendEmailHandler(
                provider.GetRequiredService<IAppConfig>(),
                provider.GetRequiredService<ISmtpClient>(),
                logger,
                from,
                smtp,
                port);

            var withLogging = new SendEmailHandlerLogging(inner, logger);
            var withException = new SendEmailHandlerException(withLogging, logger);
            
            return withException;
        });
        
        return services;
    }
}
