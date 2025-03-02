namespace AktBob.CloudConvert;
internal class CloudConvertHandlers : ICloudConvertHandlers
{
    public CloudConvertHandlers(
        IConvertHtmlToPdfHandler convertHtmlToPdfHandler,
        IGetCloudConvertDownloadUrlHandler getCloudConvertDownloadUrlHandler,
        IGetCloudConvertFileHandler getCloudConvertFileHandler,
        IGenerateCloudConvertTasksHandler generateCloudConvertTasks)
    {
        ConvertHtmlToPdf = convertHtmlToPdfHandler;
        GetCloudConvertDownloadUrl = getCloudConvertDownloadUrlHandler;
        GetCloudConvertFile = getCloudConvertFileHandler;
        GenerateCloudConvertTasks = generateCloudConvertTasks;
    }

    public IConvertHtmlToPdfHandler ConvertHtmlToPdf { get; }
    public IGetCloudConvertDownloadUrlHandler GetCloudConvertDownloadUrl { get; }
    public IGetCloudConvertFileHandler GetCloudConvertFile { get; }
    public IGenerateCloudConvertTasksHandler GenerateCloudConvertTasks { get; }
}
