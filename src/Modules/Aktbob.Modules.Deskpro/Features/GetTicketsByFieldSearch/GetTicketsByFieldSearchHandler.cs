using System.Collections.ObjectModel;
using AAK.Deskpro;
using AAK.Deskpro.Models;
using AktBob.Deskpro;
using Aktbob.Modules.Deskpro.Extensions;
using AktBob.Shared.Extensions;
using AktBob.Shared.Types.Deskpro;

namespace Aktbob.Modules.Deskpro.Features.GetTicketsByFieldSearch;
internal class GetTicketsByFieldSearchHandler(IDeskproClient deskpro) : IGetTicketsByFieldSearchHandler
{
    private readonly IDeskproClient _deskpro = deskpro;

    public async Task<ErrorOr<IReadOnlyCollection<TicketDto>>> Handle(int[] fields, string searchValue, CancellationToken cancellationToken)
    {
        ICollection<Ticket> ticketsList = new Collection<Ticket>();

        foreach (var field in fields)
        {
            var tickets = await _deskpro.GetTicketsByFieldValue(field, searchValue, cancellationToken);
            ticketsList!.AddRange(tickets);
        }

        if (ticketsList.Count == 0)
        {
            return Error.NotFound("GetTicketsByFieldSearchHandler.NotFound", $"Deskpro tickets by searching fields (fields: {string.Join(", ", fields.Select(x => x.ToString()))}) search value: '{searchValue}' not found.");
        }

        return ticketsList.Select(t => new TicketDto
        {
            Id = TicketId.Create(t.Id),
            CreatedAt = (DateTime)t.CreatedAt!,
            Agent = t.Agent.ToDto(),
            Person = t.Person.ToDto(),
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
        }).ToList();
    }
}
