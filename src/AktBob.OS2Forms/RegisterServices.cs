using AAK.OS2Forms;
using AktBob.OS2Forms.Contracts;
using AktBob.OS2Forms.Handlers.GetSubmission;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.OS2Forms;
public static class RegisterServices
{
    public static IServiceCollection AddOS2FormsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = Guard.Against.NullOrEmpty(configuration.GetValue<string>("OS2Forms:BaseUrl"));
        var apiKey = Guard.Against.NullOrEmpty(configuration.GetValue<string>("OS2Forms:ApiKey"));

        services.AddOS2Forms(baseUrl, apiKey);
        services.AddScoped<IOS2FormsModule, OS2FormsModule>();
        services.AddGetSubmissionHandler();
        
        return services;
    }
}
