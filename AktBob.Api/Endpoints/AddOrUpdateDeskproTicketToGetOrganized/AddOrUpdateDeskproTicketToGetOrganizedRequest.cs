namespace AktBob.Api.Endpoints.AddOrUpdateDeskproTicketToGetOrganized;

internal record AddOrUpdateDeskproTicketToGetOrganizedRequest(int TicketId, string GOCaseNumber, int[] CustomFieldIds, int[] CaseNumberFieldIds);