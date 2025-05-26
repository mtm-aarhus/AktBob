namespace AktBob.CloudConvert.Handlers.GenerateTasks;

internal interface IGenerateTasksHandler
{
    ErrorOr<IReadOnlyDictionary<Guid, object>> Handle(IEnumerable<byte[]> items);
}
