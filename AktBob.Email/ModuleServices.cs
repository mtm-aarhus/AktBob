using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Email;

public static class ModuleServices
{
    public static IServiceCollection AddEmailModuleServices(this IServiceCollection services, IConfiguration configuration, List<Type> mediatorHandlers)
    {
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:From"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:Smtp"));

        mediatorHandlers.Add(typeof(SendEmailCommandHandler));

        return services;
    }
}
