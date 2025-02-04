using AktBob.CheckOCRScreeningStatus.Events;
using AktBob.DatabaseAPI.Contracts.Commands;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;

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

        var updateDatabaseCaseCommand = new UpdateCaseSetFilArkivCaseIdCommand(podioItemId, message.CaseId);
        var updateDatabaseCaseCommandResult = await _mediator.SendRequest(updateDatabaseCaseCommand, context.CancellationToken);

        if (!updateDatabaseCaseCommandResult.IsSuccess)
        {
            _logger.LogWarning("Error updating database setting FilArkivCaseId {caseId} for Podio item id {id}", message.CaseId, podioItemId);
            return;
        }

        _logger.LogInformation("Database updated: FilArkivCaseId {caseId} set for Podio item id {id}", message.CaseId, podioItemId);
    }
}
