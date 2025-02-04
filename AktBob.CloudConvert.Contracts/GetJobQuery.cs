using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.CloudConvert.Contracts;
public record GetJobQuery(Guid JobId) : Request<Result<byte[]>>;