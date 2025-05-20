using AktBob.CloudConvert.Handlers.ConvertHtmlToPdf;
using AktBob.CloudConvert.Handlers.DownloadFile;
using AktBob.CloudConvert.Handlers.GenerateTasks;
using AktBob.CloudConvert.Handlers.GetDownloadUrl;
using AktBob.Shared.Contracts.CloudConvert;

namespace AktBob.CloudConvert;
internal class CloudConvertModule(
    IConvertHtmlToPdfHandler convertHtmlToPdfHandler,
    IGetDownloadUrlHandler getDownloadUrlHandler,
    IDownloadFileHandler downloadFileHandler,
    IGenerateTasksHandler generateTasksHandler) : ICloudConvertModule
{
    public async Task<ErrorOr<Guid>> ConvertHtmlToPdf(IReadOnlyDictionary<Guid, object> tasks, CancellationToken cancellationToken) => await convertHtmlToPdfHandler.Handle(tasks, cancellationToken);

    public ErrorOr<IReadOnlyDictionary<Guid, object>> GenerateTasks(IEnumerable<byte[]> items) => generateTasksHandler.Handle(items);

    public async Task<ErrorOr<string>> GetDownloadUrl(Guid jobId, CancellationToken cancellationToken = default) => await getDownloadUrlHandler.Handle(jobId, cancellationToken);

    public async Task<ErrorOr<byte[]>> DownloadFile(string url, CancellationToken cancellationToken = default) => await downloadFileHandler.Handle(url, cancellationToken);
}
