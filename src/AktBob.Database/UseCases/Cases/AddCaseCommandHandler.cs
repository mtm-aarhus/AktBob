using AktBob.Database.Contracts.Dtos;
using AktBob.Database.UseCases.Cases.AddCase;
using AktBob.Database.UseCases.Tickets;
using System.Data;

namespace AktBob.Database.UseCases.Cases;
internal class AddCaseCommandHandler(ISqlDataAccess sqlDataAccess, IQueryDispatcher queryDispatcher) : ICommandHandler<AddCaseCommand, Result<CaseDto>>
{
    private readonly ISqlDataAccess _sqlDataAccess = sqlDataAccess;
    private readonly IQueryDispatcher _queryDispatcher = queryDispatcher;

    public async Task<Result<CaseDto>> Handle(AddCaseCommand request, CancellationToken cancellationToken)
    {
        var getTicketQuery = new GetTicketByIdQuery(request.TicketId);
        var ticket = await _queryDispatcher.Dispatch(getTicketQuery, cancellationToken);

        if (!ticket.IsSuccess)
        {
            return Result<CaseDto>.Invalid(new List<ValidationError>
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
        var getCaseQueryResult = await _queryDispatcher.Dispatch(getCaseQuery, cancellationToken);

        return getCaseQueryResult.Value;
    }
}