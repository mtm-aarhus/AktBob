using Ardalis.GuardClauses;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.CheckOCRScreeningStatus.UseCases.RegisterDocuments;
internal record RegisterFilesCommand : Request<Result>
{
    public Guid CaseId { get; }

    public RegisterFilesCommand(Guid caseId)
    {
        CaseId = Guard.Against.NullOrEmpty(caseId);
    }
}