using AktBob.CloudConvert.Contracts.DTOs;
using Ardalis.Result;
using MediatR;

namespace AktBob.CloudConvert.Contracts;
public record GetFileQuery(string Url) : IRequest<Result<FileDto>>;