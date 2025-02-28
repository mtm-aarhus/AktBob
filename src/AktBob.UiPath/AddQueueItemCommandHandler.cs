using AktBob.UiPath.Contracts;
using MediatR;

namespace AktBob.UiPath;
internal class AddQueueItemCommandHandler(IUiPathOrchestratorApi uiPathOrchestratorApi) : IRequestHandler<AddQueueItemCommand>
{
    private readonly IUiPathOrchestratorApi _uiPathOrchestratorApi = uiPathOrchestratorApi;

    public async Task Handle(AddQueueItemCommand request, CancellationToken cancellationToken)
    {
        await _uiPathOrchestratorApi.AddQueueItem(
            request.QueueName,
            request.Reference,
            request.Payload);
    }
}
