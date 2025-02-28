using AAK.GetOrganized;
using AktBob.GetOrganized.Contracts;
using MediatR;

namespace AktBob.GetOrganized.UseCases;
internal class FinalizeDocumentCommandHandler(IGetOrganizedClient getOrganizedClient) : IRequestHandler<FinalizeDocumentCommand>
{
    private readonly IGetOrganizedClient _getOrganizedClient = getOrganizedClient;

    public async Task Handle(FinalizeDocumentCommand request, CancellationToken cancellationToken)
    {
        await _getOrganizedClient.FinalizeDocument(request.DocumentId, request.ShouldCloseOpenTasks, cancellationToken);
    }
}
