using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ardalis.GuardClauses;

namespace AktBob.Shared.ModuleClients;

public static class RegisterModuleClients
{
    public static IServiceCollection AddDeskproModuleClient(this IServiceCollection services, IConfiguration configuration)
    {
        var baseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Modules:Deskpro"));
        
        services.AddScoped<DeskproModuleClient>();
        services.AddHttpClient<DeskproModuleClient>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
        });
        
        return services;
    }
    
    public static IServiceCollection AddOpenOrchestratorModuleClient(this IServiceCollection services, IConfiguration configuration)
    {
        var baseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Modules:OpenOrchestrator"));
        
        services.AddScoped<OpenOrchestratorModuleClient>();
        services.AddHttpClient<OpenOrchestratorModuleClient>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
        });
        
        return services;
    }
}