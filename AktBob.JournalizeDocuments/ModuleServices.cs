using AAK.GetOrganized;
using AktBob.JournalizeDocuments.BackgroundServices;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AktBob.JournalizeDocuments;
public static class ModuleServices
{
    public static IServiceCollection AddJournalizeDocumentsModule(this IServiceCollection services, IConfiguration configuration, List<Assembly> mediatrAssemblies)
    {
        // Add GetOrganized service
        var getOrganizedOptions = new GetOrganizedOptions
        {
            BaseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>($"GetOrganized:BaseAddress")),
            Domain = Guard.Against.NullOrEmpty(configuration.GetValue<string>($"GetOrganized:Domain")),
            UserName = Guard.Against.NullOrEmpty(configuration.GetValue<string>($"GetOrganized:Username")),
            Password = Guard.Against.NullOrEmpty(configuration.GetValue<string>($"GetOrganized:Password"))
        };

        services.AddGetOrganizedModule(getOrganizedOptions);
        services.AddHostedService<JournalizeSingleMessagesBackgroundService>();

        
        services.AddHttpClient(Constants.DESKPRO_PDF_GENERATOR_HTTP_CLIENT_NAME, client =>
        {
            client.BaseAddress = new Uri(Guard.Against.NullOrEmpty(configuration.GetValue<string>("DeskproPdfGenerator:BaseUrl")));
            client.DefaultRequestHeaders.Add("ApiKey", Guard.Against.NullOrEmpty(configuration.GetValue<string>("DeskproPdfGenerator:ApiKey")));
        });

        mediatrAssemblies.Add(typeof(ModuleServices).Assembly);
        return services;
    }
}
