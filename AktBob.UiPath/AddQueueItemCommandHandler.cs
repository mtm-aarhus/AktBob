using AktBob.UiPath.Contracts;
using MassTransit.Mediator;

namespace AktBob.UiPath;
public class AddQueueItemCommandHandler(IUiPathOrchestratorApi uiPathOrchestratorApi) : MediatorRequestHandler<AddQueueItemCommand>
{
    private readonly IUiPathOrchestratorApi _uiPathOrchestratorApi = uiPathOrchestratorApi;

    protected override async Task Handle(AddQueueItemCommand request, CancellationToken cancellationToken)
    {
        await _uiPathOrchestratorApi.AddQueueItem(
            request.QueueName,
            request.Reference,
            request.QueueItem);
    }
}
