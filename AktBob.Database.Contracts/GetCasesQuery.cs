using AktBob.Database.Contracts.Dtos;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.UseCases.Cases.GetCases;
public record GetCasesQuery(int? DeskproId, long? PodioItemId, Guid? FilArkivCaseId) : Request<Result<IEnumerable<CaseDto>>>;