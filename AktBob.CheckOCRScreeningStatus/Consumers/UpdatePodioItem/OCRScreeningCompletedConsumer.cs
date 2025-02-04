using AktBob.CheckOCRScreeningStatus.Events;
using AktBob.CheckOCRScreeningStatus.UseCases.RemoveCaseFromCache;
using AktBob.CheckOCRScreeningStatus.UseCases.UpdatePodioItem;
using AktBob.Podio.Contracts;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.CheckOCRScreeningStatus.Consumers.UpdatePodioItem;
internal class OCRScreeningCompletedConsumer(IMediator mediator, ILogger<OCRScreeningCompletedConsumer> logger, IData data, IConfiguration configuration) : INotificationHandler<OCRSceeningCompleted>
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<OCRScreeningCompletedConsumer> _logger = logger;
    private readonly IData _data = data;
    private readonly IConfiguration _configuration = configuration;

    public async Task Handle(OCRSceeningCompleted notification, CancellationToken cancellationToken)
    {
        var podioItemId = _data.GetCase(notification.CaseId)?.PodioItemId;

        if (podioItemId == null)
        {
            _logger.LogError("No Podio item id registered for FilArkivCaseId {id}", notification.CaseId);
            return;
        }

        // Update Podio item
        await _mediator.Send(new UpdatePodioItemCommand(notification.CaseId));


        // Post comment on Podio item
        try
        {
            var podioAppId = _configuration.GetValue<int>("Podio:AppId");
            var commentText = "OCR screening af dokumenterne på FilArkiv er færdig.";

            var postCommentCommand = new PostItemCommentCommand(podioAppId, _data.GetCase(notification.CaseId)!.PodioItemId, commentText);
            var postCommentCommandResult = await _mediator.SendRequest(postCommentCommand, cancellationToken);

            if (!postCommentCommandResult.IsSuccess)
            {
                _logger.LogWarning("Error posting comment on Podio item {id}", _data.GetCase(notification.CaseId)!.PodioItemId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error posting comment on Podio item {id}: {ex}", _data.GetCase(notification.CaseId)!.PodioItemId, ex.Message);
        }

        await _mediator.Send(new RemoveCaseFromCacheCommand(notification.CaseId));
    }
}
