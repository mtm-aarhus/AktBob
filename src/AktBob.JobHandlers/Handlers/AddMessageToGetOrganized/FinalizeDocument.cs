using AktBob.GetOrganized.Contracts;

namespace AktBob.JobHandlers.Handlers.AddMessageToGetOrganized;
internal class FinalizeDocument(IServiceScopeFactory serviceScopeFactory)
{
    private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory;

    public async Task Run(int documentId, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var finalizeGetOrganizedDocumentsHandler = scope.ServiceProvider.GetRequiredService<IFinalizeGetOrganizedDocumentHandler>();
        await finalizeGetOrganizedDocumentsHandler.Handle(documentId, false, cancellationToken);
    }
}
