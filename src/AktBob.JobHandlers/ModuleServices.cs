using AktBob.PodioHookProcessor.UseCases;
using AktBob.JobHandlers.Handlers;
using AktBob.JobHandlers.Utils;
using AktBob.JobHandlers.Handlers.AddMessageToGetOrganized;
using FilArkivCore.Web.Client;
using AktBob.JobHandlers.Handlers.CheckOCRScreeningStatus;
using AktBob.JobHandlers.Handlers.AddOrUpdateDeskproTicketToGetOrganized;
using AktBob.Shared.Jobs;

namespace AktBob.JobHandlers;
public static class ModuleServices
{
    public static IServiceCollection AddJobHandlers(this IServiceCollection services, IConfiguration configuration)
    {
        // JOBS

        // AddMessagesToGetOrganized workflow
        services.AddScoped<IJobHandler<AddMessageToGetOrganizedJob>, AddMessageToGetOrganized>();
        services.AddScoped<IJobHandler<ProcessMessageAttachmentsJob>, ProcessMessageAttachments>();
        services.AddScoped<IJobHandler<RegisterMessagesJob>, RegisterMessages>();

        // AddOrUpdateDeskproTicketToGetOrganized workflow
        services.AddScoped<IJobHandler<AddOrUpdateDeskproTicketToGetOrganizedJob>, AddOrUpdateDeskproTicketToGetOrganized>();
        services.AddSingleton<PendingsTickets>();

        services.AddScoped<IJobHandler<QueryFilesProcessingStatusJob>, QueryFilesProcessingStatus>();
        services.AddScoped<IJobHandler<CheckOCRScreeningStatusRegisterFilesJob>, CheckOCRScreeningStatusRegisterFiles>();
        services.AddScoped<IJobHandler<CreateAfgørelsesskrivelseQueueItemJob>, CreateAfgørelsesskrivelseQueueItem>();
        services.AddScoped<IJobHandler<CreateDocumentListQueueItemJob>, CreateDocumentListQueueItem>();
        services.AddScoped<IJobHandler<CreateGetOrganizedCaseJob>, CreateGetOrganizedCase>();
        services.AddScoped<IJobHandler<CreateGoToFilArkivQueueItemJob>, CreateToFilArkivQueueItem>();
        services.AddScoped<IJobHandler<CreateJournalizeEverythingQueueItemJob>, CreateJournalizeEverythingQueueItem>();
        services.AddScoped<IJobHandler<CreateToSharepointQueueItemJob>, CreateToSharepointQueueItem>();
        services.AddScoped<IJobHandler<RegisterPodioCaseJob>, RegisterPodioCase>();
        services.AddScoped<IJobHandler<CheckOCRScreeningStatusRegisterFilesJob>, CheckOCRScreeningStatusRegisterFiles>();

        services.AddScoped<DeskproHelper>();

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
        services.AddSingleton<Settings>();

        return services;
    }

}
