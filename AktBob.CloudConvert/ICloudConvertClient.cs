using Ardalis.Result;

namespace AktBob.CloudConvert;
internal interface ICloudConvertClient
{
    Task<Result<Guid>> CreateJob(object payload, CancellationToken cancellationToken);
}