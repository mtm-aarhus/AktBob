using AktBob.DatabaseAPI.Contracts.DTOs;
using Ardalis.Result;
using MediatR;

namespace AktBob.DatabaseAPI.Contracts.Commands;
public record UpdateCaseSetFilArkivCaseIdCommand(long PodioItemId, Guid FilArkivCaseId) : IRequest<Result<CaseDto>>;
