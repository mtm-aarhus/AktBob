using Ardalis.Result;
using MediatR;

namespace AktBob.DatabaseAPI.Contracts;
public record UpdateCaseSetFilArkivCaseIdCommand(long PodioItemId, Guid FilArkivCaseId) : IRequest<Result<CaseDto>>;
