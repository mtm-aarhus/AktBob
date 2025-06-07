using Aktbob.Modules.OpenOrchestrator.Client;
using Aktbob.Modules.OpenOrchestrator.Features;
using Ardalis.GuardClauses;

namespace Aktbob.Modules.OpenOrchestrator;

internal static class RegisterModuleServices
{
    public static IServiceCollection AddModuleServices(this IServiceCollection services, IConfiguration configuration)
    {
        var baseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>("BaseAddress"));
        var apiKey = Guard.Against.NullOrEmpty(configuration.GetValue<string>("ApiKey"));
        
        services.AddScoped<OpenOrchestratorClient>();
        services.AddHttpClient<OpenOrchestratorClient>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        });

        services.AddAddQueueHandler();
        
        return services;
    }
}