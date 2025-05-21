using AktBob.Shared.Types.Podio;

namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;

internal record QueryFileProcessingStatusJob(ItemId PodioItemId, Guid FilArkivCaseId, Guid FilArkivFileId, int Count);
