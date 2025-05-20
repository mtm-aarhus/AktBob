using AktBob.Shared.Extensions;

namespace AktBob.CloudConvert;
internal class ModuleLoggingDecorator : ICloudConvertModule
{
    private readonly ICloudConvertModule _inner;
    private readonly ILogger<CloudConvertModule> _logger;

    public ModuleLoggingDecorator(ICloudConvertModule inner, ILogger<CloudConvertModule> logger)
    {
        _inner = inner;
        _logger = logger;
    }


    public async Task<ErrorOr<Guid>> ConvertHtmlToPdf(IReadOnlyDictionary<Guid, object> tasks, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Converting HTML to PDF");

        var result = await _inner.ConvertHtmlToPdf(tasks, cancellationToken);

        if (result.IsError)
        {
            _logger.LogWarning("{name}: {error}", nameof(ConvertHtmlToPdf), result.Errors.ToCommaDelimitedString());
        }
        
        return result;
    }


    public ErrorOr<IReadOnlyDictionary<Guid, object>> GenerateTasks(IEnumerable<byte[]> items)
    {
        _logger.LogInformation("Generating CloudConvert tasks");

        var result = _inner.GenerateTasks(items);

        if (result.IsError)
        {
            _logger.LogWarning("{name}: {error}", nameof(GenerateTasks), result.Errors.ToCommaDelimitedString());
        }

        return result;
    }


    public async Task<ErrorOr<string>> GetDownloadUrl(Guid jobId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting download url (CloudConvert jobId {id})", jobId);

        var result = await _inner.GetDownloadUrl(jobId, cancellationToken);

        if (result.IsError)
        {
            _logger.LogWarning("{name}: {error}", nameof(GetDownloadUrl), result.Errors.ToCommaDelimitedString());
        }
        
        return result;
    }


    public async Task<ErrorOr<byte[]>> DownloadFile(string url, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading file {url}", url);

        var result = await _inner.DownloadFile(url, cancellationToken);

        if (result.IsError)
        {
            _logger.LogWarning("{name}: {error}", nameof(DownloadFile), result.Errors.ToCommaDelimitedString());
        }
        
        return result;
    }
}
