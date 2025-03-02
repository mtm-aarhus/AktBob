namespace AktBob.CloudConvert.Contracts;

public interface IGenerateCloudConvertTasksHandler
{
    Result<Dictionary<Guid, object>> Handle(IEnumerable<byte[]> items);
}