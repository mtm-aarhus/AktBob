namespace AktBob.ExternalQueue.Endpoints;

internal record PostQueueJournalizeDeskproTicketRequest(int TicketId, string GOCaseNumber, int[] CustomFieldIds, int[] CaseNumberFieldIds);