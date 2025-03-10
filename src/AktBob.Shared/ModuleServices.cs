using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Shared;
public static class ModuleServices
{
    public static IServiceCollection AddSharedModule(this IServiceCollection services)
    {
        services.AddSingleton<ITimeProvider, TimeProvider>();
        services.AddMemoryCache();

        //services.AddScoped(typeof(IJobHandler<>), typeof(JobHandlerDecoratorFactory<>)); // Must be registered AFTER all actual job handlers are registered in the other projects
        return services;
    }
}
