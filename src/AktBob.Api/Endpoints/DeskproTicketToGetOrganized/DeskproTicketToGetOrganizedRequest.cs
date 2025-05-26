using AktBob.Shared.Types.Deskpro;

namespace AktBob.Api.Endpoints.DeskproTicketToGetOrganized;

internal record DeskproTicketToGetOrganizedRequest(int TicketId, string GOCaseNumber, int[] CustomFieldIds, int[] CaseNumberFieldIds);