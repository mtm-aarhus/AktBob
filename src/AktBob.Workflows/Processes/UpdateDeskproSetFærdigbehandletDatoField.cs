using System.Text.Json;
using Aktbob.Modules.Deskpro.Features.InvokeWebhook;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Extensions;

namespace AktBob.Workflows.Processes;
internal class UpdateDeskproSetFærdigbehandletDatoField(
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration)
    : IJobHandler<UpdateDeskproSetFærdigbehandletDatoFieldJob>
{
    public Task Handle(UpdateDeskproSetFærdigbehandletDatoFieldJob job, CancellationToken cancellationToken = default)
    {
        var deskproWebhookId = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Deskpro:Webhooks:UpdateDeskproSetFærdigbehandletDatoField"));
        using var scope = serviceScopeFactory.CreateScope();
        var deskproInvokeWebhookHandler = scope.ServiceProvider.GetRequiredServiceOrThrow<IInvokeWebhookHandler>();

        var payload = new
        {
            job.TicketId,
            DateValue = DateTime.UtcNow.ToString("yyyy-MM-dd")
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        deskproInvokeWebhookHandler.Handle(deskproWebhookId, json, cancellationToken);

        return Task.CompletedTask;
    }
}
