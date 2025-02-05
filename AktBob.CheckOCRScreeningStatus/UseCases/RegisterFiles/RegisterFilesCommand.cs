namespace AktBob.CheckOCRScreeningStatus.UseCases.RegisterFiles;
public record RegisterFilesCommand : Request<Result>
{
    public Guid CaseId { get; }

    public RegisterFilesCommand(Guid caseId)
    {
        CaseId = Guard.Against.NullOrEmpty(caseId);
    }
}