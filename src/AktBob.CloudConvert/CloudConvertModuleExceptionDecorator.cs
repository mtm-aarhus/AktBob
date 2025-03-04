using AktBob.Email.Contracts;
using Microsoft.Extensions.Configuration;

namespace AktBob.CloudConvert;
internal class CloudConvertModuleExceptionDecorator : ICloudConvertModule
{
    private readonly ICloudConvertModule _inner;
    private readonly ILogger<CloudConvertModuleExceptionDecorator> _logger;
    private readonly IEmailModule _email;
    private readonly string _emailNotificationReceiver;

    public CloudConvertModuleExceptionDecorator(ICloudConvertModule inner, ILogger<CloudConvertModuleExceptionDecorator> logger, IEmailModule email, IConfiguration configuration)
    {
        _inner = inner;
        _logger = logger;
        _email = email;
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
            _email.Send(_emailNotificationReceiver, $"{nameof(ConvertHtmlToPdf)} failure", ex.Message);
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
            _email.Send(_emailNotificationReceiver, $"{nameof(GenerateTasks)} failure", ex.Message);
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
            _email.Send(_emailNotificationReceiver, $"{nameof(GetDownloadUrl)} failure", ex.Message);
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
            _email.Send(_emailNotificationReceiver, $"{nameof(GetFile)} failure", ex.Message);
            throw;
        }
    }
}