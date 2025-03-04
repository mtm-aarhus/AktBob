using AktBob.JobHandlers.Utils;
using FilArkivCore.Web.Client;
using AktBob.JobHandlers.Processes.AddOrUpdateDeskproTicketToGetOrganized;
using AktBob.JobHandlers.Processes.CheckOCRScreeningStatus;
using AktBob.Shared.Jobs;
using AktBob.JobHandlers.Processes.AddMessageToGetOrganized;
using AktBob.JobHandlers.Processes;

namespace AktBob.JobHandlers;
public static class ModuleServices
{
    public static IServiceCollection AddJobHandlers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IJobHandler<RegisterMessagesJob>, RegisterMessages>();
        services.AddTransient<IJobHandler<AddOrUpdateDeskproTicketToGetOrganizedJob>, AddOrUpdateDeskproTicketToGetOrganized>();
        services.AddTransient<IJobHandler<CreateAfgørelsesskrivelseQueueItemJob>, CreateAfgørelsesskrivelseQueueItem>();
        services.AddTransient<IJobHandler<CreateDocumentListQueueItemJob>, CreateDocumentListQueueItem>();
        services.AddTransient<IJobHandler<CreateGetOrganizedCaseJob>, CreateGetOrganizedCase>();
        services.AddTransient<IJobHandler<CreateGoToFilArkivQueueItemJob>, CreateToFilArkivQueueItem>();
        services.AddTransient<IJobHandler<CreateJournalizeEverythingQueueItemJob>, CreateJournalizeEverythingQueueItem>();
        services.AddTransient<IJobHandler<CreateToSharepointQueueItemJob>, CreateToSharepointQueueItem>();
        services.AddTransient<IJobHandler<RegisterPodioCaseJob>, RegisterPodioCase>();
        services.AddTransient<IJobHandler<CheckOCRScreeningStatusRegisterFilesJob>, CheckOCRScreeningStatusRegisterFiles>();

        services.AddTransient<DeskproHelper>();
        services.AddSingleton<PendingsTickets>();

        return services;
    }

    public static IServiceCollection AddJobHandlersModule(this IServiceCollection services, IConfiguration configuration)
    {
        // FilArkiv client
        var filArkivUrl = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:BaseAddress"));
        var filArkivClientId = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:ClientId"));
        var filArkivClientSecret = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:ClientSecret"));
        services.AddFilArkivApiClient(filArkivUrl, filArkivClientId, filArkivClientSecret);

        services.AddSingleton<Processes.CheckOCRScreeningStatus.CachedData>();
        services.AddSingleton<Processes.CheckOCRScreeningStatus.Settings>();

        return services;
    }

}
