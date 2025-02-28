namespace AktBob.CloudConvert.Contracts;
public record ConvertHtmlToPdfCommand(IEnumerable<byte[]> Content) : IRequest<Result<ConvertHtmlToPdfResponseDto>>;