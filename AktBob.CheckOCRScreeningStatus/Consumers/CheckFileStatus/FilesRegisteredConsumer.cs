using AktBob.CheckOCRScreeningStatus.Events;
using AktBob.CheckOCRScreeningStatus.UseCases.GetFileStatus;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;

namespace AktBob.CheckOCRScreeningStatus.Consumers.CheckOCRStatus;
public class FilesRegisteredConsumer(IData data, IMediator mediator, ILogger<FilesRegistered> logger) : IConsumer<FilesRegistered>
{
    private readonly IData _data = data;
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<FilesRegistered> _logger = logger;

    public async Task Consume(ConsumeContext<FilesRegistered> context)
    {
        var message = context.Message;
        var files = _data.GetCase(message.CaseId)?.Files;

        if (files == null || files.Count == 0)
        {
            _logger.LogWarning("No files registered for case {id}. Assuming this was intented.", message.CaseId);
        }
        else
        {
            _logger.LogInformation("Start checking file statusses for case {id}", message.CaseId);
            await Task.WhenAll(_data.GetCase(message.CaseId)!.Files.Select(f => _mediator.Send(new GetFileStatusQuery(f.FileId), context.CancellationToken)));
            _logger.LogInformation("Case {id}: OCRSceeningCompleted", message.CaseId);
        }

        await context.Publish(new OCRSceeningCompleted(message.CaseId));
    }
}
