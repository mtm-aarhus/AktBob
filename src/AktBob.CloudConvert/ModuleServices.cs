using AktBob.CloudConvert.Contracts;
using AktBob.CloudConvert.Handlers.ConvertHtmlToPdf;
using AktBob.CloudConvert.Handlers.DownloadFile;
using AktBob.CloudConvert.Handlers.GenerateTasks;
using AktBob.CloudConvert.Handlers.GetDownloadUrl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.CloudConvert;
public static class ModuleServices
{
    public static IServiceCollection AddCloudConvertModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Add CloudConvert client
        var cloudConvertBaseUrl = Guard.Against.NullOrEmpty(configuration.GetValue<string>("CloudConvert:BaseUrl"));
        var cloudConvertToken = Guard.Against.NullOrEmpty(configuration.GetValue<string>("CloudConvert:Token"));

        services.AddHttpClient(Constants.CLOUDCONVERT_HTTPCLIENT_NAME, client =>
        {
            client.BaseAddress = new Uri(cloudConvertBaseUrl);
        });

        services.AddScoped<ICloudConvertClient>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient(Constants.CLOUDCONVERT_HTTPCLIENT_NAME);

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + cloudConvertToken);

            return new CloudConvertClient(client);
        });

        // Add handlers
        services.AddConvertHtmlToPdfHandler();
        services.AddDownloadFileHandler();
        services.AddGenerateTasksHandler();
        services.AddGetDownloadUrlHandler();

        // Module service orchestration
        services.AddScoped<ICloudConvertModule, CloudConvertModule>();

        return services;
    }
}