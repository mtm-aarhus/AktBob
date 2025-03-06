using AAK.Deskpro;
using AktBob.Deskpro.Handlers;
using AktBob.Deskpro.Jobs;
using AktBob.Shared;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Deskpro;

public static class ModuleServices
{
    public static IServiceCollection AddDeskproModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Deskpro client
        var deskproOptions = new DeskproOptions
        {
            BaseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Deskpro:BaseAddress")),
            AuthorizationKey = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Deskpro:AuthorizationKey"))
        };

        services.AddDeskpro(deskproOptions);

        // Add module handlers
        services.AddScoped<IGetCustomFieldSpecificationsHandler, GetCustomFieldSpecificationsHandler>();
        services.AddScoped<IGetMessageAttachmentHandler, GetMessageAttachmentHandler>();
        services.AddScoped<IGetMessageAttachmentsHandler, GetMessageAttachmentsHandler>();
        services.AddScoped<IGetMessageHandler, GetMessageHandler>();
        services.AddScoped<IGetMessagesHandler, GetMessagesHandler>();
        services.AddScoped<IGetPersonHandler, GetPersonHandler>();
        services.AddScoped<IGetTicketHandler, GetTicketHandler>();
        services.AddScoped<IGetTicketsByFieldSearchHandler, GetTicketsByFieldSearchHandler>();
        services.AddScoped<IInvokeWebhookHandler, InvokeWebhookHandler>();

        // Jobs
        services.AddScoped<IJobHandler<InvokeWebhookJob>, InvokeWebhook>();

        // Module service orchestration
        services.AddScoped<IDeskproModule>(provider =>
        {
            var inner = new Module(
                provider.GetRequiredService<IJobDispatcher>(),
                provider.GetRequiredService<IGetCustomFieldSpecificationsHandler>(),
                provider.GetRequiredService<IGetMessageAttachmentHandler>(),
                provider.GetRequiredService<IGetMessageAttachmentsHandler>(),
                provider.GetRequiredService<IGetMessageHandler>(),
                provider.GetRequiredService<IGetMessagesHandler>(),
                provider.GetRequiredService<IGetPersonHandler>(),
                provider.GetRequiredService<IGetTicketHandler>(),
                provider.GetRequiredService<IGetTicketsByFieldSearchHandler>());

            var withLogging = new ModuleLoggingDecorator(
                inner,
                provider.GetRequiredService<ILogger<ModuleLoggingDecorator>>());

            var withExceptionHandling = new ModuleExceptionDecorator(
                withLogging,
                provider.GetRequiredService<ILogger<ModuleExceptionDecorator>>());

            return withExceptionHandling;
        });

        return services;
    }
}
