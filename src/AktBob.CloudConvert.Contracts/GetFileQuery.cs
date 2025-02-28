namespace AktBob.CloudConvert.Contracts;
public record GetFileQuery(string Url) : IRequest<Result<FileDto>>;