using AktBob.CheckOCRScreeningStatus.Events;
using AktBob.CheckOCRScreeningStatus.UseCases.GetFileStatus;
using AktBob.CheckOCRScreeningStatus.UseCases.RegisterDocuments;
using AktBob.CheckOCRScreeningStatus.UseCases.RemoveCaseFromCache;
using AktBob.CheckOCRScreeningStatus.UseCases.UpdatePodioItem;
using AktBob.DatabaseAPI.Contracts.Commands;
using AktBob.Podio.Contracts;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.CheckOCRScreeningStatus;
internal class CaseAddedConsumer : INotificationHandler<CaseAdded>
{
    private readonly ILogger<CaseAddedConsumer> _logger;
    private readonly IData _data;
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public CaseAddedConsumer(
        ILogger<CaseAddedConsumer> logger,
        IData data,
        IMediator mediator,
        IConfiguration configuration)
    {
        _logger = logger;
        _data = data;
        _mediator = mediator;
        _configuration = configuration;
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

        var updateDatabaseCaseCommand = new UpdateCaseSetFilArkivCaseIdCommand(_data.GetCase(notification.CaseId)!.PodioItemId, notification.CaseId);
        var updateDatabaseCaseCommandResult = await _mediator.Send(updateDatabaseCaseCommand);

        if (!updateDatabaseCaseCommandResult.IsSuccess)
        {
            _logger.LogWarning("Error updating database setting FilArkivCaseId {caseId} for Podio item id {id}", notification.CaseId, _data.GetCase(notification.CaseId)!.PodioItemId);
        }

        await Task.WhenAll(_data.GetCase(notification.CaseId)!.Files.Select(f => _mediator.Send(new GetFileStatusQuery(f.FileId))));

        var updatePodioCommand = new UpdatePodioItemCommand(notification.CaseId);
        var updatePodioResult = await _mediator.Send(updatePodioCommand);

        if (!updatePodioResult.IsSuccess)
        {
            LogErrors(updatePodioResult.Errors);
            return;
        }


        // ** Post comment on Podio item **
        var podioAppId = _configuration.GetValue<int>("Podio:AppId");
        var commentText = "OCR screening af dokumenterne på FilArkiv er færdig.";

        var postCommentCommand = new PostItemCommentCommand(podioAppId, _data.GetCase(notification.CaseId)!.PodioItemId, commentText);
        var postCommentCommandResult = await _mediator.Send(postCommentCommand, cancellationToken);

        if (!postCommentCommandResult.IsSuccess)
        {
            _logger.LogWarning("Error posting comment on Podio item {id}", _data.GetCase(notification.CaseId)!.PodioItemId);
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