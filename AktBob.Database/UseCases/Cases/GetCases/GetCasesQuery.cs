using AktBob.Database.Entities;
using Ardalis.Result;
using MediatR;

namespace AktBob.Database.UseCases.Cases.GetCases;
internal record GetCasesQuery(int? DeskproId, long? PodioItemId, Guid? FilArkivCaseId) : IRequest<Result<IEnumerable<Case>>>;