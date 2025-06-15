using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using AktBob.Shared.ModuleClients.DeskproModule;
using ErrorOr;

namespace AktBob.Shared;

public static class DeskproHelpers
{
    public static async Task<ErrorOr<PersonDto>> GetTicketAgent(IDeskproModuleClient deskpro, int ticketId, CancellationToken cancellationToken)
    {
        var ticket = await deskpro.GetTicket(ticketId, cancellationToken);
        if (ticket.IsError) return ticket.Errors;
        
        var agent = ticket.Value.Agent?.Id != null
            ? await deskpro.GetPersonById(ticket.Value.Agent.Id, cancellationToken)
            : Error.NotFound().ToErrorOr<PersonDto>();

        return agent;
    }
}