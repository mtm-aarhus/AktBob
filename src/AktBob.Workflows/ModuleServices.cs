using AktBob.Shared.Jobs;
using AktBob.Workflows.Processes.AddOrUpdateDeskproTicketToGetOrganized;
using AktBob.Workflows.Processes.AddMessageToGetOrganized;
using AktBob.Workflows.Processes;

namespace AktBob.Workflows;
public static class ModuleServices
{
    public static IServiceCollection AddWorkflowJobs(this IServiceCollection services, IConfiguration configuration)
    {
        // AddMessageToGetOrganized
        services.AddScoped<IJobHandler<AddMessageToGetOrganizedJob>, AddMessageToGetOrganized>();
        services.AddScoped<IJobHandler<ProcessMessageAttachmentsJob>, ProcessMessageAttachments>();
        services.AddScoped<IJobHandler<RegisterMessagesJob>, RegisterMessages>();

        // Other workflows
        services.AddScoped<IJobHandler<AddOrUpdateDeskproTicketToGetOrganizedJob>, AddOrUpdateDeskproTicketToGetOrganized>();
        services.AddScoped<IJobHandler<CreateGetOrganizedCaseJob>, CreateGetOrganizedCase>();
        services.AddScoped<IJobHandler<UpdateDeskproSetGetOrganizedAggregatedCaseNumbersJob>, UpdateDeskproSetGetOrganizedAggregatedCaseNumbers>();
        services.AddScoped<IJobHandler<UpdateDeskproSetFærdigbehandletDatoFieldJob>, UpdateDeskproSetFærdigbehandletDatoField>();
        services.AddScoped<IJobHandler<UpdateGetOrganizedCaseSetKleValueJob>, UpdateGetOrganizedCaseSetKleValue>();
        
        return services;
    }

}
