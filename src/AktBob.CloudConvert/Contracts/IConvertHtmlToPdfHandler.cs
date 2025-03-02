namespace AktBob.CloudConvert.Contracts;
internal interface IConvertHtmlToPdfHandler
{
    Task<Result<Guid>> Handle(IReadOnlyDictionary<Guid, object> tasks, CancellationToken cancellationToken);
}