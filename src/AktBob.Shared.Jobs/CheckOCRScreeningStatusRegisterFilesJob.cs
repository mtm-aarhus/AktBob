using AktBob.Shared.Types.Podio;

namespace AktBob.Shared.Jobs;
public record CheckOCRScreeningStatusRegisterFilesJob(Guid FilArkivCaseId, ItemId PodioItemId);