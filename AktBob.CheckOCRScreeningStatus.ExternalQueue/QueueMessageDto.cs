namespace AktBob.CreateOCRScreeningStatus.ExternalQueue;
public record QueueMessageDto(string Id, string Body, string PopReceipt);