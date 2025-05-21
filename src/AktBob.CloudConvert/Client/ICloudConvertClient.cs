using AktBob.CloudConvert.Client.Models.JobResponse;

namespace AktBob.CloudConvert.Client;
internal interface ICloudConvertClient
{
    Task<ErrorOr<Guid>> CreateJob(object payload, CancellationToken cancellationToken = default);
    Task<ErrorOr<byte[]>> GetFile(string url, CancellationToken cancellationToken = default);
    Task<ErrorOr<JobResponseRoot>> GetJob(Guid jobId, CancellationToken cancellationToken = default);
}