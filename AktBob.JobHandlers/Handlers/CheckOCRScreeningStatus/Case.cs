namespace AktBob.JobHandlers.Handlers.CheckOCRScreeningStatus;
public class Case
{
    public Guid FilArkivCaseId { get; }
    public long PodioItemId { get; }
    public List<Guid> Files { get; set; } = new();

    public Case(Guid filArkivCaseId, long podioItemId)
    {
        FilArkivCaseId = filArkivCaseId;
        PodioItemId = podioItemId;
    }
}