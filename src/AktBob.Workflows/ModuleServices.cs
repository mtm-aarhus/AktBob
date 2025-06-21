using AktBob.Shared.Jobs;
using AktBob.Workflows.Processes.AddOrUpdateDeskproTicketToGetOrganized;
using AktBob.Workflows.Processes.AddMessageToGetOrganized;
using AktBob.Workflows.Processes;
using AktBob.Workflows.Processes.CheckOCRScreeningStatus;
using AktBob.Workflows.Processes.Cleanup;

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
        services.AddScoped<IJobHandler<QueryFileProcessingStatusJob>, QueryFileProcessingStatus>();
        services.AddScoped<IJobHandler<ScreeningIsFinishedEmailNotificationJob>, ScreeningIsFinishedEmailNotification>();
        services.AddScoped<IJobHandler<ScreeningIsFinishedPodioNotificationJob>, ScreeningIsFinishedPodioNotification>();
        services.AddScoped<IJobHandler<UpdatePodioFilArkivFieldsJob>, UpdatePodioFilArkivFields>();

        // Other workflows
        services.AddScoped<IJobHandler<AddOrUpdateDeskproTicketToGetOrganizedJob>, AddOrUpdateDeskproTicketToGetOrganized>();
        services.AddScoped<IJobHandler<CreateGetOrganizedCaseJob>, CreateGetOrganizedCase>();
        services.AddScoped<IJobHandler<UpdateDeskproSetGetOrganizedAggregatedCaseNumbersJob>, UpdateDeskproSetGetOrganizedAggregatedCaseNumbers>();
        services.AddScoped<IJobHandler<DispatchCleanupJobsJob>, DispatchCleanupJobs>();
        services.AddScoped<IJobHandler<NotitfyAboutUpcomingCleanupJob>, NotifyAboutUpcomingCleanup>();
        services.AddScoped<IJobHandler<CreateCleanupFilArkivQueueItemJob>, CreateCleanupFilArkivQueueItem>();
        services.AddScoped<IJobHandler<CreateCleanupSharepointQueueItemJob>, CreateCleanupSharepointQueueItem>();
        services.AddScoped<IJobHandler<UpdateDeskproSetFærdigbehandletDatoFieldJob>, UpdateDeskproSetFærdigbehandletDatoField>();
        services.AddScoped<IJobHandler<UpdateGetOrganizedCaseSetKleValueJob>, UpdateGetOrganizedCaseSetKleValue>();
        
        return services;
    }

}
