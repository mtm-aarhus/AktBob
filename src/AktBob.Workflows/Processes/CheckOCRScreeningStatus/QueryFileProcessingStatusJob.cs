namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;

internal record QueryFileProcessingStatusJob(PodioItemId PodioItemId, Guid FilArkivCaseId, Guid FilArkivFileId, int Count);
