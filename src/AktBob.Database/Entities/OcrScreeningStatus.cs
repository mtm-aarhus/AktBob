namespace AktBob.Database.Entities;

public class OcrScreeningStatus
{
    public int Id { get; set; }
    public long PodioItemId { get; set; }
    public Guid FilArkivCaseId { get; set; }
    public Guid FilArkivFileId { get; set; }
    public DateTime? ProcessedAt { get; set; }
}