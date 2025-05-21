using AktBob.Shared.Types.Deskpro;

namespace AktBob.Shared.Jobs;
public record RegisterOS2FormsSubmissionJob(Guid SubmissionId, TicketId TicketId);