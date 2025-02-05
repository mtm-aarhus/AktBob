using AktBob.Database.Entities;
using AktBob.Database.UseCases.Tickets.GetTicketById;
using Ardalis.Result;
using Dapper;
using MediatR;
using System.Data;

namespace AktBob.Database.UseCases.Tickets.AddTicket;
internal class AddTicketCommandHandler : IRequestHandler<AddTicketCommand, Result<Ticket>>
{
    private readonly IMediator _mediator;
    private readonly ISqlDataAccess _sqlDataAccess;

    public AddTicketCommandHandler(IMediator mediator, ISqlDataAccess sqlDataAccess)
    {
        _mediator = mediator;
        _sqlDataAccess = sqlDataAccess;
    }

    public async Task<Result<Ticket>> Handle(AddTicketCommand request, CancellationToken cancellationToken)
    {
        var ticketParameters = new DynamicParameters();
        ticketParameters.Add(Constants.T_TICKETS_DESKPRO_ID, request.DeskproTicketId, dbType: DbType.Int32, direction: ParameterDirection.Input);
        ticketParameters.Add(Constants.T_TICKETS_ID, dbType: DbType.Int32, direction: ParameterDirection.Output);

        var addTicketResult = await _sqlDataAccess.ExecuteProcedure(Constants.SP_TICKET_CREATE, ticketParameters);
        
        if (!addTicketResult.IsSuccess)
        {
            return Result.CriticalError();
        }

        var ticketId = ticketParameters.Get<int>(Constants.T_TICKETS_ID);

        var getTicketQuery = new GetTicketByIdQuery(ticketId);
        var getTicketQueryResult = await _mediator.Send(getTicketQuery);
        return getTicketQueryResult;
    }
}