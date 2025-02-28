using AktBob.CloudConvert.UseCases;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AktBob.CloudConvert;
public static class ModuleServices
{
    public static IServiceCollection AddCloudConvertModule(this IServiceCollection services, IConfiguration configuration, List<Assembly> cqrsHandlersAssemblies)
    {
        var cloudConvertBaseUrl = Guard.Against.NullOrEmpty(configuration.GetValue<string>("CloudConvert:BaseUrl"));
        var cloudConvertToken = Guard.Against.NullOrEmpty(configuration.GetValue<string>("CloudConvert:Token"));

        services.AddHttpClient(Constants.CLOUDCONVERT_HTTPCLIENT_NAME, client =>
        {
            client.BaseAddress = new Uri(cloudConvertBaseUrl);
        });

        services.AddTransient<ICloudConvertClient>(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<CloudConvertClient>>();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient(Constants.CLOUDCONVERT_HTTPCLIENT_NAME);

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + cloudConvertToken);

            return new CloudConvertClient(client, logger);
        });

        cqrsHandlersAssemblies.Add(typeof(ModuleServices).Assembly);

        return services;
    }
}
