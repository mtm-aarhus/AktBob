namespace AktBob.CloudConvert;
internal class CloudConvertModule(
    IConvertHtmlToPdfHandler convertHtmlToPdfHandler,
    IGetDownloadUrlHandler getDownloadUrlHandler,
    IDownloadFileHandler downloadFileHandler,
    IGenerateTasksHandler generateTasksHandler) : ICloudConvertModule
{
    public async Task<Result<Guid>> ConvertHtmlToPdf(IReadOnlyDictionary<Guid, object> tasks, CancellationToken cancellationToken) => await convertHtmlToPdfHandler.Handle(tasks, cancellationToken);

    public Result<IReadOnlyDictionary<Guid, object>> GenerateTasks(IEnumerable<byte[]> items) => generateTasksHandler.Handle(items);

    public async Task<Result<string>> GetDownloadUrl(Guid jobId, CancellationToken cancellationToken = default) => await getDownloadUrlHandler.Handle(jobId, cancellationToken);

    public async Task<Result<byte[]>> DownloadFile(string url, CancellationToken cancellationToken = default) => await downloadFileHandler.Handle(url, cancellationToken);
}
