using ErrorOr;

namespace AktBob.Shared.Contracts;
public interface ICloudConvertModule
{
    Task<ErrorOr<Guid>> ConvertHtmlToPdf(IReadOnlyDictionary<Guid, object> tasks, CancellationToken cancellationToken);
    Task<ErrorOr<byte[]>> DownloadFile(string url, CancellationToken cancellationToken = default);
    Task<ErrorOr<string>> GetDownloadUrl(Guid jobId, CancellationToken cancellationToken = default);
    ErrorOr<IReadOnlyDictionary<Guid, object>> GenerateTasks(IEnumerable<byte[]> items);
}
