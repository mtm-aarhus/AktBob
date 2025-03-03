namespace AktBob.Shared.Jobs;
public record CheckOCRScreeningStatusRegisterFilesJob(Guid FilArkivCaseId, long PodioItemId);