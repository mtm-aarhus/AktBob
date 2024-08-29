namespace AktBob.DatabaseAPI.Contracts;
public record CaseDto
{
    public int Id { get; set; }
    public long PodioItemId { get; set; }
    public string Sagsnummer { get; set; } = string.Empty;
    public Guid? FilArkivCaseId { get; set; }
}