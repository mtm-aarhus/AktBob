using AAK.Deskpro;
using System.Net;

namespace AktBob.Deskpro.Handlers;
internal class GetTicketHandler(IDeskproClient deskproClient) : IGetTicketHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task<Result<TicketDto>> Handle(int ticketId, CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _deskproClient.GetTicketById(ticketId, cancellationToken);

            if (ticket == null)
            {
                return Result.Error($"Error getting ticket {ticketId} from Deskpro");
            }

            var dto = new TicketDto
            {
                Id = ticket.Id,
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
                    Values = f.Values
                })
            };

            return dto;
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Error($"Error getting Deskpro ticket {ticketId}: {ex}");
        }
        catch(Exception)
        {
            throw;
        }
    }
}