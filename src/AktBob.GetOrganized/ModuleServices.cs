using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts;
using AktBob.GetOrganized.Contracts.Jobs;
using AktBob.GetOrganized.Handlers;
using AktBob.GetOrganized.JobHandlers;
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
        services.AddTransient<ICreateGetOrganizedCaseHandler, CreateGetOrganizedCaseHandler>();
        services.AddTransient<IFinalizeGetOrganizedDocumentHandler, FinalizeGetOrganizedDocumentHandler>();
        services.AddTransient<IGetOrganizedHandlers, GetOrganizedHandlers>();
        services.AddTransient<IRelateGetOrganizedDocumentsHandler, RelateGetOrganizedDocumentsHandler>();
        services.AddTransient<IUploadGetOrganizedDocumentHandler, UploadGetOrganizedDocumenHandler>();

        // Jobs
        services.AddScoped<IJobHandler<FinalizeDocumentJob>, FinalizeDocument>();

        return services;
    }
}
