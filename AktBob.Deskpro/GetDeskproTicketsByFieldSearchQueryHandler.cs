using AAK.Deskpro;
using AAK.Deskpro.Models;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using Ardalis.Result;
using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.Deskpro;
public class GetDeskproTicketsByFieldSearchQueryHandler : MediatorRequestHandler<GetDeskproTicketsByFieldSearchQuery, Result<IEnumerable<TicketDto>>>
{
    private readonly ILogger<GetDeskproTicketsByFieldSearchQueryHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly IDeskproClient _deskpro;

    public GetDeskproTicketsByFieldSearchQueryHandler(ILogger<GetDeskproTicketsByFieldSearchQueryHandler> logger, IConfiguration configuration, IDeskproClient deskpro)
    {
        _logger = logger;
        _configuration = configuration;
        _deskpro = deskpro;
    }

    protected override async Task<Result<IEnumerable<TicketDto>>> Handle(GetDeskproTicketsByFieldSearchQuery request, CancellationToken cancellationToken)
    {
        var ticketsList = new List<Ticket>();

        foreach (var field in request.Fields)
        {
            var tickets = await _deskpro.GetTicketsByFieldValue(field, request.SearchValue, cancellationToken);

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
