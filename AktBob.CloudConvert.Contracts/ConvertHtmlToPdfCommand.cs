using AktBob.CloudConvert.Contracts.DTOs;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.CloudConvert.Contracts;
public record ConvertHtmlToPdfCommand(IEnumerable<byte[]> Content) : Request<Result<ConvertHtmlToPdfResponseDto>>;