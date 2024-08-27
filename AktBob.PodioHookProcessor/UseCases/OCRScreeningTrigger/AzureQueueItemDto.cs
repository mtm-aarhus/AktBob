namespace AktBob.PodioHookProcessor.UseCases.OCRScreeningTrigger;

internal record AzureQueueItemDto(long PodioItemId, string CaseNumber);
