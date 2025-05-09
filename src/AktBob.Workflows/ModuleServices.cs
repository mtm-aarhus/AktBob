using FilArkivCore.Web.Client;
using AktBob.Workflows.Processes.AddOrUpdateDeskproTicketToGetOrganized;
using AktBob.Shared.Jobs;
using AktBob.Workflows.Processes.AddMessageToGetOrganized;
using AktBob.Workflows.Processes;
using AktBob.Workflows.Processes.CheckOCRScreeningStatus;
using AktBob.Workflows.Processes.CreateDocumentListQueueItem;
using AktBob.Workflows.Processes.Cleanup;
using Hangfire.Sagas;

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

        // AddOrUpdateDeskproTicketToGetOrganized
        services.AddScoped<IJobHandler<AddOrUpdateDeskproTicketToGetOrganizedJob>, AddOrUpdateDeskproTicketToGetOrganized>();

        // Other workflows
        services.AddScoped<IJobHandler<CreateAfgørelsesskrivelseQueueItemJob>, CreateAfgørelsesskrivelseQueueItem>();
        services.AddScoped<IJobHandler<CreateDocumentListQueueItemJob>, CreateDocumentListQueueItem>();
        services.AddScoped<IJobHandler<CreateGetOrganizedCaseJob>, CreateGetOrganizedCase>();
        services.AddScoped<IJobHandler<CreateGoToFilArkivQueueItemJob>, CreateToFilArkivQueueItem>();
        services.AddScoped<IJobHandler<CreateJournalizeEverythingQueueItemJob>, CreateJournalizeEverythingQueueItem>();
        services.AddScoped<IJobHandler<CreateToSharepointQueueItemJob>, CreateToSharepointQueueItem>();
        services.AddScoped<IJobHandler<RegisterPodioCaseJob>, RegisterPodioCase>();
        services.AddScoped<IJobHandler<UpdateDeskproSetGetOrganizedAggregatedCaseNumbersJob>, UpdateDeskproSetGetOrganizedAggregatedCaseNumbers>();
        services.AddScoped<IJobHandler<RegisterOS2FormsSubmissionJob>, RegisterOS2FormsSubmission>();
        services.AddScoped<IJobHandler<DispatchCleanupJobsJob>, DispatchCleanupJobs>();
        services.AddScoped<IJobHandler<NotitfyAboutUpcomingCleanupJob>, NotifyAboutUpcomingCleanup>();
        services.AddScoped<IJobHandler<CreateCleanupFilArkivQueueItemJob>, CreateCleanupFilArkivQueueItem>();
        services.AddScoped<IJobHandler<CreateCleanupSharepointQueueItemJob>, CreateCleanupSharepointQueueItem>();
        services.AddScoped<IJobHandler<RegisterCleanedUpFilArkivFileJob>, RegisterCleanedUpFilArkivFile>();
        services.AddScoped<IJobHandler<UpdateDeskproSetFærdigbehandletDatoFieldJob>, UpdateDeskproSetFærdigbehandletDatoField>();
        services.AddScoped<IJobHandler<ScreeningIsFinishedNotificationJob>, ScreeningIsFinishedNotification>();

        // Hangfire.Sagas
        //var connectionString = Guard.Against.NullOrEmpty(configuration.GetConnectionString("Hangfire"));
        //services.AddHangfireSagas(connectionString, [

        //    ]);

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
