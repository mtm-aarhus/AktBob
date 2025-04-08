using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Shared;
public static class ModuleServices
{
    public static IServiceCollection AddSharedModule(this IServiceCollection services)
    {
        services.AddSingleton<ITimeProvider, TimeProvider>();
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }
}
