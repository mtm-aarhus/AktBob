using AAK.Deskpro;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Handlers.DownloadMessageAttachment;
using AktBob.Deskpro.Handlers.GetCustomFieldSpecifications;
using AktBob.Deskpro.Handlers.GetMessage;
using AktBob.Deskpro.Handlers.GetMessageAttachments;
using AktBob.Deskpro.Handlers.GetMessages;
using AktBob.Deskpro.Handlers.GetPersonByEmail;
using AktBob.Deskpro.Handlers.GetPersonById;
using AktBob.Deskpro.Handlers.GetTeam;
using AktBob.Deskpro.Handlers.GetTicket;
using AktBob.Deskpro.Handlers.GetTicketsByFieldSearch;
using AktBob.Deskpro.Handlers.InvokeWebhook;
using AktBob.Deskpro.Jobs;
using AktBob.Shared;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Deskpro;

public static class ModuleServices
{
    public static IServiceCollection AddOldDeskproModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Deskpro client
        var deskproOptions = new DeskproOptions
        {
            BaseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Deskpro:BaseAddress")),
            AuthorizationKey = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Deskpro:AuthorizationKey"))
        };

        services.AddDeskpro(deskproOptions);

        // Add module handlers
        services.AddDownloadMessageAttachmentHandler();
        services.AddGetCustomFieldSpecificationsHandler();
        services.AddGetMessageHandler();
        services.AddGetMessageAttachmentsHandler();
        services.AddGetMessagesHandler();
        services.AddGetPersonByEmailHandler();
        services.AddGetPersonByIdHandler();
        services.AddGetTeamHandler();
        services.AddGetTicketHandler();
        services.AddGetTicketsByFieldSearchHandler();
        services.AddInvokeWebhookHandler();

        // Jobs
        services.AddScoped<IJobHandler<InvokeWebhookJob>, InvokeWebhook>();

        // Module orchestration
        services.AddScoped<IDeskproModule, DeskproModule>();

        return services;
    }
}
