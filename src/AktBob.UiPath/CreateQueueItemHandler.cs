using AktBob.UiPath.Contracts;

namespace AktBob.UiPath;
internal class CreateQueueItemHandler(IUiPathOrchestratorApi uiPathOrchestratorApi) : ICreateUiPathQueueItemHandler
{
    private readonly IUiPathOrchestratorApi _uiPathOrchestratorApi = uiPathOrchestratorApi;

    public async Task Handle(string queueName, string reference, string payload, CancellationToken cancellationToken)
    {
        await _uiPathOrchestratorApi.AddQueueItem(
            queueName,
            reference,
            payload);
    }
}
