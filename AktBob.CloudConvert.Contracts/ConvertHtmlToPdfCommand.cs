using AktBob.CloudConvert.Contracts.DTOs;
using Ardalis.Result;
using MediatR;

namespace AktBob.CloudConvert.Contracts;
public record ConvertHtmlToPdfCommand(IEnumerable<byte[]> Content) : IRequest<Result<ConvertHtmlToPdfResponseDto>>;