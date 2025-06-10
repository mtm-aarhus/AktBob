using AktBob.Shared.ModuleClients.DeskproModule;
using AktBob.Shared.ModuleClients.OpenOrchestratorModule;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;

namespace AktBob.Shared.ModuleClients;

public static class RegisterModuleClients
{
    public static IServiceCollection AddDeskproModuleClient(this IServiceCollection services, IConfiguration configuration)
    {
        const string moduleHttpClientName = "deskpro-module-http-client";
        var baseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Modules:Deskpro"));
        
        services.AddHttpClient(moduleHttpClientName, client =>
        {
            client.BaseAddress = new Uri(baseAddress);
        });
        
        services.AddScoped<IDeskproModuleClient>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(moduleHttpClientName);
            
            var inner = new DeskproModuleClient(httpClient);
            var withLogging = new DeskproModuleClientLogging(inner, provider.GetRequiredService<ILogger<DeskproModuleClient>>());
            return withLogging;
        });
        
        return services;
    }
    
    public static IServiceCollection AddOpenOrchestratorModuleClient(this IServiceCollection services, IConfiguration configuration)
    {
        const string moduleHttpClientName = "open-orchestrator-module-http-client";
        
        var baseAddress = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Modules:OpenOrchestrator"));
        
        services.AddHttpClient(moduleHttpClientName, client =>
        {
            client.BaseAddress = new Uri(baseAddress);
        });
        
        services.AddScoped<IOpenOrchestratorModuleClient>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(moduleHttpClientName);
            
            var inner = new OpenOrchestratorModuleClient(httpClient);
            var withLogging = new OpenOrchestratorModuleClientLogging(inner, provider.GetRequiredService<ILogger<OpenOrchestratorModuleClient>>());
            return withLogging;
        });
        
        return services;
    }
}