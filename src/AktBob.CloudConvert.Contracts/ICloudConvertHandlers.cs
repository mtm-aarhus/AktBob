namespace AktBob.CloudConvert.Contracts;
public interface ICloudConvertHandlers
{
    IConvertHtmlToPdfHandler ConvertHtmlToPdf { get; }
    IGetCloudConvertFileHandler GetCloudConvertFile { get; }
    IGetCloudConvertDownloadUrlHandler GetCloudConvertDownloadUrl { get; }
    IGenerateCloudConvertTasksHandler GenerateCloudConvertTasks { get; }
}
