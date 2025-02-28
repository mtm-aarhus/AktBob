using AktBob.Shared.CQRS;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AktBob.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCQRSHandlers(this IServiceCollection services, Assembly[] assemblies)
    {
        var handlerTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces(), (type, iface) => new { type, iface })
            .Where(x => x.iface.IsGenericType && (
                x.iface.GetGenericTypeDefinition() == typeof(IQueryHandler<,>) ||
                x.iface.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)
            ));

        foreach (var handler in handlerTypes)
        {
            Console.WriteLine($"{handler.iface}, {handler.type}");
            services.AddTransient(handler.iface, handler.type);
        }

        return services;
    }
}