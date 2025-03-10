namespace AktBob.CloudConvert;
internal class CloudConvertModuleLoggingDecorator : ICloudConvertModule
{
    private readonly ICloudConvertModule _inner;
    private readonly ILogger<CloudConvertModule> _logger;

    public CloudConvertModuleLoggingDecorator(ICloudConvertModule inner, ILogger<CloudConvertModule> logger)
    {
        _inner = inner;
        _logger = logger;
    }


    public async Task<Result<Guid>> ConvertHtmlToPdf(IReadOnlyDictionary<Guid, object> tasks, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Converting HTML to PDF");

        var result = await _inner.ConvertHtmlToPdf(tasks, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogError("{name} failed: {error}", nameof(ConvertHtmlToPdf), result.Errors);
            return result;
        }
        
        return result;
    }


    public Result<IReadOnlyDictionary<Guid, object>> GenerateTasks(IEnumerable<byte[]> items)
    {
        _logger.LogInformation("Generating CloudConvert tasks");

        var result = _inner.GenerateTasks(items);

        if (!result.IsSuccess)
        {
            _logger.LogError("{name} failed: {error}", nameof(GenerateTasks), result.Errors);
            return result;
        }

        return result;
    }


    public async Task<Result<string>> GetDownloadUrl(Guid jobId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting download url (CloudConvert jobId {id})", jobId);

        var result = await _inner.GetDownloadUrl(jobId, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogError("{name} failed: {error}", nameof(GetDownloadUrl), result.Errors);
            return result;
        }
        
        return result;
    }


    public async Task<Result<byte[]>> GetFile(string url, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading file {url}", url);

        var result = await _inner.GetFile(url, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogError("{name} failed: {error}", nameof(GetFile), result.Errors);
            return result;
        }
        
        return result;
    }
}
