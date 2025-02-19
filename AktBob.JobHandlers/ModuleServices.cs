using AktBob.PodioHookProcessor.UseCases;
using AktBob.Shared.Contracts;
using AktBob.Shared;
using Microsoft.Extensions.DependencyInjection;
using AktBob.JobHandlers.Handlers;
using AktBob.JobHandlers.Utils;
using AktBob.JobHandlers.Handlers.AddMessageToGetOrganized;
using FilArkivCore.Web.Client;
using AktBob.JobHandlers.Handlers.CheckOCRScreeningStatus;

namespace AktBob.JobHandlers;
public static class ModuleServices
{
    public static IServiceCollection AddJobHandlers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IJobHandler<RegisterMessagesJob>, RegisterMessagesJobHandler>();
        services.AddTransient<IJobHandler<AddOrUpdateDeskproTicketToGetOrganizedJob>, Handlers.AddOrUpdateDeskproTicketToGetOrganized.AddOrUpdateDeskproTicketToGetOrganizedJobHandler>();
        services.AddTransient<IJobHandler<CreateAfgørelsesskrivelseQueueItemJob>, CreateAfgørelsesskrivelseQueueItemJobHandler>();
        services.AddTransient<IJobHandler<CreateDocumentListQueueItemJob>, CreateDocumentListQueueItemJobHandler>();
        services.AddTransient<IJobHandler<CreateGetOrganizedCaseJob>, Handlers.CreateGetOrganizedCase.CreateGetOrganizedCaseJobHandler>();
        services.AddTransient<IJobHandler<CreateGoToFilArkivQueueItemJob>, CreateToFilArkivQueueItemJobHandler>();
        services.AddTransient<IJobHandler<CreateJournalizeEverythingQueueItemJob>, CreateJournalizeEverythingQueueItemJobHandler>();
        services.AddTransient<IJobHandler<CreateToSharepointQueueItemJob>, CreateToSharepointQueueItemJobHandler>();
        services.AddTransient<IJobHandler<RegisterPodioCaseJob>, RegisterPodioCaseJobHandler>();
        services.AddTransient<IJobHandler<CheckOCRScreeningStatusJob>, RegisterFilesJobHandler>();

        services.AddTransient<DeskproHelper>();
        services.AddSingleton<Handlers.AddOrUpdateDeskproTicketToGetOrganized.PendingsTickets>();

        return services;
    }

    public static IServiceCollection AddJobHandlersModule(this IServiceCollection services, IConfiguration configuration)
    {
        // FilArkiv client
        var filArkivUrl = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:BaseAddress"));
        var filArkivClientId = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:ClientId"));
        var filArkivClientSecret = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:ClientSecret"));
        services.AddFilArkivApiClient(filArkivUrl, filArkivClientId, filArkivClientSecret);

        services.AddSingleton<CachedData>();
        services.AddSingleton<CheckOCRScreeningStatusSettings>();

        return services;
    }

}
