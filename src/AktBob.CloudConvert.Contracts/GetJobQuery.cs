namespace AktBob.CloudConvert.Contracts;
public record GetJobQuery(Guid JobId) : Request<Result<byte[]>>;