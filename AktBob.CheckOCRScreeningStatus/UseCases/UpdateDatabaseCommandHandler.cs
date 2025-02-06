using AktBob.Database.Contracts;
using AktBob.Database.UseCases.Cases.GetCases;
using Microsoft.Extensions.DependencyInjection;

namespace AktBob.CheckOCRScreeningStatus.UseCases;

public record UpdateDatabaseCommand(Guid FilArkivCaseId, long PodioItemId);

public class UpdateDatabaseCommandHandler(IServiceScopeFactory serviceScopeFactory, ILogger<UpdateDatabaseCommandHandler> logger) : MediatorRequestHandler<UpdateDatabaseCommand>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<UpdateDatabaseCommandHandler> _logger = logger;

    protected override async Task Handle(UpdateDatabaseCommand request, CancellationToken cancellationToken)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var getDatabaseCaseQuery = new GetCasesQuery(null, request.PodioItemId, null);
            var getDatabaseCaseResult = await mediator.SendRequest(getDatabaseCaseQuery, cancellationToken);

            if (!getDatabaseCaseResult.IsSuccess || !getDatabaseCaseResult.Value.Any())
            {
                _logger.LogWarning("Database did not return any case for Podio item id {id}", request.PodioItemId);
                return;
            }

            var updateDatabaseCaseCommand = new UpdateCaseCommand(getDatabaseCaseResult.Value.First().Id, request.PodioItemId, null, null, null);
            var updateDatabaseCaseCommandResult = await mediator.SendRequest(updateDatabaseCaseCommand, cancellationToken);

            if (!updateDatabaseCaseCommandResult.IsSuccess)
            {
                _logger.LogWarning("Error updating database setting FilArkivCaseId {caseId} for Podio item id {id}", request.FilArkivCaseId, request.PodioItemId);
                return;
            }
        }
    }
}
