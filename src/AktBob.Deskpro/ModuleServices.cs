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
        services.AddScoped<IGetDeskproCustomFieldSpecificationsHandler, GetDeskproCustomFieldSpecificationsHandler>();
        services.AddScoped<IGetDeskproMessageAttachmentHandler, GetDeskproMessageAttachmentHandler>();
        services.AddScoped<IGetDeskproMessageAttachmentsHandler, GetDeskproMessageAttachmentsHandler>();
        services.AddScoped<IGetDeskproMessageHandler, GetDeskproMessageHandler>();
        services.AddScoped<IGetDeskproMessagesHandler, GetDeskproMessagesHandler>();
        services.AddScoped<IGetDeskproPersonHandler, GetDeskproPersonHandler>();
        services.AddScoped<IGetDeskproTicketHandler, GetDeskproTicketHandler>();
        services.AddScoped<IGetDeskproTicketsByFieldSearchHandler, GetDeskproTicketsByFieldSearchHandler>();
        services.AddScoped<IInvokeDeskproWebhookHandler, InvokeDeskproWebhookHandler>();
        services.AddScoped<IDeskproHandlers, DeskproHandlers>();

        return services;
    }
}
