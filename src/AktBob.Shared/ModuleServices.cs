using AktBob.Shared.CQRS;
using AktBob.Shared.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AktBob.Shared;
public static class ModuleServices
{
    public static IServiceCollection AddSharedModule(this IServiceCollection services, List<Assembly> cqrsHandlersAssemblies)
    {
        services.AddTransient<ITimeProvider, TimeProvider>();
        services.AddMemoryCache();


        // CQRS
        services.AddTransient(typeof(IMediatorMiddleware<>), typeof(LoggingMiddleware<>));
        services.AddTransient(typeof(IMediatorMiddleware<,>), typeof(LoggingMiddleware<,>));
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
        services.AddCQRSHandlers(cqrsHandlersAssemblies.ToArray());


        return services;
    }
}
