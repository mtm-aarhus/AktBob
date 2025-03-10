namespace AktBob.CloudConvert;
internal class CloudConvertModule(
    IConvertHtmlToPdfHandler convertHtmlToPdfHandler,
    IGetCloudConvertDownloadUrlHandler getCloudConvertDownloadUrlHandler,
    IGetCloudConvertFileHandler getCloudConvertFileHandler,
    IGenerateCloudConvertTasksHandler generateCloudConvertTasks) : ICloudConvertModule
{
    public async Task<Result<Guid>> ConvertHtmlToPdf(IReadOnlyDictionary<Guid, object> tasks, CancellationToken cancellationToken) => await convertHtmlToPdfHandler.Handle(tasks, cancellationToken);

    public Result<IReadOnlyDictionary<Guid, object>> GenerateTasks(IEnumerable<byte[]> items) => generateCloudConvertTasks.Handle(items);

    public async Task<Result<string>> GetDownloadUrl(Guid jobId, CancellationToken cancellationToken = default) => await getCloudConvertDownloadUrlHandler.Handle(jobId, cancellationToken);

    public async Task<Result<byte[]>> GetFile(string url, CancellationToken cancellationToken = default) => await getCloudConvertFileHandler.Handle(url, cancellationToken);
}
