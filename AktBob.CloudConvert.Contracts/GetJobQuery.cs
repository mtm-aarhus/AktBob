using AktBob.CloudConvert.Contracts.DTOs;
using Ardalis.Result;
using MediatR;

namespace AktBob.CloudConvert.Contracts;
public record GetJobQuery(Guid JobId) : IRequest<Result<JobDto>>;