using AktBob.Podio.Contracts;
using AktBob.Shared.CQRS;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AktBob.Podio.Endpoints;
internal class UpdateDokumentlisteField(IConfiguration configuration, ICommandDispatcher commandDispatcher) : Endpoint<UpdatePodioFieldRequest>
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ICommandDispatcher _commandDispatcher = commandDispatcher;

    public override void Configure()
    {
        Put("/Podio/{ItemId}/Fields/Dokumentliste", "/Podio/{ItemId}/DokumentlisteField");
        Options(x => x.WithTags("Podio"));
    }

    public async override Task HandleAsync(UpdatePodioFieldRequest req, CancellationToken ct)
    {
        var appId = _configuration.GetValue<int>("Podio:AktindsigtApp:Id");
        var fieldId = _configuration.GetValue<int>("Podio:AktindsigtApp:Fields:Dokumentliste");

        var command = new UpdateFieldCommand(appId, req.ItemId, fieldId, req.Value);
        await _commandDispatcher.Dispatch(command, ct);

        await SendNoContentAsync(ct);
    }
}
