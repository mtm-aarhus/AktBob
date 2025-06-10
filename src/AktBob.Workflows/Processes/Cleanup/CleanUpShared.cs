using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Workflows.Processes.Cleanup;
internal static class CleanUpShared
{
    public static async Task<ErrorOr<TicketDto>> GetDeskproTicket(int ticketId, IDeskproModule deskpro, CancellationToken cancellationToken)
    {
        var getDeskproTicketResult = await deskpro.GetTicket(ticketId, cancellationToken);
        if (getDeskproTicketResult.IsError || getDeskproTicketResult.Value is null)
        {
            throw new BusinessException($"Error getting ticket {ticketId} from Deskpro.");
        }

        return getDeskproTicketResult;
    }

    public static DateTime? ParseAfslutningsdatoValue(TicketDto deskproTicket, int afslutningsdatoFieldId)
    {
        var afslutningsdatoFieldValue = deskproTicket.Fields.FirstOrDefault(x => x.Id == afslutningsdatoFieldId)?.Values.FirstOrDefault();
        if (DateTime.TryParse(afslutningsdatoFieldValue, out DateTime dateTime))
        {
            return dateTime;
        }

        return null;
    }

    public static int? ParseWorkflowValue(TicketDto deskproTicket, int workflowFieldId)
    {
        var workflow = deskproTicket.Fields.FirstOrDefault(x => x.Id == workflowFieldId)?.Values.FirstOrDefault();
        if (int.TryParse(workflow, out int workflowId))
        {
            return workflowId;
        }

        return null;
    }
}
