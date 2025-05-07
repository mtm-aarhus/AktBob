using AktBob.Shared;
using AktBob.Shared.Jobs;
using FastEndpoints;

namespace AktBob.Api.Endpoints.DeleteFilArkivFiles;

internal class DeleteFilArkivFilesEndpoint : Endpoint<DeleteFilArkivFilesRequest>
{
    private readonly IJobDispatcher _jobDispatcher;

    public DeleteFilArkivFilesEndpoint(IJobDispatcher jobDispatcher)
    {
        _jobDispatcher = jobDispatcher;
    }

    public override void Configure()
    {
        Post("/Jobs/QueueFilArkivFilesForDeletion");
        Options(x => x.WithTags("Jobs"));
        Summary(s =>
        {
            s.Summary = "Enqueue FilArkiv files to be deleted from internal file storage";
        });
    }

    public override async Task HandleAsync(DeleteFilArkivFilesRequest req, CancellationToken ct)
    {
        foreach (var file in req.Files)
        {
            var job = new RegisterCleanedUpFilArkivFileJob(file);
            _jobDispatcher.Dispatch(job);
        }

        await SendNoContentAsync(ct);
    }
}
