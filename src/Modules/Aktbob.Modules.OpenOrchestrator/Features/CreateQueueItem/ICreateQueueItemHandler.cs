using System.Text.Json;
using AktBob.Shared.Contracts.Modules.OpenOrchestrator;
using ErrorOr;

namespace Aktbob.Modules.OpenOrchestrator.Features.CreateQueueItem;

internal interface ICreateQueueItemHandler
{
    Task<ErrorOr<CreateQueueItemResponse>> Handle(string queueName, string reference, JsonDocument? payload, CancellationToken cancellationToken);
}