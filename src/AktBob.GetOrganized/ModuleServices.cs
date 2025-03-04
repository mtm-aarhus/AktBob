using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts;
using AktBob.GetOrganized.Handlers;
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

        services.AddGetOrganizedModule(getOrganizedOptions);

        // Handlers
        services.AddScoped<ICreateCaseHandler, CreateGetOrganizedCaseHandler>();
        services.AddScoped<IFinalizeDocumentHandler, FinalizeGetOrganizedDocumentHandler>();
        services.AddScoped<IRelateDocumentsHandler, RelateGetOrganizedDocumentsHandler>();
        services.AddScoped<IUploadDocumentHandler, UploadGetOrganizedDocumenHandler>();

        // Jobs
        services.AddScoped<IJobHandler<FinalizeDocumentJob>, FinalizeDocument>();

        // Module Service orchestration
        services.AddScoped<IGetOrganizedModule, Module>();

        return services;
    }
}
