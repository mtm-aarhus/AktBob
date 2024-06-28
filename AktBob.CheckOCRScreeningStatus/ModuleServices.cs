using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FilArkivCore.Web.Client;
using Microsoft.Extensions.Configuration;
using Ardalis.GuardClauses;
using AktBob.CreateOCRScreeningStatus.ExternalQueue;
using System.Net.Http.Headers;

namespace AktBob.CheckOCRScreeningStatus;
public static class ModuleServices
{
    public static IServiceCollection AddCheckOCRScreeningStatusModule(this IServiceCollection services, IConfiguration configuration, List<Assembly> mediatRAssemblies)
    {
        services.AddSingleton<IData, Data>();

        var filArkivUrl = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:BaseAddress"));
        var filArkivClientId = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:ClientId"));
        var filArkivClientSecret = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:ClientSecret"));
        services.AddFilArkivApiClient(filArkivUrl, filArkivClientId, filArkivClientSecret);

        Guard.Against.NullOrEmpty(configuration.GetValue<string>("AzureQueue:CheckOCRScreeningStatus:QueueName"));
        Guard.Against.NullOrEmpty(configuration.GetConnectionString("AzureStorage"));

        services.AddTransient<ICheckOCRScreeningStatusQueue, CheckOCRScreeningStatusQueue>();
        services.AddTransient<ICheckOCRScreeningStatusService, CheckOCRScreeningStatusService>();


        services.AddScoped<IFilArkiv, FilArkiv>();

        services.AddTransient<IAktBobApi, AktBobApi>();
        services.AddHttpClient<IAktBobApi, AktBobApi>(client =>
        {
            var apiBaseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>("AktBobApi:BaseAddress"));
            var apiApiKey = Guard.Against.NullOrEmpty(configuration.GetValue<string>("AktBobApi:ApiKey"));

            client.BaseAddress = new Uri(apiBaseAddress);
            client.DefaultRequestHeaders.Add("ApiKey", apiApiKey);
        });

        mediatRAssemblies.Add(typeof(ModuleServices).Assembly);
        return services;
    }
}
