using AktBob.UiPath.Contracts;
using MediatR;

namespace AktBob.UiPath;
internal class AddQueueItemCommandHandler : IRequestHandler<AddQueueItemCommand>
{
    private readonly IUiPathOrchestratorApi _uiPathOrchestratorApi;

    public AddQueueItemCommandHandler(IUiPathOrchestratorApi uiPathOrchestratorApi)
    {
        _uiPathOrchestratorApi = uiPathOrchestratorApi;
    }

    public async Task Handle(AddQueueItemCommand request, CancellationToken cancellationToken)
    {
        await _uiPathOrchestratorApi.AddQueueItem(
            request.QueueName,
            request.Reference,
            request.QueueItem);
    }
}
