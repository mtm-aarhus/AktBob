using AktBob.GetOrganized.Contracts;

namespace AktBob.JobHandlers.Handlers.AddMessageToGetOrganized;
internal class FinalizeDocument(IServiceScopeFactory serviceScopeFactory)
{
    private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory;

    public async Task Run(int documentId, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var commandDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        var finalizeParentDocumentCommand = new FinalizeDocumentCommand(documentId, false);
        await commandDispatcher.Dispatch(finalizeParentDocumentCommand, cancellationToken);
    }
}
