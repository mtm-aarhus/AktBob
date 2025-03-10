using AktBob.Email.Contracts;
using AktBob.Email.Decorators;
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

        services.AddScoped<IJobHandler<SendEmailJob>, SendEmailJobHandler>();

        services.AddScoped<IEmailModule>(provider =>
        {
            var inner = new EmailModule(provider.GetRequiredService<IJobDispatcher>());

            var withLogging = new ModuleLoggingDecorator(
                inner,
                provider.GetRequiredService<ILogger<EmailModule>>());

            var withExceptionHandling = new ModuleExceptionDecorator(
                withLogging,
                provider.GetRequiredService<ILogger<EmailModule>>());

            return withExceptionHandling;
        });

        return services;
    }
}
