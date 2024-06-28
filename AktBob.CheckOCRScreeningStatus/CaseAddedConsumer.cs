using AktBob.CheckOCRScreeningStatus.Events;
using AktBob.CheckOCRScreeningStatus.UseCases.GetFileStatus;
using AktBob.CheckOCRScreeningStatus.UseCases.RegisterDocuments;
using AktBob.CheckOCRScreeningStatus.UseCases.RemoveCaseFromCache;
using AktBob.CheckOCRScreeningStatus.UseCases.UpdatePodioItem;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AktBob.CheckOCRScreeningStatus;
internal class CaseAddedConsumer : INotificationHandler<CaseAdded>
{
    private readonly ILogger<CaseAddedConsumer> _logger;
    private readonly IData _data;
    private readonly IMediator _mediator;

    public CaseAddedConsumer(
        ILogger<CaseAddedConsumer> logger,
        IData data,
        IMediator mediator) 
    {
        _logger = logger;
        _data = data;
        _mediator = mediator;
    }

    public async Task Handle(CaseAdded notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting processing case {id}", notification.CaseId);

        var registerFilesCommand = new RegisterFilesCommand(notification.CaseId);
        var registerFilesResult = await _mediator.Send(registerFilesCommand);

        if (!registerFilesResult.IsSuccess)
        {
            LogErrors(registerFilesResult.Errors);
            return;
        }

        await Task.WhenAll(_data.GetCase(notification.CaseId)!.Files.Select(f => _mediator.Send(new GetFileStatusQuery(f.FileId))));

        var updatePodioCommand = new UpdatePodioItemCommand(notification.CaseId);
        var updatePodioResult = await _mediator.Send(updatePodioCommand);

        if (!updatePodioResult.IsSuccess)
        {
            LogErrors(updatePodioResult.Errors);
            return;
        }

        var removeCaseFromCacheCommand = new RemoveCaseFromCacheCommand(notification.CaseId);
        await _mediator.Send(removeCaseFromCacheCommand);


        _logger.LogInformation("Case {id} processed", notification.CaseId);
    }

    private void LogErrors(IEnumerable<string> errors)
    {
        foreach (var error in errors)
        {
            _logger.LogError(error);
        }
    }
}
