using AktBob.Shared.Types.Deskpro;

namespace AktBob.Api.Endpoints.DeskproTicketToGetOrganized;

internal record DeskproTicketToGetOrganizedRequest(TicketId TicketId, string GOCaseNumber, int[] CustomFieldIds, int[] CaseNumberFieldIds);