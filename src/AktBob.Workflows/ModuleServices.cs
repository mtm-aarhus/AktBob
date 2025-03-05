using FilArkivCore.Web.Client;
using AktBob.Workflows.Processes.AddOrUpdateDeskproTicketToGetOrganized;
using AktBob.Shared.Jobs;
using AktBob.Workflows.Processes.AddMessageToGetOrganized;
using AktBob.Workflows.Processes;
using AktBob.Workflows.Processes.CheckOCRScreeningStatus;
using AktBob.Workflows.Helpers;

namespace AktBob.Workflows;
public static class ModuleServices
{
    public static IServiceCollection AddWorkflowJobs(this IServiceCollection services, IConfiguration configuration)
    {
        // AddMessageToGetOrganized
        services.AddScoped<IJobHandler<AddMessageToGetOrganizedJob>, AddMessageToGetOrganized>();
        services.AddScoped<IJobHandler<ProcessMessageAttachmentsJob>, ProcessMessageAttachments>();
        services.AddScoped<IJobHandler<RegisterMessagesJob>, RegisterMessages>();

        // CheckOCRScreeningStatus
        services.AddScoped<IJobHandler<CheckOCRScreeningStatusRegisterFilesJob>, CheckOCRScreeningStatusRegisterFiles>();
        services.AddScoped<IJobHandler<QueryFilesProcessingStatusJob>, QueryFilesProcessingStatus>();
        services.AddSingleton<Processes.CheckOCRScreeningStatus.CachedData>();
        services.AddSingleton<Settings>();

        // AddOrUpdateDeskproTicketToGetOrganized
        services.AddScoped<IJobHandler<AddOrUpdateDeskproTicketToGetOrganizedJob>, AddOrUpdateDeskproTicketToGetOrganized>();
        services.AddSingleton<PendingsTickets>();

        // Other workflows
        services.AddScoped<IJobHandler<CreateAfgørelsesskrivelseQueueItemJob>, CreateAfgørelsesskrivelseQueueItem>();
        services.AddScoped<IJobHandler<CreateDocumentListQueueItemJob>, CreateDocumentListQueueItem>();
        services.AddScoped<IJobHandler<CreateGetOrganizedCaseJob>, CreateGetOrganizedCase>();
        services.AddScoped<IJobHandler<CreateGoToFilArkivQueueItemJob>, CreateToFilArkivQueueItem>();
        services.AddScoped<IJobHandler<CreateJournalizeEverythingQueueItemJob>, CreateJournalizeEverythingQueueItem>();
        services.AddScoped<IJobHandler<CreateToSharepointQueueItemJob>, CreateToSharepointQueueItem>();
        services.AddScoped<IJobHandler<RegisterPodioCaseJob>, RegisterPodioCase>();
        services.AddScoped<IJobHandler<UpdateDeskproSetGetOrganizedAggregatedCaseNumbersJob>, UpdateDeskproSetGetOrganizedAggregatedCaseNumbers>();

        services.AddScoped<DeskproHelper>();

        return services;
    }

    public static IServiceCollection AddWorkflowModule(this IServiceCollection services, IConfiguration configuration)
    {
        // FilArkiv client
        var filArkivUrl = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:BaseAddress"));
        var filArkivClientId = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:ClientId"));
        var filArkivClientSecret = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:ClientSecret"));
        services.AddFilArkivApiClient(filArkivUrl, filArkivClientId, filArkivClientSecret);

       

        return services;
    }

}
