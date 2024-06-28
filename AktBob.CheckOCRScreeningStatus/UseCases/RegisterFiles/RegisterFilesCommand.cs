using Ardalis.GuardClauses;
using Ardalis.Result;
using MediatR;

namespace AktBob.CheckOCRScreeningStatus.UseCases.RegisterDocuments;
internal record RegisterFilesCommand : IRequest<Result>
{
    public Guid CaseId { get; }

    public RegisterFilesCommand(Guid caseId)
    {
        CaseId = Guard.Against.NullOrEmpty(caseId);
    }
}