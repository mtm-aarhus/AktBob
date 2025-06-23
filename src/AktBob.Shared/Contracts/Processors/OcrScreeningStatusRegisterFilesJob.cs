namespace AktBob.Shared.Contracts.Processors;

public record OcrScreeningStatusRegisterFilesJob(Guid FilArkivCaseId, long PodioItemId);