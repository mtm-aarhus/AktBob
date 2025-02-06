using AktBob.Podio.Contracts;
using FastEndpoints;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AktBob.Podio.Endpoints;
internal record UpdatePodioFieldRequest(long ItemId, string Value);

internal class UpdateFilArkivField(IMediator mediator, IConfiguration configuration) : Endpoint<UpdatePodioFieldRequest>
{
    private readonly IMediator _mediator = mediator;
    private readonly IConfiguration _configuration = configuration;

    public override void Configure()
    {
        Put("Podio/{ItemId}/FilArkivField");
        Options(x => x.WithTags("Podio"));
    }

    public override async Task HandleAsync(UpdatePodioFieldRequest req, CancellationToken ct)
    {
        var appId = _configuration.GetValue<int>("Podio:AktindsigtApp:Id");

        // Update FilArkiv Case Id Field
        var filArkivCaseIdFieldId = _configuration.GetValue<int>("Podio:AktindsigtApp:Fields:FilArkivCaseId");
        var updateFilArkivCaseIdFieldCommand = new UpdateFieldCommand(appId, req.ItemId, filArkivCaseIdFieldId, req.Value);
        await _mediator.Send(updateFilArkivCaseIdFieldCommand, ct);
        
        // Update FilArkiv Case Link Field
        var filArkivCaseLinkFieldId = _configuration.GetValue<int>("Podio:AktindsigtApp:Fields:FilArkivLink");
        var updateFilArkivCaseLinkFieldCommand = new UpdateFieldCommand(appId, req.ItemId, filArkivCaseLinkFieldId, $"https://aarhus.filarkiv.dk/archives/case/{req.Value}");         
        await _mediator.Send(updateFilArkivCaseLinkFieldCommand, ct);

        await SendNoContentAsync(ct);
    }
}
