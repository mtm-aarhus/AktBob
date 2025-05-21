using AktBob.Shared;
using AktBob.Shared.Jobs;
using AktBob.Shared.Types.Deskpro;
using FastEndpoints;

namespace AktBob.Api.Endpoints.RegisterOS2FormsSubmission;

internal class RegisterOS2FormsSubmissionEndpoint : Endpoint<RegisterOS2FormsSubmissionRequest>
{
    private readonly IJobDispatcher _jobDispatcher;

    public RegisterOS2FormsSubmissionEndpoint(IJobDispatcher jobDispatcher)
    {
        _jobDispatcher = jobDispatcher;
    }

    public override void Configure()
    {
        Post("/Jobs/OS2FormsSubmission");
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Registers the OS2Forms submission with the related Deskpro ticekt ID";
        });
    }

    public override async Task HandleAsync(RegisterOS2FormsSubmissionRequest req, CancellationToken ct)
    {
        var ticketId = TicketId.Create(req.DeskproTicketId);
        var job = new RegisterOS2FormsSubmissionJob(req.OS2FormsSubmissionId, ticketId);
        _jobDispatcher.Dispatch(job);

        await SendNoContentAsync(ct);
    }
}
