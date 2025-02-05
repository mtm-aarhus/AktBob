using AktBob.CheckOCRScreeningStatus.Events;
using AktBob.Database.Contracts;
using AktBob.Database.UseCases.Cases.GetCases;

namespace AktBob.CheckOCRScreeningStatus.Consumers.UpdateDatabase;
public class FilesRegisteredConsumer(IData data, IMediator mediator, ILogger<FilesRegisteredConsumer> logger) : IConsumer<FilesRegistered>
{
    private readonly IData _data = data;
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<FilesRegisteredConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<FilesRegistered> context)
    {
        var message = context.Message;

        var podioItemId = _data.GetCase(message.CaseId)!.PodioItemId;

        var getDatabaseCaseQuery = new GetCasesQuery(null, podioItemId, null);
        var getDatabaseCaseResult = await _mediator.SendRequest(getDatabaseCaseQuery, context.CancellationToken);

        if (!getDatabaseCaseResult.IsSuccess)
        {
            _logger.LogWarning("Database did not return any case for Podio item id {id}", podioItemId);
            return;
        }

        var updateDatabaseCaseCommand = new UpdateCaseCommand(getDatabaseCaseResult.Value.First().Id, podioItemId, null, null, null);
        var updateDatabaseCaseCommandResult = await _mediator.SendRequest(updateDatabaseCaseCommand, context.CancellationToken);

        if (!updateDatabaseCaseCommandResult.IsSuccess)
        {
            _logger.LogWarning("Error updating database setting FilArkivCaseId {caseId} for Podio item id {id}", message.CaseId, podioItemId);
            return;
        }

        _logger.LogInformation("Database updated: FilArkivCaseId {caseId} set for Podio item id {id}", message.CaseId, podioItemId);
    }
}
