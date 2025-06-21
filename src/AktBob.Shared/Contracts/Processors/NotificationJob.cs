namespace AktBob.Shared.Contracts.Processors;

public record NotificationJob(
    string Recipient,
    string TemplateName,
    string Subject,
    IReadOnlyDictionary<string, string> Fields);