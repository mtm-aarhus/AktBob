namespace AktBob.CloudConvert.Contracts;

internal interface IGenerateTasksHandler
{
    Result<IReadOnlyDictionary<Guid, object>> Handle(IEnumerable<byte[]> items);
}