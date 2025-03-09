using AktBob.Podio.Jobs;
using AktBob.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AktBob.Podio.Endpoints;
internal class UpdateDokumentlisteField(IConfiguration configuration, IJobDispatcher jobDispatcher) : Endpoint<UpdatePodioFieldRequest>
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IJobDispatcher _jobDispatcher = jobDispatcher;

    public override void Configure()
    {
        Put("/Podio/{ItemId}/Fields/Dokumentliste", "/Podio/{ItemId}/DokumentlisteField");
        Options(x => x.WithTags("Podio"));
    }

    public async override Task HandleAsync(UpdatePodioFieldRequest req, CancellationToken ct)
    {
        var appId = _configuration.GetValue<int>("Podio:AktindsigtApp:Id");
        var fieldId = _configuration.GetValue<int>("Podio:AktindsigtApp:Fields:Dokumentliste");

        var job = new UpdateTextFieldJob(new PodioItemId(appId, req.ItemId), fieldId, req.Value);
        _jobDispatcher.Dispatch(job);
        await SendNoContentAsync(ct);
    }
}
