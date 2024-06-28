using Ardalis.Result;
using MediatR;

namespace AktBob.CheckOCRScreeningStatus.UseCases.RemoveCaseFromCache;

internal record RemoveCaseFromCacheCommand(Guid CaseId) : IRequest<Result>;