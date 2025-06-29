using AAK.Deskpro;
using Aktbob.Modules.Deskpro.Features;
using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;

namespace Aktbob.Modules.Deskpro;

public static class RegisterModuleServices
{
    public static IServiceCollection AddDeskproModule(this IServiceCollection services, string baseAddress, string key)
    {
        // Add Deskpro client
        var deskproOptions = new DeskproOptions
        {
            BaseAddress = Guard.Against.NullOrEmpty(baseAddress),
            AuthorizationKey = Guard.Against.NullOrEmpty(key)
        };

        services.AddDeskpro(deskproOptions);

        // Add module handlers
        services
            .AddDownloadMessageAttachmentHandler()
            .AddGetCustomFieldSpecificationsHandler()
            .AddGetMessageHandler()
            .AddGetMessageAttachmentsHandler()
            .AddGetMessagesHandler()
            .AddGetPersonByEmailHandler()
            .AddGetPersonByIdHandler()
            .AddGetTeamHandler()
            .AddGetTicketHandler()
            .AddGetTicketsByFieldSearchHandler()
            .AddInvokeWebhookHandler();
        
        return services;
    }
}
