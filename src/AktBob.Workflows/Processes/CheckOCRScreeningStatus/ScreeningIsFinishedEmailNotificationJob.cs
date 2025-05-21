using AktBob.Shared.Types.Podio;

namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;
internal record ScreeningIsFinishedEmailNotificationJob(ItemId PodioItemId, Guid FilArkivCaseId);