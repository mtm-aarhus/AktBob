namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;
public class Case
{
    public Guid FilArkivCaseId { get; }
    public PodioItemId PodioItemId { get; }
    public List<Guid> Files { get; set; } = new();

    public Case(Guid filArkivCaseId, PodioItemId podioItemId)
    {
        FilArkivCaseId = filArkivCaseId;
        PodioItemId = podioItemId;
    }
}