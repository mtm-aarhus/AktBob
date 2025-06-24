using AktBob.Email.Client;
using AktBob.Email.Handler;
using AktBob.Shared;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.Email;

public static class ModuleServices
{
    public static IServiceCollection AddEmailModuleServices(this IServiceCollection services, IConfiguration configuration)
    {
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:From"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:SmtpUrl"));

        services.AddHostedService<AzureServiceBusReceiver>();
        
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
