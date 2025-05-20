namespace AktBob.CloudConvert.Handlers.GenerateTasks;
internal class GenerateTasksHandlerException : IGenerateTasksHandler
{
    private readonly IGenerateTasksHandler _inner;
    private readonly ILogger<GenerateTasksHandler> _logger;

    public GenerateTasksHandlerException(IGenerateTasksHandler inner, ILogger<GenerateTasksHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public ErrorOr<IReadOnlyDictionary<Guid, object>> Handle(IEnumerable<byte[]> items)
    {
        try
        {
            return _inner.Handle(items);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error in {name}", nameof(GenerateTasksHandler));
            return Error.Failure("GenerateTasksHandler.Failure", ex.Message);
        }
    }
}
