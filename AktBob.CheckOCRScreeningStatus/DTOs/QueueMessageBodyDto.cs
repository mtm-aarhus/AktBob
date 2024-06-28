namespace AktBob.CheckOCRScreeningStatus.DTOs;

internal record QueueMessageBodyDto(Guid FilArkivCaseId, long PodioItemId);