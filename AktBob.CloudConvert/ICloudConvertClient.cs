using AktBob.CloudConvert.Models.JobResponse;
using Ardalis.Result;

namespace AktBob.CloudConvert;
internal interface ICloudConvertClient
{
    Task<Result<Guid>> CreateJob(object payload, CancellationToken cancellationToken);
    Task<Result<Models.File>> GetFile(string url, CancellationToken cancellationToken = default);
    Task<Result<JobResponseRoot?>> GetJob(Guid jobId, CancellationToken cancellationToken = default);
}