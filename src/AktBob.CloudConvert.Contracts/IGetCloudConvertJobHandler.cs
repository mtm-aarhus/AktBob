namespace AktBob.CloudConvert.Contracts;
public interface IGetCloudConvertJobHandler
{
    Task<Result<byte[]>> Handle(Guid jobId, CancellationToken cancellationToken = default);
}