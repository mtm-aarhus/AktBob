namespace Aktbob.Processors.CheckOcrScreeningStatus.Jobs;

internal record UpdatePodioItemJob(long PodioItemId, Guid FilArkivCaseId);