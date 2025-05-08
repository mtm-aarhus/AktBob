using AktBob.Deskpro.Contracts;
using AktBob.Shared.Jobs;
using System.Text.Json;

namespace AktBob.Workflows.Processes;
internal class UpdateDeskproSetFærdigbehandletDatoField : IJobHandler<UpdateDeskproSetFærdigbehandletDatoFieldJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;

    public UpdateDeskproSetFærdigbehandletDatoField(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
    }

    public Task Handle(UpdateDeskproSetFærdigbehandletDatoFieldJob job, CancellationToken cancellationToken = default)
    {
        var deskproWebhookId = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("Deskpro:Webhooks:UpdateDeskproSetFærdigbehandletDatoField"));
        var scope = _serviceScopeFactory.CreateScope();
        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();

        var payload = new
        {
            job.DeskproTicketId,
            DateValue = DateTime.UtcNow.ToString("yyyy-MM-dd")
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        deskpro.InvokeWebhook(deskproWebhookId, json);

        return Task.CompletedTask;
    }
}
