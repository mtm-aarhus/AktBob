using AktBob.Shared.Types.Podio;

namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;
internal record UpdatePodioFilArkivFieldsJob(ItemId PodioItemId, Guid FilArkivCaseId);