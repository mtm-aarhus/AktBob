using AktBob.Shared.Extensions;

namespace AktBob.CloudConvert.Handlers.GenerateTasks;
internal class GenerateTasksHandlerLogging : IGenerateTasksHandler
{
    private readonly IGenerateTasksHandler _inner;
    private readonly ILogger<GenerateTasksHandler> _logger;

    public GenerateTasksHandlerLogging(IGenerateTasksHandler inner, ILogger<GenerateTasksHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public ErrorOr<IReadOnlyDictionary<Guid, object>> Handle(IEnumerable<byte[]> items)
    {
        _logger.LogInformation("Generating CloudConvert tasks");

        var result = _inner.Handle(items);

        result.Switch(
            value => _logger.LogInformation("CloudConvert tasks generated"),
            errors => _logger.LogWarning("{name}: {error}", nameof(GenerateTasksHandler), errors.ToCommaDelimitedString()));

        return result;
    }
}