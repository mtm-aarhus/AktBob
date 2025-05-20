namespace AktBob.CloudConvert.Contracts;
internal interface IConvertHtmlToPdfHandler
{
    Task<ErrorOr<Guid>> Handle(IReadOnlyDictionary<Guid, object> tasks, CancellationToken cancellationToken);
}