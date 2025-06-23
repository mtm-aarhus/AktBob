namespace Aktbob.Processors.CheckOcrScreeningStatus.Jobs;

public record DispatchNotificationJob(long PodioItemId, Guid FilArkivCaseId);