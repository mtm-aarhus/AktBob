using AktBob.PodioHookProcessor.UseCases;
using AktBob.Shared.Contracts;
using AktBob.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AktBob.JobHandlers.Handlers;
using AktBob.JobHandlers.Utils;
using AktBob.JournalizeDocuments.BackgroundServices;

namespace AktBob.JobHandlers;
public static class RegisterJobHandlers
{
    public static IServiceCollection AddJobHandlers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IJobHandler<CreateDocumentListQueueItemJob>, CreateDocumentListQueueItemJobHandler>();
        services.AddTransient<IJobHandler<CreateGoToFilArkivQueueItemJob>, CreateGoToFilArkivQueueItemJobHandler>();
        services.AddTransient<IJobHandler<CreateJournalizeEverythingQueueItemJob>, CreateJournalizeEverythingQueueItemJobHandler>();
        services.AddTransient<IJobHandler<RegisterPodioCaseJob>, RegisterPodioCaseJobHandler>();
        services.AddTransient<IJobHandler<CreateToSharepointQueueItemJob>, CreateToSharepointQueueItemJobHandler>();

        services.AddHostedService<AddMessagesToGetOrganizedBackgroundJobHandler>();

        services.AddTransient<DeskproHelper>();

        return services;
    }

}
