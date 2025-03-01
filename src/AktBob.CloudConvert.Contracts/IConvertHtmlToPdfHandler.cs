namespace AktBob.CloudConvert.Contracts;
public interface IConvertHtmlToPdfHandler
{
    Task<Result<Guid>> Handle(IEnumerable<byte[]> content, CancellationToken cancellationToken);
}