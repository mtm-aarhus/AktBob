using AktBob.GetOrganized.Contracts;
using MassTransit.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.JobHandlers.Handlers.AddMessagesToGetOrganized;
internal class FinalizeDocument(IServiceScopeFactory serviceScopeFactory)
{
    private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory;

    public async Task Run(int documentId, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var finalizeParentDocumentCommand = new FinalizeDocumentCommand(documentId, false);
        await mediator.Send(finalizeParentDocumentCommand, cancellationToken);
    }
}
