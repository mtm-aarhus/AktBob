using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Shared;
public static class ModuleServices
{
    public static IServiceCollection AddSharedModule(this  IServiceCollection services)
    {
        services.AddMemoryCache();
        return services;
    }
}
