namespace AktBob.Database.Endpoints.Messages;
internal record GetMessagesRequest(bool? IncludeJournalized, int? DeskproMessageId);