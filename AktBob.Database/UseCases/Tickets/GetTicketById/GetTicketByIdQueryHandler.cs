using AktBob.Database.Entities;
using AktBob.Database.UseCases.Cases.GetCasesByTicketId;
using Ardalis.Result;
using Dapper;
using MassTransit;
using MassTransit.Mediator;
using System.Data;

namespace AktBob.Database.UseCases.Tickets.GetTicketById;
internal class GetTicketByIdQueryHandler(ISqlDataAccess sqlDataAccess, IMediator mediator) : MediatorRequestHandler<GetTicketByIdQuery, Result<Ticket>>
{
    private readonly ISqlDataAccess _sqlDataAccess = sqlDataAccess;
    private readonly IMediator _mediator = mediator;

    protected override async Task<Result<Ticket>> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
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
        var getCasesQueryResult = await _mediator.SendRequest(getCasesQuery, cancellationToken);

        if (getCasesQueryResult.IsSuccess)
        {
            ticket.Cases = getCasesQueryResult.Value.AsList();
        }

        return ticket;
    }
}
