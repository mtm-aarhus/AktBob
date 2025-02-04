using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.Queue;
public static class ModuleServices
{
    public static IServiceCollection AddQueueModule(this IServiceCollection services, IConfiguration configuration, List<Type> mediatorHandlers)
    {
        services.AddTransient<IQueue, Queue>();

        mediatorHandlers.AddRange([
            typeof(UseCases.DeleteQueueMessageCommandHandler),
            typeof(UseCases.GetQueueMessagesQueryHandler)]);

        return services;
    }
}
