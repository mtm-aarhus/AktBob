using AktBob.CloudConvert.Models.JobResponse;

namespace AktBob.CloudConvert;
public interface ICloudConvertClient
{
    Task<Result<Guid>> CreateJob(object payload, CancellationToken cancellationToken = default);
    Task<Result<Models.File>> GetFile(string url, CancellationToken cancellationToken = default);
    Task<Result<JobResponseRoot?>> GetJob(Guid jobId, CancellationToken cancellationToken = default);
}