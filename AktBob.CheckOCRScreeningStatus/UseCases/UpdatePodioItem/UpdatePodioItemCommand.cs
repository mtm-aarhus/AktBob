using Ardalis.Result;
using MediatR;


namespace AktBob.CheckOCRScreeningStatus.UseCases.UpdatePodioItem;
internal record UpdatePodioItemCommand(Guid CaseId) : IRequest<Result>;