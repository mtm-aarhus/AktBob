using AktBob.Shared.CQRS;

namespace AktBob.CloudConvert.Contracts;
public record GetFileQuery(string Url) : IQuery<Result<FileDto>>;