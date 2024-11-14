using AktBob.CheckOCRScreeningStatus.Events;
using AktBob.CheckOCRScreeningStatus.UseCases.RegisterDocuments;
using AktBob.CheckOCRScreeningStatus.UseCases.RemoveCaseFromCache;
using JNJ.MessageBus;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AktBob.CheckOCRScreeningStatus.Consumers.RegisterFiles;
internal class CaseAddedConsumer : INotificationHandler<CaseAdded>
{
    private readonly ILogger<CaseAddedConsumer> _logger;
    private readonly IMediator _mediator;
    private readonly IEventBus _eventBus;

    public CaseAddedConsumer(
        ILogger<CaseAddedConsumer> logger,
        IMediator mediator,
        IEventBus eventBus)
    {
        _logger = logger;
        _mediator = mediator;
        _eventBus = eventBus;
    }

    public async Task Handle(CaseAdded notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering files for case {id}", notification.CaseId);

        var registerFilesCommand = new RegisterFilesCommand(notification.CaseId);
        var registerFilesResult = await _mediator.Send(registerFilesCommand);

        _logger.LogInformation("Files registered for case {id}", notification.CaseId);

        if (!registerFilesResult.IsSuccess)
        {
            await _mediator.Send(new RemoveCaseFromCacheCommand(notification.CaseId));
            return;
        }

        await _eventBus.Publish(new FilesRegistered(notification.CaseId));
    }
}