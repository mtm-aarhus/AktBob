using AktBob.Shared.CQRS;

namespace AktBob.CloudConvert.Contracts;
public record GetJobQuery(Guid JobId) : IQuery<Result<byte[]>>;