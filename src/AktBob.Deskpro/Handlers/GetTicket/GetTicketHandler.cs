using AAK.Deskpro;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Shared.Types.Deskpro;

namespace AktBob.Deskpro.Handlers.GetTicket;
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
            Agent = Mappers.MapPerson(ticket.Agent),
            Person = Mappers.MapPerson(ticket.Person),
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