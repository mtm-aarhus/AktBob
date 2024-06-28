using Ardalis.Result;
using MediatR;

namespace AktBob.CheckOCRScreeningStatus.UseCases.GetFileStatus;
internal record GetFileStatusQuery(Guid FileId) : IRequest<Result>;