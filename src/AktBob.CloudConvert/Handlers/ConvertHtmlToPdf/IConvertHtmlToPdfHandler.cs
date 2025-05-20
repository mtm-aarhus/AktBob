namespace AktBob.CloudConvert.Handlers.ConvertHtmlToPdf;

internal interface IConvertHtmlToPdfHandler
{
    Task<ErrorOr<Guid>> Handle(IReadOnlyDictionary<Guid, object> tasks, CancellationToken cancellationToken);
}
