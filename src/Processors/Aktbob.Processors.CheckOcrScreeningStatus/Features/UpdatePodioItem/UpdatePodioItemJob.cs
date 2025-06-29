namespace Aktbob.Processors.CheckOcrScreeningStatus.Features.UpdatePodioItem;

internal record UpdatePodioItemJob(long PodioItemId, Guid FilArkivCaseId);