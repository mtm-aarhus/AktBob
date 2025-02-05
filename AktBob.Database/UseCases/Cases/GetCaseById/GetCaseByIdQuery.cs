using AktBob.Database.Entities;
using Ardalis.Result;
using MediatR;

namespace AktBob.Database.UseCases.Cases.GetCaseById;
internal record GetCaseByIdQuery(int Id) : IRequest<Result<Case>>;
