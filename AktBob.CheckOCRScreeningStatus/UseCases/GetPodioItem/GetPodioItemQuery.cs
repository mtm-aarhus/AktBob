using AktBob.CheckOCRScreeningStatus.DTOs;
using Ardalis.Result;
using MediatR;

namespace AktBob.CheckOCRScreeningStatus.UseCases.GetPodioItem;
internal record GetPodioItemQuery(long ItemId) : IRequest<Result<PodioItemDto>>;
