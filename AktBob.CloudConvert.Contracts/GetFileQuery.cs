using AktBob.CloudConvert.Contracts.DTOs;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.CloudConvert.Contracts;
public record GetFileQuery(string Url) : Request<Result<FileDto>>;