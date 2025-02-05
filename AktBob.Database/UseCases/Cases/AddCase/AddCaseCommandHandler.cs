using AktBob.Database.Entities;
using AktBob.Database.UseCases.Cases.GetCaseById;
using AktBob.Database.UseCases.Tickets.GetTicketById;
using Ardalis.Result;
using Dapper;
using MediatR;
using System.Data;

namespace AktBob.Database.UseCases.Cases.AddCase;
internal class AddCaseCommandHandler : IRequestHandler<AddCaseCommand, Result<Case>>
{
    private readonly ISqlDataAccess _sqlDataAccess;
    private readonly IMediator _mediator;

    public AddCaseCommandHandler(ISqlDataAccess sqlDataAccess, IMediator mediator)
    {
        _sqlDataAccess = sqlDataAccess;
        _mediator = mediator;
    }

    public async Task<Result<Case>> Handle(AddCaseCommand request, CancellationToken cancellationToken)
    {
        var getTicketQuery = new GetTicketByIdQuery(request.TicketId);
        var ticket = await _mediator.Send(getTicketQuery);

        if (!ticket.IsSuccess)
        {
            return Result<Case>.Invalid(new List<ValidationError>
            {
                new ValidationError
                {
                    Identifier = nameof(request.TicketId),
                    ErrorMessage = $"Ticket with id [{request.TicketId}] not found in database"
                }
            });
        }

        var parameters = new DynamicParameters();
        parameters.Add(Constants.T_CASES_TICKET_ID, request.TicketId);
        parameters.Add(Constants.T_CASES_PODIO_ITEM_ID, request.PodioItemId);
        parameters.Add(Constants.T_CASES_FILARKIV_CASE_ID, request.FilArkivCaseId);
        parameters.Add(Constants.T_CASES_CASENUMBER, request.CaseNumber);
        parameters.Add(Constants.T_CASES_ID, dbType: DbType.Int32, direction: ParameterDirection.Output);

        await _sqlDataAccess.ExecuteProcedure(Constants.SP_CASE_CREATE, parameters);
        var caseId = parameters.Get<int>(Constants.T_CASES_ID);

        var getCaseQuery = new GetCaseByIdQuery(caseId);
        var getCaseQueryResult = await _mediator.Send(getCaseQuery, cancellationToken);

        return getCaseQueryResult;
    }
}