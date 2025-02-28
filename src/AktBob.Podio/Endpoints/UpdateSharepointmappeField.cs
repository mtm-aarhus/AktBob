using AktBob.Podio.Contracts;
using AktBob.Shared.CQRS;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AktBob.Podio.Endpoints;
internal class UpdateSharepointmappeField(ICommandDispatcher commandDispatcher, IConfiguration configuration) : Endpoint<UpdatePodioFieldRequest>
{
    private readonly ICommandDispatcher _commandDispatcher = commandDispatcher;
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

        var command = new UpdateFieldCommand(appId, req.ItemId, fieldId, req.Value);
        await _commandDispatcher.Dispatch(command);

        await SendNoContentAsync(ct);
    }
}
