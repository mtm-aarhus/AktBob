namespace AktBob.CloudConvert;
internal class ModuleExceptionDecorator : ICloudConvertModule
{
    private readonly ICloudConvertModule _inner;
    private readonly ILogger<CloudConvertModule> _logger;

    public ModuleExceptionDecorator(ICloudConvertModule inner, ILogger<CloudConvertModule> logger)
    {
        _inner = inner;
        _logger = logger;
    }


    public async Task<ErrorOr<Guid>> ConvertHtmlToPdf(IReadOnlyDictionary<Guid, object> tasks, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.ConvertHtmlToPdf(tasks, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(ConvertHtmlToPdf));
            throw;
        }
    }

    public ErrorOr<IReadOnlyDictionary<Guid, object>> GenerateTasks(IEnumerable<byte[]> items)
    {
        try
        {
            return _inner.GenerateTasks(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GenerateTasks));
            throw;
        }
    }

    public async Task<ErrorOr<string>> GetDownloadUrl(Guid jobId, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.GetDownloadUrl(jobId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetDownloadUrl));
            throw;
        }
    }

    public async Task<ErrorOr<byte[]>> DownloadFile(string url, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.DownloadFile(url, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(DownloadFile));
            throw;
        }
    }
}