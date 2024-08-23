namespace AktBob.Queue.Contracts;
public record QueueMessageDto(string Id, string Body, string PopReceipt);