using Ardalis.Result;

namespace AktBob.CloudConvert.Contracts;
public interface ICloudConvertModule
{
    Task<Result<Guid>> ConvertHtmlToPdf(IReadOnlyDictionary<Guid, object> tasks, CancellationToken cancellationToken);
    Task<Result<byte[]>> GetFile(string url, CancellationToken cancellationToken = default);
    Task<Result<string>> GetDownloadUrl(Guid jobId, CancellationToken cancellationToken = default);
    Result<IReadOnlyDictionary<Guid, object>> GenerateTasks(IEnumerable<byte[]> items);
}
