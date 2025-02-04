using AAK.GetOrganized;
using AktBob.JournalizeDocuments.BackgroundServices;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.JournalizeDocuments;
public static class ModuleServices
{
    public static IServiceCollection AddJournalizeDocumentsModule(this IServiceCollection services, IConfiguration configuration)
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
        services.AddHostedService<JournalizeFullTicketDocumentBackgroundService>();

        services.AddTransient<DeskproHelper>();
        services.AddTransient<GetOrganizedHelper>();

        return services;
    }
}
