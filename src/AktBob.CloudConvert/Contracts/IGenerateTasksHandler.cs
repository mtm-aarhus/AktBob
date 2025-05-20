namespace AktBob.CloudConvert.Contracts;

internal interface IGenerateTasksHandler
{
    ErrorOr<IReadOnlyDictionary<Guid, object>> Handle(IEnumerable<byte[]> items);
}