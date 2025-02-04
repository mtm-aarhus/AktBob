namespace AktBob.CheckOCRScreeningStatus;
public class Case
{
    public Guid CaseId { get; }
    public long PodioItemId { get; }
    public bool PodioItemUpdated { get; set;  } = false;

    public List<File> Files { get; set; } = new List<File>();

    public Case(Guid caseId, long podioItemId)
    {
        CaseId = caseId;
        PodioItemId = podioItemId;
    }
}
