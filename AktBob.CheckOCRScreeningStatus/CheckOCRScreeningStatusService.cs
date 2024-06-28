using AktBob.CheckOCRScreeningStatus.DTOs;
using AktBob.CheckOCRScreeningStatus.UseCases.GetFileStatus;
using AktBob.CheckOCRScreeningStatus.UseCases.RegisterDocuments;
using AktBob.CheckOCRScreeningStatus.UseCases.RemoveCaseFromCache;
using AktBob.CheckOCRScreeningStatus.UseCases.UpdatePodioItem;
using AktBob.CreateOCRScreeningStatus.ExternalQueue;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AktBob.CheckOCRScreeningStatus;
internal class CheckOCRScreeningStatusService : ICheckOCRScreeningStatusService
{
    private readonly ICheckOCRScreeningStatusQueue _queue;
    private readonly ILogger<CheckOCRScreeningStatusService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IData _data;
    private readonly IMediator _mediator;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public CheckOCRScreeningStatusService(
        ICheckOCRScreeningStatusQueue queue,
        ILogger<CheckOCRScreeningStatusService> logger,
        IConfiguration configuration,
        IData data,
        IMediator mediator)
    {
        _queue = queue;
        _logger = logger;
        _configuration = configuration;
        _data = data;
        _mediator = mediator;
    }

    public async Task Execute()
    {
        _logger.LogInformation("Getting queue messsages");

        var messages = await _queue.GetMessages(1);

        if (!messages.Any())
        {
            _logger.LogInformation("No messages pending");
            return;
        }

        var message = messages.First();

        // Delete the queue before processing starts because we don't know how long the processing will take and we don't want another job start processing the same queue message concurrently
        _logger.LogInformation("Deleting queue message {id}", message.Id);
        await _queue.DeleteMessage(message.Id, message.PopReceipt);

        _logger.LogInformation("Processing queue message {id}", message.Id);

        var content = JsonSerializer.Deserialize<QueueMessageBodyDto>(message.Body, _jsonSerializerOptions);

        if (content is not null)
        {
            _data.AddCase(content.FilArkivCaseId, content.PodioItemId);

            var registerFilesCommand = new RegisterFilesCommand(content.FilArkivCaseId);
            var registerFilesResult = await _mediator.Send(registerFilesCommand);

            if (!registerFilesResult.IsSuccess)
            {
                LogErrors(registerFilesResult.Errors);
                return;
            }

            await Task.WhenAll(_data.GetCase(content.FilArkivCaseId)!.Files.Select(f => _mediator.Send(new GetFileStatusQuery(f.FileId))));

            var updatePodioCommand = new UpdatePodioItemCommand(content.FilArkivCaseId);
            var updatePodioResult = await _mediator.Send(updatePodioCommand);

            if (!updatePodioResult.IsSuccess)
            {
                LogErrors(updatePodioResult.Errors);
                return;
            }

            var removeCaseFromCacheCommand = new RemoveCaseFromCacheCommand(content.FilArkivCaseId);
            await _mediator.Send(removeCaseFromCacheCommand);
        }
        else
        {
            _logger.LogError($"Queue message not valid. ({message.Body})");
        }

        _logger.LogInformation("Process finished");
    }

    private void LogErrors(IEnumerable<string> errors)
    {
        foreach (var error in errors)
        {
            _logger.LogError(error);
        }
    }
}
