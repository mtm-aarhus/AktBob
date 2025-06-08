using Aktbob.Modules.OpenOrchestrator.Client;
using Aktbob.Modules.OpenOrchestrator.Features.CreateQueueItem;

namespace Aktbob.Modules.OpenOrchestrator.Features;

internal static class RegisterHandlers
{
    public static IServiceCollection AddAddQueueHandler(this IServiceCollection services)
    {
        services.AddScoped<ICreateQueueItemHandler>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<CreateQueueItemHandler>>();

            var inner = new CreateQueueItemHandler(provider.GetRequiredService<OpenOrchestratorClient>());
            var withLogging = new CreateQueueItemHandlerLogging(inner, logger);
            var withException = new CreateQueueItemHandlerException(withLogging, logger);

            return withException;
        });
        
        return services;
    }
}