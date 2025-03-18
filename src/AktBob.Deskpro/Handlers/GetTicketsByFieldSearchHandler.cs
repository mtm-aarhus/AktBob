using AAK.Deskpro;
using AAK.Deskpro.Models;
using AktBob.Shared.Extensions;
using System.Collections.ObjectModel;

namespace AktBob.Deskpro.Handlers;
internal class GetTicketsByFieldSearchHandler(IDeskproClient deskpro) : IGetTicketsByFieldSearchHandler
{
    private readonly IDeskproClient _deskpro = deskpro;

    public async Task<Result<IEnumerable<TicketDto>>> Handle(int[] fields, string searchValue, CancellationToken cancellationToken)
    {
        try
        {
            ICollection<Ticket> ticketsList = new Collection<Ticket>();

            foreach (var field in fields)
            {
                var tickets = await _deskpro.GetTicketsByFieldValue(field, searchValue, cancellationToken);

                if (tickets is not null)
                {
                    ticketsList!.AddRange(tickets);
                }
            }

            if (!ticketsList.Any())
            {
                return Result.Error($"No Deskpro tickets found by searching fields (fields: {string.Join(", ", fields.Select(x => x.ToString()))}) search value: '{searchValue}'.");
            }

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
        catch (HttpRequestException ex)
        {
            return Result.Error($"Error getting tickets by field search: {ex}");
        }
        catch (Exception)
        {
            throw;
        }
    }
}
