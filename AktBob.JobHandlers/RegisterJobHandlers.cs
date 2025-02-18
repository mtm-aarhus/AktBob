using AktBob.PodioHookProcessor.UseCases;
using AktBob.Shared.Contracts;
using AktBob.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AktBob.JobHandlers.Handlers;
using AktBob.JobHandlers.Utils;
using AktBob.JobHandlers.Handlers.AddMessagesToGetOrganized;

namespace AktBob.JobHandlers;
public static class RegisterJobHandlers
{
    public static IServiceCollection AddJobHandlers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IJobHandler<AddOrUpdateDeskproTicketToGetOrganizedJob>, Handlers.AddOrUpdateDeskproTicketToGetOrganized.AddOrUpdateDeskproTicketToGetOrganizedJobHandler>();
        services.AddTransient<IJobHandler<CreateAfgørelsesskrivelseQueueItemJob>, CreateAfgørelsesskrivelseQueueItemJobHandler>();
        services.AddTransient<IJobHandler<CreateDocumentListQueueItemJob>, CreateDocumentListQueueItemJobHandler>();
        services.AddTransient<IJobHandler<CreateGetOrganizedCaseJob>, Handlers.CreateGetOrganizedCase.CreateGetOrganizedCaseJobHandler>();
        services.AddTransient<IJobHandler<CreateGoToFilArkivQueueItemJob>, CreateToFilArkivQueueItemJobHandler>();
        services.AddTransient<IJobHandler<CreateJournalizeEverythingQueueItemJob>, CreateJournalizeEverythingQueueItemJobHandler>();
        services.AddTransient<IJobHandler<CreateToSharepointQueueItemJob>, CreateToSharepointQueueItemJobHandler>();
        services.AddTransient<IJobHandler<RegisterPodioCaseJob>, RegisterPodioCaseJobHandler>();

        services.AddHostedService<AddMessagesToGetOrganizedBackgroundJob>();

        services.AddTransient<DeskproHelper>();
        services.AddSingleton<Handlers.AddOrUpdateDeskproTicketToGetOrganized.PendingsTickets>();

        return services;
    }

}
