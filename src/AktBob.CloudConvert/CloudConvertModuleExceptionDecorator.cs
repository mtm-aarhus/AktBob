using AktBob.Email.Contracts;
using AktBob.Shared;
using Microsoft.Extensions.Configuration;

namespace AktBob.CloudConvert;
internal class CloudConvertModuleExceptionDecorator : ICloudConvertModule
{
    private readonly ICloudConvertModule _inner;
    private readonly ILogger<CloudConvertModuleExceptionDecorator> _logger;
    private readonly IJobDispatcher jobDispatcher;
    private readonly string _emailNotificationReceiver;

    public CloudConvertModuleExceptionDecorator(ICloudConvertModule inner, ILogger<CloudConvertModuleExceptionDecorator> logger, IJobDispatcher jobDispatcher, IConfiguration configuration)
    {
        _inner = inner;
        _logger = logger;
        this.jobDispatcher = jobDispatcher;
        _emailNotificationReceiver = Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailNotificationReceiver"));
    }


    public async Task<Result<Guid>> ConvertHtmlToPdf(IReadOnlyDictionary<Guid, object> tasks, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.ConvertHtmlToPdf(tasks, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(ConvertHtmlToPdf));
            jobDispatcher.Dispatch(new SendEmailJob(_emailNotificationReceiver, $"{nameof(ConvertHtmlToPdf)} failure", ex.Message));
            throw;
        }
    }

    public Result<IReadOnlyDictionary<Guid, object>> GenerateTasks(IEnumerable<byte[]> items)
    {
        try
        {
            return _inner.GenerateTasks(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GenerateTasks));
            jobDispatcher.Dispatch(new SendEmailJob(_emailNotificationReceiver, $"{nameof(GenerateTasks)} failure", ex.Message));
            throw;
        }
    }

    public async Task<Result<string>> GetDownloadUrl(Guid jobId, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.GetDownloadUrl(jobId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetDownloadUrl));
            jobDispatcher.Dispatch(new SendEmailJob(_emailNotificationReceiver, $"{nameof(GetDownloadUrl)} failure", ex.Message));
            throw;
        }
    }

    public async Task<Result<byte[]>> GetFile(string url, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.GetFile(url, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetFile));
            jobDispatcher.Dispatch(new SendEmailJob(_emailNotificationReceiver, $"{nameof(GetFile)} failure", ex.Message));
            throw;
        }
    }
}