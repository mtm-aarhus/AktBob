namespace AktBob.CloudConvert;
internal class CloudConvertHandlers : ICloudConvertHandlers
{
    public CloudConvertHandlers(
        IConvertHtmlToPdfHandler convertHtmlToPdfHandler,
        IGetCloudConvertJobHandler getCloudConvertJobHandler,
        IGetCloudConvertFileHandler getCloudConvertFileHandler)
    {
        ConvertHtmlToPdf = convertHtmlToPdfHandler;
        GetCloudConvertJob = getCloudConvertJobHandler;
        GetCloudConvertFile = getCloudConvertFileHandler;
    }

    public IConvertHtmlToPdfHandler ConvertHtmlToPdf { get; }
    public IGetCloudConvertJobHandler GetCloudConvertJob { get; }
    public IGetCloudConvertFileHandler GetCloudConvertFile { get; }
}
