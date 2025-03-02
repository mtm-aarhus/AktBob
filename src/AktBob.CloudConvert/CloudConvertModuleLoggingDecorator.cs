using AktBob.Email.Contracts;
using Microsoft.Extensions.Configuration;

namespace AktBob.CloudConvert;
internal class CloudConvertModuleLoggingDecorator : ICloudConvertModule
{
    private readonly ICloudConvertModule _inner;
    private readonly ILogger<CloudConvertModuleLoggingDecorator> _logger;
    private readonly ISendEmailHandler _sendEmail;
    private readonly string _emailNotificationReceiver;

    public CloudConvertModuleLoggingDecorator(ICloudConvertModule inner, ILogger<CloudConvertModuleLoggingDecorator> logger, ISendEmailHandler sendEmail, IConfiguration configuration)
    {
        _inner = inner;
        _logger = logger;
        _sendEmail = sendEmail;
        _emailNotificationReceiver = Guard.Against.NullOrEmpty(configuration.GetValue<string>("EmailNotificationReceiver"));
    }


    public async Task<Result<Guid>> ConvertHtmlToPdf(IReadOnlyDictionary<Guid, object> tasks, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting {name}. Task count: {taskCount}", nameof(ConvertHtmlToPdf), tasks.Count);

        var result = await _inner.ConvertHtmlToPdf(tasks, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("{name} failed: {error}", nameof(ConvertHtmlToPdf), result.Errors);
            await _sendEmail.Handle(_emailNotificationReceiver, $"{nameof(ConvertHtmlToPdf)} error", string.Join(", ", result.Errors), false, cancellationToken);
            return result;
        }

        _logger.LogInformation("{name} finished successfully. Result: jobId {id}", nameof(ConvertHtmlToPdf), result.Value);
        
        return result;
    }


    public Result<IReadOnlyDictionary<Guid, object>> GenerateTasks(IEnumerable<byte[]> items)
    {
        _logger.LogInformation("Starting {name}. Item count: {count}", nameof(GenerateTasks), items.Count());

        var result = _inner.GenerateTasks(items);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("{name} failed: {error}", nameof(GenerateTasks), result.Errors);
            _sendEmail.Handle(_emailNotificationReceiver, $"{nameof(GenerateTasks)} error", string.Join(", ", result.Errors), false, CancellationToken.None).GetAwaiter().GetResult();
            return result;
        }

        _logger.LogInformation("{name} finished successfully.", nameof(GenerateTasks));
        
        return result;
    }


    public async Task<Result<string>> GetDownloadUrl(Guid jobId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting {name}. JobId: {id}", nameof(GetDownloadUrl), jobId);

        var result = await _inner.GetDownloadUrl(jobId, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("{name} failed: {error}", nameof(GetDownloadUrl), result.Errors);
            await _sendEmail.Handle(_emailNotificationReceiver, $"{nameof(GetDownloadUrl)} error", string.Join(", ", result.Errors), false, cancellationToken);
            return result;
        }

        _logger.LogInformation("{name} finished successfully. JobId {id} download url: {url}", nameof(GetDownloadUrl), jobId, result.Value);
        
        return result;
    }


    public async Task<Result<byte[]>> GetFile(string url, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting {name}. Download url: {url}", nameof(GetFile), url);

        var result = await _inner.GetFile(url, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("{name} failed: {error}", nameof(GetFile), result.Errors);
            await _sendEmail.Handle(_emailNotificationReceiver, $"{nameof(GetFile)} error", string.Join(", ", result.Errors), false, cancellationToken);
            return result;
        }

        _logger.LogInformation("{name} finished successfully. {size} bytes from download url: {url}", nameof(GetFile), result.Value.Count(), url);
        
        return result;
    }
}
