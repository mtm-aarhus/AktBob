using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.GetOrganized.Contracts;
public record CreateCaseCommand(string CaseTypePrefix, string CaseTitle, string Description, string Status, string Access) : Request<Result<CreateCaseResponse>>;
