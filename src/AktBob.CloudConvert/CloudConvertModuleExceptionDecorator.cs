using AktBob.Email.Contracts;
using Microsoft.Extensions.Configuration;

namespace AktBob.CloudConvert;
internal class CloudConvertModuleExceptionDecorator : ICloudConvertModule
{
    private readonly ICloudConvertModule _inner;
    private readonly ILogger<CloudConvertModuleExceptionDecorator> _logger;
    private readonly ISendEmailHandler _sendEmail;
    private readonly string _emailNotificationReceiver;

    public CloudConvertModuleExceptionDecorator(ICloudConvertModule inner, ILogger<CloudConvertModuleExceptionDecorator> logger, ISendEmailHandler sendEmail, IConfiguration configuration)
    {
        _inner = inner;
        _logger = logger;
        _sendEmail = sendEmail;
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
            await _sendEmail.Handle(_emailNotificationReceiver, $"{nameof(ConvertHtmlToPdf)} failure", ex.Message, false, cancellationToken);
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
            _sendEmail.Handle(_emailNotificationReceiver, $"{nameof(GenerateTasks)} failure", ex.Message, false, CancellationToken.None).GetAwaiter().GetResult();
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
            await _sendEmail.Handle(_emailNotificationReceiver, $"{nameof(GetDownloadUrl)} failure", ex.Message, false, cancellationToken);
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
            await _sendEmail.Handle(_emailNotificationReceiver, $"{nameof(GetFile)} failure", ex.Message, false, cancellationToken);
            throw;
        }
    }
}