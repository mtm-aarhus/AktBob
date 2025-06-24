using AktBob.Email.Client;
using AktBob.Email.Contracts;
using AktBob.Email.Handler;
using AktBob.Email.Jobs;
using AktBob.Shared;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddScoped<IJobHandler<SendEmailJob>, SendEmailJobHandler>();

        services.AddSendEmailHandler();
        services.AddScoped<IEmailModule, EmailModule>();

        return services;
    }
}
