namespace AktBob.Database.Entities;
public class OS2FormsSubmission
{
    public int Id { get; set; }
    public int? DeskproTicketId { get; set; }
    public Guid SubmissionId { get; set; }
    public string DescriptionFieldValue { get; set; } = string.Empty;
}
