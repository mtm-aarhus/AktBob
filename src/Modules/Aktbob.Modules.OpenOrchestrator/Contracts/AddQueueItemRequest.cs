using System.Text.Json;

namespace Aktbob.Modules.OpenOrchestrator.Contracts;

public record AddQueueItemRequest(string QueueName, string Reference, JsonDocument? Payload);