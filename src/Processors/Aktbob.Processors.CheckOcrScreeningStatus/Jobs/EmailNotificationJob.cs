namespace Aktbob.Processors.CheckOcrScreeningStatus.Jobs;

internal record EmailNotificationJob(long PodioItemId, Guid FilArkivCaseId);