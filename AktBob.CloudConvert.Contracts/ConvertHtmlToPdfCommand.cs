using AktBob.CloudConvert.Contracts.DTOs;
using Ardalis.Result;
using MediatR;

namespace AktBob.CloudConvert.Contracts;
public record ConvertHtmlToPdfCommand(IEnumerable<byte[]> base64HTMLDocuments) : IRequest<Result<ConvertHtmlToPdfResponseDto>>;