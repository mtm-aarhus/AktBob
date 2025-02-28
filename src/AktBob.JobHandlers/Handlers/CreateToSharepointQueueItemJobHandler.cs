using AAK.Podio.Models;
using AktBob.Database.Contracts;
using AktBob.Database.UseCases.Cases.GetCases;
using AktBob.Deskpro.Contracts;
using AktBob.JobHandlers.Handlers;
using AktBob.JobHandlers.Utils;
using AktBob.Podio.Contracts;
using AktBob.Shared.Contracts;
using System.Text.RegularExpressions;

namespace AktBob.PodioHookProcessor.UseCases;
internal class CreateToSharepointQueueItemJobHandler(ILogger<CreateToSharepointQueueItemJobHandler> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, DeskproHelper deskproHelper) : IJobHandler<CreateToSharepointQueueItemJob>
{
    private readonly ILogger<CreateToSharepointQueueItemJobHandler> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly DeskproHelper _deskproHelper = deskproHelper;

    public async Task Handle(CreateToSharepointQueueItemJob job, CancellationToken cancellationToken = default)
    {
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateToSharepointQueueItemJobHandler:OpenOrchestratorQueueName"));
        var podioAppId = Guard.Against.Null(_configuration.GetValue<int?>("Podio:AppId"));
        var podioFields = Guard.Against.Null(Guard.Against.NullOrEmpty(_configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => int.Parse(x.Key), x => x.Get<PodioFieldConfigurationSection>())));
        var podioFieldCaseNumber = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value!.AppId == podioAppId && x.Value.Label == "CaseNumber"));
        Guard.Against.Null(podioFieldCaseNumber.Value);

        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Get metadata from Podio
        var getPodioItemQuery = new GetItemQuery(podioAppId, job.PodioItemId);
        var getPodioItemQueryResult = await mediator.Send(getPodioItemQuery, cancellationToken);

        if (!getPodioItemQueryResult.IsSuccess)
        {
            _logger.LogError("Could not get item {itemId} from Podio", job.PodioItemId);
            return;
        }

        var caseNumber = getPodioItemQueryResult.Value.GetField(podioFieldCaseNumber.Key)?.GetValues<FieldValueText>()?.Value ?? string.Empty;
        if (string.IsNullOrEmpty(caseNumber))
        {
            _logger.LogError("Could not get case number field value from Podio Item {itemId}", job.PodioItemId);
            return;
        }

        // Find database case from PodioItemID
        var getDatabaseCasesQuery = new GetCasesQuery(null, job.PodioItemId, null);
        var getDataCaseCasesResult = await mediator.Send(getDatabaseCasesQuery, cancellationToken);

        if (!getDataCaseCasesResult.IsSuccess)
        {
            _logger.LogError("Error getting cases from databse by PodioItemId {podioItemId}", job.PodioItemId);
            return;
        }

        if (getDataCaseCasesResult.Value is null || getDataCaseCasesResult.Value.Count() == 0 || getDataCaseCasesResult.Value.FirstOrDefault() == null)
        {
            _logger.LogError("No database cases found by PodioItemId {podioItemId}", job.PodioItemId);
            return;
        }

        var databaseCase = getDataCaseCasesResult.Value.First();

        // Find database ticket from PodioItemId
        var getDatabaseTicketQuery = new GetTicketsQuery(null, job.PodioItemId, null);
        var getDatabaseTicketResult = await mediator.Send(getDatabaseTicketQuery, cancellationToken);

        if (!getDatabaseTicketResult.IsSuccess)
        {
            _logger.LogError("Error getting ticket from database by PodioItemId {podioItemId}.", job.PodioItemId);
            return;
        }

        var databaseTickets = getDatabaseTicketResult.Value;

        if (databaseTickets.Count() < 1)
        {
            _logger.LogError("No tickets found in database for PodioItemId {podioItemId}.", job.PodioItemId);
            return;
        }

        if (databaseTickets.Count() > 1)
        {
            _logger.LogWarning("{count} Deskpro tickets found in database for PodioItemId {podioItemId}. Only processing the first.", databaseTickets.Count(), job.PodioItemId);
        }

        var databaseTicket = databaseTickets.First();

        var caseSharepointFolderName = databaseTicket.Cases?.FirstOrDefault(x => x.PodioItemId == job.PodioItemId)?.SharepointFolderName;
        if (caseSharepointFolderName == null)
        {
            _logger.LogError("Case related to PodioItemId {id}: SharepointFolderName is null or empty", job.PodioItemId);
            return;
        }

        var filArkivCaseId = databaseTicket.Cases?.FirstOrDefault(c => c.PodioItemId == job.PodioItemId)?.FilArkivCaseId;
        if (filArkivCaseId == null)
        {
            _logger.LogError("FilArkivCaseId not found for PodioItemId {podioItemId}", job.PodioItemId);
            return;
        }

        var getDeskproTicketQuery = new GetDeskproTicketByIdQuery(databaseTicket.DeskproId);
        var getDeskproTicketResult = await mediator.Send(getDeskproTicketQuery, cancellationToken);

        if (!getDeskproTicketResult.IsSuccess)
        {
            _logger.LogError("Error getting ticket {id} from Deskpro", databaseTicket.DeskproId);
            return;
        }

        var deskproTicket = getDeskproTicketResult.Value;

        // Get Deskpro agent
        (string Name, string Email) agent = new(string.Empty, string.Empty);

        if (deskproTicket.Agent is not null && deskproTicket.Agent.Id > 0)
        {
            agent = await _deskproHelper.GetAgent(mediator, deskproTicket.Agent.Id, cancellationToken);
        }
        else
        {
            _logger.LogWarning($"Deskpro ticket {deskproTicket.Id} has no agents assigned");
        }


        // Create queue item
        var payload = new
        {
            Sagsnummer = caseNumber,
            MailModtager = agent.Email,
            DeskProID = deskproTicket.Id,
            DeskProTitel = deskproTicket.Id,
            PodioID = job.PodioItemId,
            Overmappe = databaseTicket.SharepointFolderName,
            Undermappe = caseSharepointFolderName,
            GeoSag = !IsNovaCase(caseNumber),
            NovaSag = IsNovaCase(caseNumber),
            AktSagsURL = databaseTicket.CaseUrl,
            FilarkivCaseID = databaseCase.FilArkivCaseId
        };

        BackgroundJob.Enqueue<CreateOpenOrchestratorQueueItem>(x => x.Run(openOrchestratorQueueName, $"PodioItemID {job.PodioItemId}", payload.ToJson(), CancellationToken.None));
    }

    private bool IsNovaCase(string caseNumber)
    {
        string pattern = @"^[A-Za-z]\d{4}-\d{1,10}$";
        Regex regex = new Regex(pattern);
        return regex.IsMatch(caseNumber);
    }
}