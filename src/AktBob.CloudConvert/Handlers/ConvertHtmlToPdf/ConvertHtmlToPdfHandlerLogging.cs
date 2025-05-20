using AktBob.Shared.Extensions;

namespace AktBob.CloudConvert.Handlers.ConvertHtmlToPdf;
internal class ConvertHtmlToPdfHandlerLogging(IConvertHtmlToPdfHandler inner, ILogger<ConvertHtmlToPdfHandler> logger) : IConvertHtmlToPdfHandler
{
    private readonly IConvertHtmlToPdfHandler _inner = inner;
    private readonly ILogger<ConvertHtmlToPdfHandler> _logger = logger;

    public async Task<ErrorOr<Guid>> Handle(IReadOnlyDictionary<Guid, object> tasks, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Converting HTML to PDF");

        var result = await _inner.Handle(tasks, cancellationToken);

        result.Switch(
            value => _logger.LogInformation("HTML to PDF conversion completed"),
            errors => _logger.LogWarning("{name}: {error}", nameof(ConvertHtmlToPdfHandler), errors.ToCommaDelimitedString()));

        return result;
    }
}