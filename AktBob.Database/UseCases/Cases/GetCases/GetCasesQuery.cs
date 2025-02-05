using AktBob.Database.Entities;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.UseCases.Cases.GetCases;
internal record GetCasesQuery(int? DeskproId, long? PodioItemId, Guid? FilArkivCaseId) : Request<Result<IEnumerable<Case>>>;