namespace AktBob.PodioHookProcessor.UseCases.MoveToFilArkivTrigger;

internal record AzureQueueItemDto(long PodioItemId, string CaseNumber);
