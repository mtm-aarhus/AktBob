using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using ErrorOr;

namespace AktBob.Api.Endpoints.Cases;

internal class CreateCaseTransaction(IUnitOfWork unitOfWork)
{
    public async Task<ErrorOr<Success>> Run(long podioItemId, int deskproId, string caseNumber, CancellationToken cancellationToken)
    {
        // Get ticket from repository
        var databaseTicket = await unitOfWork.Tickets.GetByDeskproTicketId(deskproId);
        if (databaseTicket is null)
        {
            return Error.NotFound("CreateCase.NotFound", "The Deskpro ID was not found in the database");
        }
        
        // Add case to database
        var @case = new Case
        {
            TicketId = databaseTicket.Id,
            PodioItemId = podioItemId,
            CaseNumber = caseNumber
        };

        var success = await unitOfWork.Cases.Add(@case);
        if (success)
        {
            return Result.Success;
        }
        
        return Error.Failure("CreateCase.Failure", "Something went wrong creating a new case object in the database");
    }
}