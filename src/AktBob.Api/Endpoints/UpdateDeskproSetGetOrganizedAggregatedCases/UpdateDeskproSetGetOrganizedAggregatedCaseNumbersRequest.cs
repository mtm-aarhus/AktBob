using AktBob.Shared.Types.Deskpro;

namespace AktBob.Api.Endpoints.UpdateDeskproSetGetOrganizedAggregatedCases;

internal record UpdateDeskproSetGetOrganizedAggregatedCaseNumbersRequest(TicketId DeskproTicketId, string CaseIds);
