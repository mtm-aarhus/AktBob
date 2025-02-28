using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Entities;
using AktBob.Database.Extensions;
using AktBob.Database.UseCases.Cases;
using System.Data;

namespace AktBob.Database.UseCases.Tickets;

internal record GetTicketByIdQuery(int Id) : IRequest<Result<TicketDto>>;

internal class GetTicketByIdQueryHandler(ISqlDataAccess sqlDataAccess, IMediator mediator) : IRequestHandler<GetTicketByIdQuery, Result<TicketDto>>
{
    private readonly ISqlDataAccess _sqlDataAccess = sqlDataAccess;
    private readonly IMediator _mediator = mediator;

    public async Task<Result<TicketDto>> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add(Constants.T_TICKETS_ID, request.Id, DbType.Int32, ParameterDirection.Input);

        var getTicketsResult = await _sqlDataAccess.ExecuteProcedure<Ticket>(Constants.SP_TICKET_GET_BY_ID, parameters);

        if (!getTicketsResult.IsSuccess)
        {
            if (getTicketsResult.Status == ResultStatus.NotFound)
            {
                return Result.NotFound();
            }

            return Result.CriticalError();
        }

        var ticket = getTicketsResult.Value.First();

        // Get cases for the ticket
        var getCasesQuery = new GetCasesByTicketIdQuery(ticket.Id);
        var getCasesQueryResult = await _mediator.Send(getCasesQuery, cancellationToken);

        var dto = ticket.ToDto();

        if (getCasesQueryResult.IsSuccess)
        {
            dto.Cases = getCasesQueryResult.Value.AsList();
        }

        return dto;
    }
}
