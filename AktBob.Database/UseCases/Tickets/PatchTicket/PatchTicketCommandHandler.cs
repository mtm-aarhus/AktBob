using AktBob.Database.Entities;
using AktBob.Database.UseCases.Tickets.GetTicketById;
using Ardalis.Result;
using Dapper;
using MediatR;

namespace AktBob.Database.UseCases.Tickets.PatchTicket;
internal class PatchTicketCommandHandler : IRequestHandler<PatchTicketCommand, Result<Ticket>>
{
    private readonly ISqlDataAccess _sqlDataAccess;
    private readonly IMediator _mediator;

    public PatchTicketCommandHandler(ISqlDataAccess sqlDataAccess, IMediator mediator)
    {
        _sqlDataAccess = sqlDataAccess;
        _mediator = mediator;
    }

    public async Task<Result<Ticket>> Handle(PatchTicketCommand request, CancellationToken cancellationToken)
    {
        var getTicketQuery = new GetTicketByIdQuery(request.Id);
        var getTicketResult = await _mediator.Send(getTicketQuery);

        if (!getTicketResult.IsSuccess)
        {
            return getTicketResult;
        }

        var ticket = getTicketResult.Value;

        
        // Assign new values to properties

        if (!string.IsNullOrEmpty(request.CaseNumber))
        {
            ticket.CaseNumber = request.CaseNumber;
        }

        if (!string.IsNullOrEmpty(request.SharepointFolderName))
        {
            ticket.SharepointFolderName = request.SharepointFolderName;
        }

        if (request.JournalizedAt != null)
        {
            ticket.JournalizedAt = request.JournalizedAt;
        }

        if (request.TicketClosedAt != null)
        {
            ticket.TicketClosedAt = request.TicketClosedAt;
        }

        // Execture database procedure
        var parameters = new DynamicParameters();
        parameters.Add(Constants.T_TICKETS_ID, ticket.Id);
        parameters.Add(Constants.T_TICKETS_CASENUMBER, ticket.CaseNumber);
        parameters.Add(Constants.T_CASES_SHAREPOINT_FOLDERNAME, ticket.SharepointFolderName);
        parameters.Add(Constants.T_TICKETS_JOURNALIZED_AT, ticket.JournalizedAt);
        parameters.Add(Constants.T_TICKETS_CLOSED_AT, ticket.TicketClosedAt);

        var result = await _sqlDataAccess.ExecuteProcedure(Constants.SP_TICKET_UPDATE_BY_ID, parameters);

        if (!result.IsSuccess)
        {
            return Result.CriticalError();
        }

        return Result.Success(ticket);
    }
}
