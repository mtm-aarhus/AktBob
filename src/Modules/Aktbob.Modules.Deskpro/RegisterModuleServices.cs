using AAK.Deskpro;
using Ardalis.GuardClauses;

namespace Aktbob.Modules.Deskpro;

internal static class RegisterModuleServices
{
    public static IServiceCollection AddModuleServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Deskpro client
        var deskproOptions = new DeskproOptions
        {
            BaseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>("BaseAddress")),
            AuthorizationKey = Guard.Against.NullOrEmpty(configuration.GetValue<string>("AuthorizationKey"))
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
