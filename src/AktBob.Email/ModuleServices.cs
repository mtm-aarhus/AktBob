using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AktBob.Email;

public static class ModuleServices
{
    public static IServiceCollection AddEmailModuleServices(this IServiceCollection services, IConfiguration configuration, List<Assembly> cqrsHandlersAssemblies)
    {
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:From"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:Smtp"));

        cqrsHandlersAssemblies.Add(typeof(ModuleServices).Assembly);

        return services;
    }
}
