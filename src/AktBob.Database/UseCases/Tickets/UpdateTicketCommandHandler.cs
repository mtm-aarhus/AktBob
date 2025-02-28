using AktBob.Database.Contracts.Dtos;
using AktBob.Database.UseCases.Tickets.UpdateTicket;

namespace AktBob.Database.UseCases.Tickets;
internal class UpdateTicketCommandHandler(ISqlDataAccess sqlDataAccess, IMediator mediator) : IRequestHandler<UpdateTicketCommand, Result<TicketDto>>
{
    private readonly ISqlDataAccess _sqlDataAccess = sqlDataAccess;
    private readonly IMediator _mediator = mediator;

    public async Task<Result<TicketDto>> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
    {
        var getTicketQuery = new GetTicketByIdQuery(request.Id);
        var getTicketResult = await _mediator.Send(getTicketQuery, cancellationToken);

        if (!getTicketResult.IsSuccess)
        {
            return Result.Error();
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

        if (request.CaseUrl != null)
        {
            ticket.CaseUrl = request.CaseUrl;
        }

        // Execture database procedure
        var parameters = new DynamicParameters();
        parameters.Add(Constants.T_TICKETS_ID, ticket.Id);
        parameters.Add(Constants.T_TICKETS_CASENUMBER, ticket.CaseNumber);
        parameters.Add(Constants.T_CASES_SHAREPOINT_FOLDERNAME, ticket.SharepointFolderName);
        parameters.Add(Constants.T_TICKETS_JOURNALIZED_AT, ticket.JournalizedAt);
        parameters.Add(Constants.T_TICKETS_CLOSED_AT, ticket.TicketClosedAt);
        parameters.Add(Constants.T_TICKETS_CASEURL, ticket.CaseUrl);

        var result = await _sqlDataAccess.ExecuteProcedure(Constants.SP_TICKET_UPDATE_BY_ID, parameters);

        if (!result.IsSuccess)
        {
            return Result.CriticalError();
        }

        return Result.Success(ticket);
    }
}
