using AktBob.Database.Entities;

namespace AktBob.Database.Contracts;
public interface IOS2FormsSubmissionRepository
{
    Task<bool> Add(OS2FormsSubmission submission);
    Task<OS2FormsSubmission?> GetBySubmissionId(Guid submissionId);
    Task<OS2FormsSubmission?> GetByDeskproTicketId(int  deskproTicketId);
}
