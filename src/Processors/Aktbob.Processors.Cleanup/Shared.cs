using ErrorOr;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using AktBob.Shared.Exceptions;
using AktBob.Shared.ModuleClients.DeskproModule;

namespace Aktbob.Processors.Cleanup;

internal static class Shared
{
    public static async Task<ErrorOr<TicketDto>> GetDeskproTicket(int ticketId, IDeskproModuleClient deskpro, CancellationToken cancellationToken)
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
        if (DateTime.TryParse(afslutningsdatoFieldValue, out var dateTime))
        {
            return dateTime;
        }

        return null;
    }

    public static int? ParseWorkflowValue(TicketDto deskproTicket, int workflowFieldId)
    {
        var workflow = deskproTicket.Fields.FirstOrDefault(x => x.Id == workflowFieldId)?.Values.FirstOrDefault();
        if (int.TryParse(workflow, out var workflowId))
        {
            return workflowId;
        }

        return null;
    }
}