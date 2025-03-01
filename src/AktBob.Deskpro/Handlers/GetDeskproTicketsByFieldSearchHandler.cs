using AAK.Deskpro;
using AAK.Deskpro.Models;

namespace AktBob.Deskpro.Handlers;
internal class GetDeskproTicketsByFieldSearchHandler(IDeskproClient deskpro) : IGetDeskproTicketsByFieldSearchHandler
{
    private readonly IDeskproClient _deskpro = deskpro;

    public async Task<Result<IEnumerable<TicketDto>>> Handle(int[] fields, string searchValue, CancellationToken cancellationToken)
    {
        var ticketsList = new List<Ticket>();

        foreach (var field in fields)
        {
            var tickets = await _deskpro.GetTicketsByFieldValue(field, searchValue, cancellationToken);

            if (tickets is not null && tickets.Count() > 0)
            {
                ticketsList.AddRange(tickets!);
            }
        };

        if (ticketsList.Count > 0)
        {
            var dto = ticketsList.Select(t => new TicketDto
            {
                Id = t.Id,
                Agent = Mappers.MapPerson(t.Agent),
                Person = Mappers.MapPerson(t.Person),
                AgentTeamId = t.AgentTeamId,
                Auth = t.Auth,
                Department = t.Department,
                Ref = t.Ref,
                Subject = t.Subject,
                Fields = t.Fields.Select(f => new FieldDto
                {
                    Id = f.Id,
                    Values = f.Values
                })
            });

            return Result.Success(dto);
        }

        return Result.NotFound();
    }
}
