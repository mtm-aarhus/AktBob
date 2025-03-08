using AktBob.GetOrganized.Contracts.DTOs;
using Ardalis.Result;

namespace AktBob.GetOrganized.Contracts;
internal interface ICreateCaseHandler
{
    Task<Result<CreateCaseResponse>> Handle(CreateGetOrganizedCaseCommand command, CancellationToken cancellationToken);
}