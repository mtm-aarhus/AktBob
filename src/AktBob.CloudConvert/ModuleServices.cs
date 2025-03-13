using AktBob.CloudConvert.Handlers;
using AktBob.Email.Contracts;
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
            var logger = serviceProvider.GetRequiredService<ILogger<CloudConvertClient>>();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient(Constants.CLOUDCONVERT_HTTPCLIENT_NAME);

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + cloudConvertToken);

            return new CloudConvertClient(client, logger);
        });

        // Add module handlers
        services.AddScoped<IConvertHtmlToPdfHandler, ConvertHtmlToPdfHandler>();
        services.AddScoped<IGenerateTasksHandler, GenerateTasksHandler>();
        services.AddScoped<IGettDownloadUrlHandler, GetDownloadUrlHandler>();
        services.AddScoped<IDownloadFileHandler, DownloadFileHandler>();

        // Module service orchestration
        services.AddScoped<ICloudConvertModule>(provider =>
        {
            var inner = new CloudConvertModule(
                provider.GetRequiredService<IConvertHtmlToPdfHandler>(),
                provider.GetRequiredService<IGettDownloadUrlHandler>(),
                provider.GetRequiredService<IDownloadFileHandler>(),
                provider.GetRequiredService<IGenerateTasksHandler>());

            var withLogging = new ModuleLoggingDecorator(
                inner,
                provider.GetRequiredService<ILogger<CloudConvertModule>>());

            var withExceptionHandling = new ModuleExceptionDecorator(
                withLogging,
                provider.GetRequiredService<ILogger<CloudConvertModule>>());

            return withExceptionHandling;
        });

        return services;
    }
}