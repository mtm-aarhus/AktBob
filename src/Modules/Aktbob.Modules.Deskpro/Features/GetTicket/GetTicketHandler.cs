using AAK.Deskpro;
using AktBob.Deskpro;
using Aktbob.Modules.Deskpro.Contracts.DTOs;
using Aktbob.Modules.Deskpro.Extensions;
using AktBob.Shared.Types.Deskpro;

namespace Aktbob.Modules.Deskpro.Features.GetTicket;
internal class GetTicketHandler(IDeskproClient deskproClient) : IGetTicketHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task<ErrorOr<TicketDto>> Handle(int ticketId, CancellationToken cancellationToken)
    {
        var ticket = await _deskproClient.GetTicketById(ticketId, cancellationToken);

        if (ticket == null)
        {
            return Error.Failure("GetTicketHandler.Failure", $"Error getting ticket {ticketId} from Deskpro");
        }

        var dto = new TicketDto
        {
            Id = TicketId.Create(ticket.Id),
            CreatedAt = (DateTime)ticket.CreatedAt!,
            Agent = ticket.Agent.ToDto(),
            Person = ticket.Person.ToDto(),
            AgentTeamId = ticket.AgentTeamId,
            Auth = ticket.Auth,
            Department = ticket.Department,
            Ref = ticket.Ref,
            Subject = ticket.Subject,
            Fields = ticket.Fields.Select(f => new FieldDto
            {
                Id = f.Id,
                Values = f.Values,
                Choices = f.Choices
            })
        };

        return dto;
    }
}