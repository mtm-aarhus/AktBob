using AAK.Deskpro;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Deskpro;
internal class GetDeskproTicketByIdQueryHandler(IDeskproClient deskproClient) : MediatorRequestHandler<GetDeskproTicketByIdQuery, Result<TicketDto>>
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    protected override async Task<Result<TicketDto>> Handle(GetDeskproTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _deskproClient.GetTicketById(request.Id, cancellationToken);

        if (ticket == null)
        {
            return Result.NotFound();
        }

        var dto = new TicketDto
        {
            Id = ticket.Id,
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

    
}
