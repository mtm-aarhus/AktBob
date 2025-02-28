using AktBob.Shared.CQRS;

namespace AktBob.CloudConvert.Contracts;
public record ConvertHtmlToPdfCommand(IEnumerable<byte[]> Content) : ICommand<Result<ConvertHtmlToPdfResponseDto>>;