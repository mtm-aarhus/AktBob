namespace AktBob.Shared.Contracts.Processors;

public record CreateDocumentListJob(long PodioItemId, int RescheduleCounter);