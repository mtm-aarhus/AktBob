using AktBob.Shared.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.Shared;
public static class ModuleServices
{
    public static IServiceCollection AddSharedModule(this IServiceCollection services)
    {
        services.AddSingleton<ITimeProvider, TimeProvider>();
        services.AddMemoryCache();

        return services;
    }
}
