namespace Aktbob.Processors.CheckOcrScreeningStatus.Features.NotificationDispatcher;

public record DispatchNotificationJob(long PodioItemId, Guid FilArkivCaseId);