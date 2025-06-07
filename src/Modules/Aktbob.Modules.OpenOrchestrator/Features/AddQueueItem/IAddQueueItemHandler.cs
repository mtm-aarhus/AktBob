using System.Text.Json;
using ErrorOr;

namespace Aktbob.Modules.OpenOrchestrator.Features.AddQueueItem;

internal interface IAddQueueItemHandler
{
    Task<ErrorOr<Guid>> Handle(string queueName, string reference, JsonDocument? payload, CancellationToken cancellationToken);
}