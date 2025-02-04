using AktBob.CheckOCRScreeningStatus.Events;
using AktBob.CheckOCRScreeningStatus.UseCases.RemoveCaseFromCache;
using AktBob.CheckOCRScreeningStatus.UseCases.UpdatePodioItem;
using AktBob.Podio.Contracts;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.CheckOCRScreeningStatus.Consumers.UpdatePodioItem;
internal class OCRScreeningCompletedConsumer(IMediator mediator, ILogger<OCRScreeningCompletedConsumer> logger, IData data, IConfiguration configuration) : IConsumer<OCRSceeningCompleted>
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<OCRScreeningCompletedConsumer> _logger = logger;
    private readonly IData _data = data;
    private readonly IConfiguration _configuration = configuration;

    public async Task Consume(ConsumeContext<OCRSceeningCompleted> context)
    {
        var message = context.Message;

        var podioItemId = _data.GetCase(message.CaseId)?.PodioItemId;

        if (podioItemId == null)
        {
            _logger.LogError("No Podio item id registered for FilArkivCaseId {id}", message.CaseId);
            return;
        }

        // Update Podio item
        await _mediator.Send(new UpdatePodioItemCommand(message.CaseId));


        // Post comment on Podio item
        try
        {
            var podioAppId = _configuration.GetValue<int>("Podio:AppId");
            var commentText = "OCR screening af dokumenterne på FilArkiv er færdig.";

            var postCommentCommand = new PostItemCommentCommand(podioAppId, _data.GetCase(message.CaseId)!.PodioItemId, commentText);
            var postCommentCommandResult = await _mediator.SendRequest(postCommentCommand, context.CancellationToken);

            if (!postCommentCommandResult.IsSuccess)
            {
                _logger.LogWarning("Error posting comment on Podio item {id}", _data.GetCase(message.CaseId)!.PodioItemId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error posting comment on Podio item {id}: {ex}", _data.GetCase(message.CaseId)!.PodioItemId, ex.Message);
        }

        await _mediator.Send(new RemoveCaseFromCacheCommand(message.CaseId), context.CancellationToken);
    }
}
