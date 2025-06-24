using AktBob.Email.Client;
using AktBob.Email.Handler;
using AktBob.Shared;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Formatting.Display;
using Serilog.Sinks.Email;

namespace AktBob.Email;

public static class ModuleServices
{
    public static IServiceCollection AddModuleServices(this IServiceCollection services, IConfiguration configuration)
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

    public static IServiceCollection ConfigureLogging(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddSerilog(config =>
        {
            config.Enrich.FromLogContext();

            if (environment.IsDevelopment())
            {
                config.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:j} {NewLine}{Exception}");
            }
            
            if (environment.IsProduction())
            {
                config.WriteTo.File(
                    outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:j} {NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day,
                    shared: true,
                    path: Guard.Against.NullOrEmpty(configuration.GetValue<string>("LogFilesPath")));
            }

            if (configuration.GetValue<bool?>("EmailLogEvents:Enabled") ?? false)
            {
                config.WriteTo.Email(
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
                    options: new EmailSinkOptions
                    {
                        To = Guard.Against.NullOrEmpty(configuration.GetSection("EmailLogEvents:To").Get<IEnumerable<string>>()).ToList(),
                        From = Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailLogEvents:From")),
                        Host = Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailLogEvents:Host")),
                        Port = Guard.Against.Null(configuration.GetValue<int?>("EmailLogEvents:Port")),
                        Subject = new MessageTemplateTextFormatter("{Timestamp:HH:mm:ss.fff} AktBob log messages"),
                        ConnectionSecurity = MailKit.Security.SecureSocketOptions.None
                    },
                    batchingOptions: new BatchingOptions
                    {
                        BufferingTimeLimit = TimeSpan.FromMinutes(Guard.Against.Null(configuration.GetValue<int?>("EmailLogEvents:TimeLimitMinutes"))),
                        EagerlyEmitFirstEvent = false
                    });
            }

            config.ReadFrom.Configuration(configuration);
        });
        
        return services;
    }
}
