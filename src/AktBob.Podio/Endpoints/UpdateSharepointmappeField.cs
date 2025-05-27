using AktBob.Podio.Contracts;
using AktBob.Shared.Types.Podio;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AktBob.Podio.Endpoints;
internal class UpdateSharepointmappeField(IPodioModule podio, IConfiguration configuration) : Endpoint<UpdatePodioFieldRequest>
{
    public override void Configure()
    {
        Put("/Podio/{ItemId}/Fields/Sharepointmappe", "/Podio/{ItemId}/SharepointmappeField");
        Options(x => x.WithTags("Podio"));
    }

    public override async Task HandleAsync(UpdatePodioFieldRequest req, CancellationToken ct)
    {
        var appId = configuration.GetValue<int>("Podio:AktindsigtApp:Id");
        var fieldId = configuration.GetValue<int>("Podio:AktindsigtApp:Fields:Sharepointmappe");
        var itemId = ItemId.Create(appId, req.ItemId);
        
        podio.UpdateTextField(itemId, fieldId, req.Value);
        await SendNoContentAsync(ct);
    }
}
