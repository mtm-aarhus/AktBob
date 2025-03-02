namespace AktBob.CloudConvert.Contracts;

internal interface IGenerateCloudConvertTasksHandler
{
    Result<IReadOnlyDictionary<Guid, object>> Handle(IEnumerable<byte[]> items);
}