using AAK.OS2Forms;
using Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Contracts;
using Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Handlers.GetSubmission;
using Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Handlers.GetSubmissions;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule;

public static class RegisterServices
{
    public static IServiceCollection AddOS2FormsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = Guard.Against.NullOrEmpty(configuration.GetValue<string>("BaseAddress"));
        var apiKey = Guard.Against.NullOrEmpty(configuration.GetValue<string>("ApiKey"));

        services.AddOS2Forms(baseUrl, apiKey);
        services.AddScoped<IOS2FormsModule, OS2FormsSubmissions.OS2FormsModule.OS2FormsModule>();
        services.AddGetSubmissionHandler();
        services.AddGetSubmissionsHandler();
        
        return services;
    }
}
