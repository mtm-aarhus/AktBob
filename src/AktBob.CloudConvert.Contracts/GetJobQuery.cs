namespace AktBob.CloudConvert.Contracts;
public record GetJobQuery(Guid JobId) : IRequest<Result<byte[]>>;