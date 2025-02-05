using AktBob.Database.Contracts.Dtos;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Database.UseCases.Cases.GetCaseById;
internal record GetCaseByIdQuery(int Id) : Request<Result<CaseDto>>;
