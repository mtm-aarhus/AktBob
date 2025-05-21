using AktBob.Shared.Types.Deskpro;

namespace AktBob.Api.Endpoints.RegisterOS2FormsSubmission;

internal record RegisterOS2FormsSubmissionRequest(TicketId DeskproTicketId, Guid OS2FormsSubmissionId);
