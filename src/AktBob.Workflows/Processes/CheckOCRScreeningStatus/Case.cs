namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;
public class Case
{
    public Guid FilArkivCaseId { get; }
    public PodioItemId PodioItemId { get; }
    public Dictionary<Guid, bool> Files { get; set; } = new();

    public Case(Guid filArkivCaseId, PodioItemId podioItemId)
    {
        FilArkivCaseId = filArkivCaseId;
        PodioItemId = podioItemId;
    }
}