using System.Text.Json;

namespace AktBob.Shared.Contracts.Modules.OpenOrchestrator;

public record CreateQueueItemRequest(string QueueName, string Reference, JsonDocument? Payload);