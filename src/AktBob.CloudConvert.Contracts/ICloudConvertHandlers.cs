namespace AktBob.CloudConvert.Contracts;
public interface ICloudConvertHandlers
{
    IConvertHtmlToPdfHandler ConvertHtmlToPdf { get; }
    IGetCloudConvertFileHandler GetCloudConvertFile { get; }
    IGetCloudConvertJobHandler GetCloudConvertJob { get; }
}
