using AAK.GetOrganized;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AktBob.GetOrganized;
public static class ModuleServices
{
    public static IServiceCollection AddGetOrganizedModule(this IServiceCollection services, IConfiguration configuration, List<Assembly> cqrsHandlersAssemblies)
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

        cqrsHandlersAssemblies.Add(typeof(ModuleServices).Assembly);

        return services;
    }
}
