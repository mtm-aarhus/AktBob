namespace AktBob.CloudConvert.Handlers.ConvertHtmlToPdf;
internal class ConvertHtmlToPdfHandlerException : IConvertHtmlToPdfHandler
{
    private readonly IConvertHtmlToPdfHandler _inner;
    private readonly ILogger<ConvertHtmlToPdfHandler> _logger;

    public ConvertHtmlToPdfHandlerException(IConvertHtmlToPdfHandler inner, ILogger<ConvertHtmlToPdfHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<Guid>> Handle(IReadOnlyDictionary<Guid, object> tasks, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.Handle(tasks, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error in {name}", nameof(ConvertHtmlToPdfHandler));
            return Error.Failure("CloudConvertConvertHtmlToPdfHandler.Failure", ex.Message);
        }
    }
}
