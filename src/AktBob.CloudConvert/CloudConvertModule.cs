namespace AktBob.CloudConvert;
internal class CloudConvertModule(
    IConvertHtmlToPdfHandler convertHtmlToPdfHandler,
    IGetCloudConvertDownloadUrlHandler getCloudConvertDownloadUrlHandler,
    IGetCloudConvertFileHandler getCloudConvertFileHandler,
    IGenerateCloudConvertTasksHandler generateCloudConvertTasks) : ICloudConvertModule
{
    private readonly IConvertHtmlToPdfHandler _convertHtmlToPdfHandler = convertHtmlToPdfHandler;
    private readonly IGetCloudConvertDownloadUrlHandler _getCloudConvertDownloadUrlHandler = getCloudConvertDownloadUrlHandler;
    private readonly IGetCloudConvertFileHandler _getCloudConvertFileHandler = getCloudConvertFileHandler;
    private readonly IGenerateCloudConvertTasksHandler _generateCloudConvertTasks = generateCloudConvertTasks;

    public async Task<Result<Guid>> ConvertHtmlToPdf(IReadOnlyDictionary<Guid, object> tasks, CancellationToken cancellationToken) => await _convertHtmlToPdfHandler.Handle(tasks, cancellationToken);

    public Result<IReadOnlyDictionary<Guid, object>> GenerateTasks(IEnumerable<byte[]> items) => _generateCloudConvertTasks.Handle(items);

    public async Task<Result<string>> GetDownloadUrl(Guid jobId, CancellationToken cancellationToken = default) => await _getCloudConvertDownloadUrlHandler.Handle(jobId, cancellationToken);

    public async Task<Result<byte[]>> GetFile(string url, CancellationToken cancellationToken = default) => await _getCloudConvertFileHandler.Handle(url, cancellationToken);
}
