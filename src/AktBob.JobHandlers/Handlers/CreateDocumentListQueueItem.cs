using AAK.Podio.Models;
using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Deskpro.Contracts;
using AktBob.JobHandlers.Utils;
using AktBob.OpenOrchestrator.Contracts;
using AktBob.Podio.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Shared.Jobs;
using AktBob.UiPath.Contracts;

namespace AktBob.JobHandlers.Handlers;
internal class CreateDocumentListQueueItem(ILogger<CreateDocumentListQueueItem> logger,
                                                     IConfiguration configuration,
                                                     IServiceScopeFactory serviceScopeFactory) : IJobHandler<CreateDocumentListQueueItemJob>
{
    private readonly ILogger<CreateDocumentListQueueItem> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly string _configurationObjectName = "CreateDocumentListQueueItemJobHandler";

    public async Task Handle(CreateDocumentListQueueItemJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        // Services
        var openOrchestrator = scope.ServiceProvider.GetRequiredService<IOpenOrchestratorModule>();
        var uiPath = scope.ServiceProvider.GetRequiredService<IUiPathModule>();
        var podio = scope.ServiceProvider.GetRequiredService<IPodioModule>();
        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        var deskproHelper = scope.ServiceProvider.GetRequiredService<DeskproHelper>();
        var ticketRepository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<ITimeProvider>();

        // UiPath variables
        var tenancyName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("UiPath:TenancyName"));
        var uiPathQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"{_configurationObjectName}:UiPathQueueName:{tenancyName}"));

        // OpenOrchestrator variables
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>($"{_configurationObjectName}:OpenOrchestratorQueueName"));
        var useOpenOrchestrator = _configuration.GetValue<bool>($"{_configurationObjectName}:UseOpenOrchestrator");

        // Podio variables
        var podioAppId = Guard.Against.Null(_configuration.GetValue<int?>("Podio:AppId"));
        var podioFields = Guard.Against.Null(Guard.Against.NullOrEmpty(_configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => int.Parse(x.Key), x => x.Get<PodioFieldConfigurationSection>())));
        var podioFieldCaseNumber = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value!.AppId == podioAppId && x.Value.Label == "CaseNumber"));

        Guard.Against.Null(podioFieldCaseNumber.Value);


        // Get metadata from Podio
        var caseNumberResult = await GetCaseNumberFromPodioItem(podio, podioAppId, job.PodioItemId, podioFieldCaseNumber.Key, cancellationToken);
        if (!caseNumberResult.IsSuccess)
        {
            return;
        }

        // Find ticket in database from PodioItemId
        var databaseTicketResult = await GetTicketFromApiDatabaseByPodioItemId(ticketRepository, timeProvider, job.PodioItemId, cancellationToken);
        if (!databaseTicketResult.IsSuccess)
        {
            return;
        }

        // Get ticket from Deskpro
        var deskproTicket = await deskpro.GetTicket(databaseTicketResult.Value.DeskproId, cancellationToken);
        if (deskproTicket == null)
        {
            _logger.LogError("Error getting ticket {id} from Deskpro", databaseTicketResult.Value.DeskproId);
            return;
        }

        var agent = await deskproHelper.GetAgent(deskpro, deskproTicket.Value.Agent?.Id ?? 0, cancellationToken);

        if (useOpenOrchestrator)
        {
            var payload = new
            {
                SagsNummer = caseNumberResult.Value,
                agent.Email,
                Navn = agent.Name,
                PodioID = job.PodioItemId,
                DeskproID = deskproTicket.Value.Id,
                Titel = deskproTicket.Value.Subject
            };

            openOrchestrator.CreateQueueItem(openOrchestratorQueueName, $"PodioItemID {job.PodioItemId}: {caseNumberResult}", payload.ToJson());
        }
        else
        {
            var payload = new
            {
                SagsNummer = caseNumberResult.Value,
                agent.Email,
                Navn = agent.Name,
                PodioID = job.PodioItemId,
                DeskproID = deskproTicket.Value.Id,
                Titel = deskproTicket.Value.Subject
            };

            uiPath.CreateQueueItem(uiPathQueueName, $"PodioItemID {job.PodioItemId}: {caseNumberResult}", payload.ToJson());
        }
    }

    private async Task<Result<string>> GetCaseNumberFromPodioItem(IPodioModule podio, int podioAppId, long podioItemId, int podioFieldId, CancellationToken cancellationToken)
    {
        var getPodioItemResult = await podio.GetItem(podioAppId, podioItemId, cancellationToken);

        if (!getPodioItemResult.IsSuccess)
        {
            _logger.LogError("Could not get item {itemId} from Podio", podioItemId);
            return Result.Error();
        }

        var caseNumber = getPodioItemResult.Value.GetField(podioFieldId)?.GetValues<FieldValueText>()?.Value ?? string.Empty;

        if (string.IsNullOrEmpty(caseNumber))
        {
            _logger.LogError("Case number field is null or empty on Podio item {itemId} fieldId {fieldId}", podioItemId, podioFieldId);
        }

        return caseNumber;
    }

    private async Task<Result<Ticket>> GetTicketFromApiDatabaseByPodioItemId(ITicketRepository ticketRepository, ITimeProvider timeProvider, long podioItemId, CancellationToken cancellationToken)
    {
        var retriesCount = 10;
        var counter = 1;
        var delay = TimeSpan.FromSeconds(5);

        // Try get some data from the database API based on the provided podioItemId
        // This might fail initially since Podio triggers both the create event and DocumentListTrigger event almost at the same time
        // Because of this, we utilize a simple retry feature. To be absolutely sure, we allow a retry of max of 10. If nothing is found
        // after 10 retries something else is wrong.
        while (counter <= retriesCount || !cancellationToken.IsCancellationRequested)
        {
            var ticket = await ticketRepository.GetByPodioItemId(podioItemId);

            // We have data: Exit the while loop
            if (ticket != null)
            {
                _logger.LogInformation("Try {count}/{retries}: Database API ticket data for PodioItemId {id} found", counter, retriesCount, podioItemId);
                return ticket;
            }

            // No data was found: retry
            _logger.LogWarning("Try {count}/{retries}: Database API did not return any ticket data for PodioItemId {id}. Retry in {time} ...", counter, retriesCount, podioItemId, delay.ToString());

            counter++;
            await timeProvider.Delay(delay, cancellationToken);
        }

        
        _logger.LogError("Final try: Database API did not return any ticket data for PodioItemId {id}", podioItemId);
        return Result.Error();
    }
}
