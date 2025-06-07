using Aktbob.Modules.OpenOrchestrator.Client;
using Aktbob.Modules.OpenOrchestrator.Features.AddQueueItem;

namespace Aktbob.Modules.OpenOrchestrator.Features;

internal static class RegisterHandlers
{
    public static IServiceCollection AddAddQueueHandler(this IServiceCollection services)
    {
        services.AddScoped<IAddQueueItemHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<AddQueueItemHandler>>();

            var inner = new AddQueueItemHandler(provider.GetRequiredService<OpenOrchestratorClient>());
            var withLogging = new AddQueueItemHandlerLogging(inner, logger);
            var withException = new AddQueueItemHandlerException(withLogging, logger);

            return withException;
        });
        
        return services;
    }
}