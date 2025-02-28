using AktBob.Deskpro.Contracts;

namespace AktBob.JobHandlers.Handlers.CreateGetOrganizedCase;
internal class UpdateDeskproField(ILogger<UpdateDeskproField> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
{
    private readonly ILogger<UpdateDeskproField> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task SetGetOrganizedCaseId(int deskproId, string caseId, string caseUrl, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var commandDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        var deskproWebhookId = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("Deskpro:Webhooks:UpdateTicketSetGoCaseId"));

        _logger.LogInformation("Updating Deskpro ticket. Invoking webhook ID {deskproWebhookId}. Payload: getOrganizedCaseId = {caseId}, getOrganizedCaseUrl = {caseUrl}, deskproTicketId = {deskproId}", deskproWebhookId, caseId, caseUrl, deskproId);

        var payload = new
        {
            GetOrganizedCaseId = caseId,
            GetOrganizedCaseUrlClean = caseUrl,
            DeskproTicketId = deskproId
        };

        var invokeWebhookCommand = new InvokeWebhookCommand(deskproWebhookId, payload);
        await commandDispatcher.Dispatch(invokeWebhookCommand, cancellationToken);

        _logger.LogInformation("webhook ID {deskproWebhookId} invoked.Payload: getOrganizedCaseId = {caseId}, getOrganizedCaseUrl = {caseUrl}, deskproTicketId = {deskproId}", deskproWebhookId, caseId, caseUrl, deskproId);
    }
}