using AktBob.DatabaseAPI.Contracts.DTOs;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.DatabaseAPI.Contracts.Commands;
public record UpdateCaseSetFilArkivCaseIdCommand(long PodioItemId, Guid FilArkivCaseId) : Request<Result<CaseDto>>;
