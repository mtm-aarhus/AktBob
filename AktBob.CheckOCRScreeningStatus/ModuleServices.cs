using Microsoft.Extensions.DependencyInjection;
using FilArkivCore.Web.Client;
using AktBob.Shared;
using AktBob.Shared.Contracts;
using AktBob.CheckOCRScreeningStatus.Jobs;

namespace AktBob.CheckOCRScreeningStatus;

public static class ModuleServices
{
    public static IServiceCollection AddCheckOCRScreeningStatusModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IJobHandler<CheckOCRScreeningStatusJob>, RegisterFilesJobHandler>();
        services.AddSingleton<CachedData>();

        // FilArkiv client
        var filArkivUrl = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:BaseAddress"));
        var filArkivClientId = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:ClientId"));
        var filArkivClientSecret = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:ClientSecret"));
        services.AddFilArkivApiClient(filArkivUrl, filArkivClientId, filArkivClientSecret);

        return services;
    }
}
