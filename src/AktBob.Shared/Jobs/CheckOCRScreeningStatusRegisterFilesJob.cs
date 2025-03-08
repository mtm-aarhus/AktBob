namespace AktBob.Shared.Jobs;
public record CheckOCRScreeningStatusRegisterFilesJob(Guid FilArkivCaseId, PodioItemId PodioItemId);