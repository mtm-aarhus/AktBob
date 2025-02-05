using AktBob.CloudConvert.Contracts.DTOs;

namespace AktBob.CloudConvert.Contracts;
public record GetFileQuery(string Url) : Request<Result<FileDto>>;