using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AktBob.Queue;
public static class ModuleServices
{
    public static IServiceCollection AddQueueModule(this IServiceCollection services, IConfiguration configuration, List<Assembly> mediatRAssemblies)
    {
        services.AddTransient<IQueue, Queue>();


        mediatRAssemblies.Add(typeof(ModuleServices).Assembly);

        return services;
    }
}
