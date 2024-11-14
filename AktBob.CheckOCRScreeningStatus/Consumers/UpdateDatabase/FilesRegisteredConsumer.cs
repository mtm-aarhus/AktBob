using AktBob.CheckOCRScreeningStatus.Events;
using AktBob.DatabaseAPI.Contracts.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AktBob.CheckOCRScreeningStatus.Consumers.UpdateDatabase;
internal class FilesRegisteredConsumer(IData data, IMediator mediator, ILogger<FilesRegisteredConsumer> logger) : INotificationHandler<FilesRegistered>
{
    private readonly IData _data = data;
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<FilesRegisteredConsumer> _logger = logger;

    public async Task Handle(FilesRegistered notification, CancellationToken cancellationToken)
    {
        var podioItemId = _data.GetCase(notification.CaseId)!.PodioItemId;
        
        var updateDatabaseCaseCommand = new UpdateCaseSetFilArkivCaseIdCommand(podioItemId, notification.CaseId);
        var updateDatabaseCaseCommandResult = await _mediator.Send(updateDatabaseCaseCommand);

        if (!updateDatabaseCaseCommandResult.IsSuccess)
        {
            _logger.LogWarning("Error updating database setting FilArkivCaseId {caseId} for Podio item id {id}", notification.CaseId, podioItemId);
            return;
        }

        _logger.LogInformation("Database updated: FilArkivCaseId {caseId} set for Podio item id {id}", notification.CaseId, podioItemId);
    }
}
