using AktBob.Shared.Types.Podio;

namespace AktBob.Shared.Jobs;
public record CreateDocumentListQueueItemJob(ItemId PodioItemId);