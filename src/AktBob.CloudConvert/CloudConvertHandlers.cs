namespace AktBob.CloudConvert;
internal class CloudConvertHandlers : ICloudConvertHandlers
{
    public CloudConvertHandlers(
        IConvertHtmlToPdfHandler convertHtmlToPdfHandler,
        IGetCloudConvertDownloadUrlHandler getCloudConvertDownloadUrlHandler,
        IGetCloudConvertFileHandler getCloudConvertFileHandler)
    {
        ConvertHtmlToPdf = convertHtmlToPdfHandler;
        GetCloudConvertDownloadUrl = getCloudConvertDownloadUrlHandler;
        GetCloudConvertFile = getCloudConvertFileHandler;
    }

    public IConvertHtmlToPdfHandler ConvertHtmlToPdf { get; }
    public IGetCloudConvertDownloadUrlHandler GetCloudConvertDownloadUrl { get; }
    public IGetCloudConvertFileHandler GetCloudConvertFile { get; }
}
