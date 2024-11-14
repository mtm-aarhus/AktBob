using AktBob.CheckOCRScreeningStatus.Events;
using AktBob.CheckOCRScreeningStatus.UseCases.GetFileStatus;
using JNJ.MessageBus;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AktBob.CheckOCRScreeningStatus.Consumers.CheckOCRStatus;
internal class FilesRegisteredConsumer(IEventBus eventBus, IData data, IMediator mediator, ILogger<FilesRegistered> logger) : INotificationHandler<FilesRegistered>
{
    private readonly IEventBus _eventBus = eventBus;
    private readonly IData _data = data;
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<FilesRegistered> _logger = logger;

    public async Task Handle(FilesRegistered notification, CancellationToken cancellationToken)
    {
        var files = _data.GetCase(notification.CaseId)?.Files;

        if (files == null || files.Count == 0)
        {
            _logger.LogWarning("No files registered for case {id}. Assuming this was intented.", notification.CaseId);
        }
        else
        {
            _logger.LogInformation("Start checking file statusses for case {id}", notification.CaseId);
            await Task.WhenAll(_data.GetCase(notification.CaseId)!.Files.Select(f => _mediator.Send(new GetFileStatusQuery(f.FileId))));
            _logger.LogInformation("Case {id}: OCRSceeningCompleted", notification.CaseId);
        }

        await _eventBus.Publish(new OCRSceeningCompleted(notification.CaseId));
    }
}
