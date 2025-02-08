using AktBob.Deskpro.Contracts;
using Ardalis.GuardClauses;
using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.JobHandlers.Handlers.CreateGetOrganizedCase;
internal class UpdateDeskproField(ILogger<UpdateDeskproField> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
{
    private readonly ILogger<UpdateDeskproField> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task SetGetOrganizedCaseId(int deskproId, string caseId, string caseUrl, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var deskproWebhookId = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("Deskpro:Webhooks:UpdateTicketSetGoCaseId"));
        var caseUrlClean = caseUrl.Replace("ad.", "");

        _logger.LogInformation($"Updating Deskpro ticket. Invoking webhook ID {deskproWebhookId}. Payload: getOrganizedCaseId = {caseId}, getOrganizedCaseUrl = {caseUrlClean}, deskproTicketId = {deskproId}");

        var payload = new
        {
            GetOrganizedCaseId = caseId,
            GetOrganizedCaseUrlClean = caseUrlClean,
            DeskproTicketId = deskproId
        };

        var invokeWebhookCommand = new InvokeWebhookCommand(deskproWebhookId, payload);
        await mediator.Send(invokeWebhookCommand, cancellationToken);

        _logger.LogInformation($"webhook ID {deskproWebhookId} invoked.Payload: getOrganizedCaseId = {caseId}, getOrganizedCaseUrl = {caseUrlClean}, deskproTicketId = {deskproId}");
    }
}