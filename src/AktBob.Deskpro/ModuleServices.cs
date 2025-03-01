using AAK.Deskpro;
using AktBob.Deskpro.Handlers;
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
        services.AddTransient<IGetDeskproCustomFieldSpecificationsHandler, GetDeskproCustomFieldSpecificationsHandler>();
        services.AddTransient<IGetDeskproMessageAttachmentHandler, GetDeskproMessageAttachmentHandler>();
        services.AddTransient<IGetDeskproMessageAttachmentsHandler, GetDeskproMessageAttachmentsHandler>();
        services.AddTransient<IGetDeskproMessageHandler, GetDeskproMessageHandler>();
        services.AddTransient<IGetDeskproMessagesHandler, GetDeskproMessagesHandler>();
        services.AddTransient<IGetDeskproPersonHandler, GetDeskproPersonHandler>();
        services.AddTransient<IGetDeskproTicketHandler, GetDeskproTicketHandler>();
        services.AddTransient<IGetDeskproTicketsByFieldSearchHandler, GetDeskproTicketsByFieldSearchHandler>();
        services.AddTransient<IInvokeDeskproWebhookHandler, InvokeDeskproWebhookHandler>();

        return services;
    }
}
