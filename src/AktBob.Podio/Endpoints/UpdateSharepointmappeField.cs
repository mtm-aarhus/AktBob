using AktBob.Podio.Jobs;
using AktBob.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AktBob.Podio.Endpoints;
internal class UpdateSharepointmappeField(IJobDispatcher jobDispatcher, IConfiguration configuration) : Endpoint<UpdatePodioFieldRequest>
{
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;
    private readonly IConfiguration _configuration = configuration;

    public override void Configure()
    {
        Put("/Podio/{ItemId}/Fields/Sharepointmappe", "/Podio/{ItemId}/SharepointmappeField");
        Options(x => x.WithTags("Podio"));
    }

    public override async Task HandleAsync(UpdatePodioFieldRequest req, CancellationToken ct)
    {
        var appId = _configuration.GetValue<int>("Podio:AktindsigtApp:Id");
        var fieldId = _configuration.GetValue<int>("Podio:AktindsigtApp:Fields:Sharepointmappe");

        var job = new UpdateTextFieldJob(new PodioItemId(appId, req.ItemId), fieldId, req.Value);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
