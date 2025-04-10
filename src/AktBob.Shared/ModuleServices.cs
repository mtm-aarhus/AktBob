using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Shared;
public static class ModuleServices
{
    public static IServiceCollection AddSharedModule(this IServiceCollection services)
    {
        services.AddSingleton<IAppConfig, AppConfig>();
        services.AddSingleton<ITimeProvider, TimeProvider>();
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }
}
