using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace AktBob.Email;

public static class ModuleServices
{
    public static IServiceCollection AddEmailModuleServices(
        this IServiceCollection services,
        IConfiguration configuration,
        List<System.Reflection.Assembly> mediatrAssemblies)
    {
        Guard.Against.Null(configuration.GetValue<int>("EmailModule:IntervalSeconds"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:From"));
        Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailModule:Smtp"));

        // Mediatr
        mediatrAssemblies.Add(typeof(ModuleServices).Assembly);
        return services;
    }
}
