using AktBob.Database.Contracts.Dtos;
using System.Data;

namespace AktBob.Database.UseCases.Tickets;

internal record AddTicketCommand(int DeskproTicketId) : IRequest<Result<TicketDto>>;

internal class AddTicketCommandHandler(IMediator mediator, ISqlDataAccess sqlDataAccess) : IRequestHandler<AddTicketCommand, Result<TicketDto>>
{
    private readonly IMediator _mediator = mediator;
    private readonly ISqlDataAccess _sqlDataAccess = sqlDataAccess;

    public async Task<Result<TicketDto>> Handle(AddTicketCommand request, CancellationToken cancellationToken)
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
        var getTicketQueryResult = await _mediator.Send(getTicketQuery, cancellationToken);
        return getTicketQueryResult.Value;
    }
}