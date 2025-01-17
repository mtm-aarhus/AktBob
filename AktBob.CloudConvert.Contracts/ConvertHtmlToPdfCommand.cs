using Ardalis.Result;
using MediatR;

namespace AktBob.CloudConvert.Contracts;
public record ConvertHtmlToPdfCommand(string[] base64HTMLDocuments) : IRequest<Result<ConvertHtmlToPdfResponseDto>>;