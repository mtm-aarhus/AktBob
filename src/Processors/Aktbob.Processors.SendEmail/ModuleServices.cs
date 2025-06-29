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
    public static IServiceCollection AddSendEmailProcessor(this IServiceCollection services, IConfiguration configuration)
    {
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:From"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:SmtpUrl"));

        services.AddHostedService<SendEmailBackgroundService>();
        
        services.AddTransient<ISendEmailHandler, SendEmailHandler>();
        services.AddTransient<ISmtpClient, SmtpClientWrapper>();

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
