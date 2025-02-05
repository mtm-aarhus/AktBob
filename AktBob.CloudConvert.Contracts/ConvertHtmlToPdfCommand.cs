using AktBob.CloudConvert.Contracts.DTOs;

namespace AktBob.CloudConvert.Contracts;
public record ConvertHtmlToPdfCommand(IEnumerable<byte[]> Content) : Request<Result<ConvertHtmlToPdfResponseDto>>;