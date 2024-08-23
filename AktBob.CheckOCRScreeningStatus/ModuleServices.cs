using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FilArkivCore.Web.Client;
using Microsoft.Extensions.Configuration;
using Ardalis.GuardClauses;
using AAK.Deskpro;

namespace AktBob.CheckOCRScreeningStatus;
public static class ModuleServices
{
    public static IServiceCollection AddCheckOCRScreeningStatusModule(this IServiceCollection services, IConfiguration configuration, List<Assembly> mediatRAssemblies)
    {
        Guard.Against.NullOrEmpty(configuration.GetSection("Deskpro:PodioItemIdFields").Get<int[]>());
        Guard.Against.Null(configuration.GetValue<int>("Podio:AppId"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("CheckOCRScreeningStatus:QueueName"));
        Guard.Against.NullOrEmpty(configuration.GetConnectionString("AzureStorage"));


        services.AddHostedService<BackgroundServices.Worker>();

        services.AddSingleton<IData, Data>();

        var filArkivUrl = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:BaseAddress"));
        var filArkivClientId = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:ClientId"));
        var filArkivClientSecret = Guard.Against.NullOrEmpty(configuration.GetValue<string>("FilArkiv:ClientSecret"));
        services.AddFilArkivApiClient(filArkivUrl, filArkivClientId, filArkivClientSecret);

        services.AddScoped<IFilArkiv, FilArkiv>();

        services.AddTransient<IAktBobApi, AktBobApi>();
        services.AddHttpClient<IAktBobApi, AktBobApi>(client =>
        {
            var apiBaseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>("AktBobApi:BaseAddress"));
            var apiApiKey = Guard.Against.NullOrEmpty(configuration.GetValue<string>("AktBobApi:ApiKey"));

            client.BaseAddress = new Uri(apiBaseAddress);
            client.DefaultRequestHeaders.Add("ApiKey", apiApiKey);
        });

        var deskproOptions = new DeskproOptions
        {
            BaseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Deskpro:BaseAddress")),
            AuthorizationKey = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Deskpro:AuthorizationKey"))
        };

        services.AddDeskpro(deskproOptions);

        mediatRAssemblies.Add(typeof(ModuleServices).Assembly);
        return services;
    }
}
