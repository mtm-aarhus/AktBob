using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts;
using AktBob.GetOrganized.Handlers.CreateCase;
using AktBob.GetOrganized.Handlers.FinalizeDocument;
using AktBob.GetOrganized.Handlers.GetAggregatedCase;
using AktBob.GetOrganized.Handlers.GetCaseMetadata;
using AktBob.GetOrganized.Handlers.RelateDocuments;
using AktBob.GetOrganized.Handlers.UpdateCaseMetadata;
using AktBob.GetOrganized.Handlers.UploadDocument;
using AktBob.GetOrganized.Jobs;
using AktBob.Shared;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.GetOrganized;
public static class ModuleServices
{
    public static IServiceCollection AddGetOrganizedModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Add GetOrganized service
        var getOrganizedOptions = new GetOrganizedOptions
        {
            BaseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>($"GetOrganized:BaseAddress")),
            Domain = Guard.Against.NullOrEmpty(configuration.GetValue<string>($"GetOrganized:Domain")),
            UserName = Guard.Against.NullOrEmpty(configuration.GetValue<string>($"GetOrganized:Username")),
            Password = Guard.Against.NullOrEmpty(configuration.GetValue<string>($"GetOrganized:Password"))
        };

        services.AddGetOrganized(getOrganizedOptions);

        // Handlers
        services.AddCreateCaseHandler();
        services.AddFinalizeDocumentHandler();
        services.AddGetAggregatedCaseHandler();
        services.AddGetCaseMetadataHandler();
        services.AddRelateDocumentsHandler();
        services.AddUpdateCaseMetadataHandler();
        services.AddUploadDocumentHandler();
        
        // Jobs
        services.AddScoped<IJobHandler<FinalizeDocumentJob>, FinalizeDocument>();

        // Module Service orchestration
        services.AddScoped<IGetOrganizedModule, GetOrganizedModule>();

        return services;
    }
}
