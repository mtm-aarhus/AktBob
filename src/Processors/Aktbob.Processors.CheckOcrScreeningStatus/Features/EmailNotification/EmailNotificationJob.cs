namespace Aktbob.Processors.CheckOcrScreeningStatus.Features.EmailNotification;

internal record EmailNotificationJob(long PodioItemId, Guid FilArkivCaseId);