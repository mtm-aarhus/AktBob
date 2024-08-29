using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AktBob.DatabaseAPI;

public static class ModuleServices
{
    public static IServiceCollection AddDatabaseApiModule(this IServiceCollection services, IConfiguration configuration, List<Assembly> mediatRAssemblies)
    {
        services.AddTransient<IDatabaseApi, DatabaseApi>();
        services.AddHttpClient<IDatabaseApi, DatabaseApi>(client =>
        {
            client.BaseAddress = new Uri(Guard.Against.NullOrEmpty(configuration.GetValue<string>("DatabaseApi:Url")));
            client.DefaultRequestHeaders.Add("ApiKey", Guard.Against.NullOrEmpty(configuration.GetValue<string>("DatabaseApi:ApiKey")));
        });

        mediatRAssemblies.Add(typeof(ModuleServices).Assembly);

        return services;
    }
}
