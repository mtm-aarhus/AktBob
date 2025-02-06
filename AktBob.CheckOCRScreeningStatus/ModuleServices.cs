using Microsoft.Extensions.DependencyInjection;
using FilArkivCore.Web.Client;
using AktBob.CheckOCRScreeningStatus.UseCases;
using AktBob.Shared;
using AktBob.Shared.Contracts;
using AktBob.CheckOCRScreeningStatus.JobHandlers;

namespace AktBob.CheckOCRScreeningStatus;

public static class ModuleServices
{
    public static IServiceCollection AddCheckOCRScreeningStatusModule(this IServiceCollection services, IConfiguration configuration, List<Type> mediatorHandlers)
    {
        // Guard against missing Podio configuration
        var podioAppId = Guard.Against.Null(configuration.GetValue<int>("Podio:AppId"));
        var podioFields = Guard.Against.Null(Guard.Against.NullOrEmpty(configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => int.Parse(x.Key), x => x.Get<PodioField>())));
        var podioFieldFilArkivCaseId = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value.AppId == podioAppId && x.Value.Label == "FilArkivCaseId"));
        Guard.Against.Null(podioFieldFilArkivCaseId.Value);

        var podioFieldFilArkivLink = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value.AppId == podioAppId && x.Value.Label == "FilArkivLink"));
        Guard.Against.Null(podioFieldFilArkivLink.Value);



        // FilArkiv client
        var filArkivUrl = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:BaseAddress"));
        var filArkivClientId = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:ClientId"));
        var filArkivClientSecret = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:ClientSecret"));
        services.AddFilArkivApiClient(filArkivUrl, filArkivClientId, filArkivClientSecret);
        services.AddScoped<IFilArkiv, FilArkiv>();


        mediatorHandlers.AddRange([
            typeof(GetFileStatusQueryHandler),
            typeof(RegisterFilesCommandHandler),
            typeof(RemoveCaseFromCacheCommandHandler),
            typeof(UpdateDatabaseCommandHandler),
            typeof(UpdatePodioItemCommandHandler)]);

        services.AddTransient<IJobHandler<CheckOCRScreeningStatusJob>, CheckOCRScreeningStatusJobHandler>();
        services.AddSingleton<IData, Data>();

        return services;
    }
}
