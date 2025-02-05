using AktBob.CheckOCRScreeningStatus.Events;
using AktBob.CheckOCRScreeningStatus.UseCases.RegisterFiles;
using AktBob.CheckOCRScreeningStatus.UseCases.RemoveCaseFromCache;

namespace AktBob.CheckOCRScreeningStatus.Consumers.RegisterFiles;

public class CaseAddedConsumer(ILogger<CaseAddedConsumer> logger, IMediator mediator) : IConsumer<CaseAdded>
{
    private readonly ILogger<CaseAddedConsumer> _logger = logger;
    private readonly IMediator _mediator = mediator;

    public async Task Consume(ConsumeContext<CaseAdded> context)
    {
        var message = context.Message;

        _logger.LogInformation("Registering files for case {id}", message.CaseId);

        var registerFilesCommand = new RegisterFilesCommand(message.CaseId);
        var registerFilesResult = await _mediator.SendRequest(registerFilesCommand, context.CancellationToken);

        _logger.LogInformation("Files registered for case {id}", message.CaseId);

        if (!registerFilesResult.IsSuccess)
        {
            await _mediator.Send(new RemoveCaseFromCacheCommand(message.CaseId));
            return;
        }

        await context.Publish(new FilesRegistered(message.CaseId));
    }
}