namespace AktBob.CloudConvert.Contracts;
public interface IConvertHtmlToPdfHandler
{
    Task<Result<Guid>> Handle(Dictionary<Guid, object> tasks, CancellationToken cancellationToken);
}