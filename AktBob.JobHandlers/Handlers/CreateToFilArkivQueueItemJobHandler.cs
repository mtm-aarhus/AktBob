using AAK.Podio.Models;
using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.JobHandlers.Handlers;
using AktBob.JobHandlers.Utils;
using AktBob.Podio.Contracts;
using AktBob.Shared;
using AktBob.Shared.Contracts;
using Ardalis.GuardClauses;
using Hangfire;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace AktBob.PodioHookProcessor.UseCases;
internal class CreateToFilArkivQueueItemJobHandler(ILogger<CreateToFilArkivQueueItemJobHandler> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, DeskproHelper deskproHelper) : IJobHandler<CreateGoToFilArkivQueueItemJob>
{
    private readonly ILogger<CreateToFilArkivQueueItemJobHandler> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly DeskproHelper _deskproHelper = deskproHelper;

    public async Task Handle(CreateGoToFilArkivQueueItemJob job, CancellationToken cancellationToken = default)
    {
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateToFilArkivQueueItemJobHandler:OpenOrchestratorQueueName"));
        var podioAppId = Guard.Against.Null(_configuration.GetValue<int?>("Podio:AppId"));
        var podioFields = Guard.Against.Null(Guard.Against.NullOrEmpty(_configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => int.Parse(x.Key), x => x.Get<PodioFieldConfigurationSection>())));
        var podioFieldCaseNumber = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value.AppId == podioAppId && x.Value.Label == "CaseNumber"));
        Guard.Against.Null(podioFieldCaseNumber.Value);

        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Get metadata from Podio
        var getPodioItemQuery = new GetItemQuery(podioAppId, job.PodioItemId);
        var getPodioItemQueryResult = await mediator.SendRequest(getPodioItemQuery, cancellationToken);

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

        // Find database ticket by PodioItemId
        var getDatabaseTicketByPodioItemIdQuery = new GetTicketsQuery(null, job.PodioItemId, null);
        var getDatabaseTicketByPodioItemIdQueryResult = await mediator.SendRequest(getDatabaseTicketByPodioItemIdQuery, cancellationToken);

        if (!getDatabaseTicketByPodioItemIdQueryResult.IsSuccess)
        {
            _logger.LogError($"Could not get data from database for ticket by PodioItemId {job.PodioItemId}");
            return;
        }

        if (getDatabaseTicketByPodioItemIdQueryResult.Value.Count() < 1)
        {
            _logger.LogError("No Deskpro tickets found for PodioItemId {podioItemId}.", job.PodioItemId);
            return;
        }

        if (getDatabaseTicketByPodioItemIdQueryResult.Value.Count() > 1)
        {
            _logger.LogWarning("{count} Deskpro tickets found for PodioItemId {podioItemId}. Only processing the first.", getDatabaseTicketByPodioItemIdQueryResult.Value.Count(), job.PodioItemId);
        }

        var databaseTicket = getDatabaseTicketByPodioItemIdQueryResult.Value.FirstOrDefault();

        if (databaseTicket is null)
        {
            _logger.LogError("Ticket related to PodioItemId {id} not found in database", job.PodioItemId);
            return;
        }

        // Get the case sharepoint folder name ("undermappenavn")
        var caseSharepointFolderName = databaseTicket.Cases?.FirstOrDefault(x => x.PodioItemId == job.PodioItemId)?.SharepointFolderName;
        if (caseSharepointFolderName == null)
        {
            _logger.LogError("Case related to PodioItemId {id}: SharepointFolderName is null or empty", job.PodioItemId);
        }

        // Get data from Deskpro
        var getDeskproTicketQuery = new GetDeskproTicketByIdQuery(databaseTicket.DeskproId);
        var getDeskproTicketQueryResult = await mediator.SendRequest(getDeskproTicketQuery, cancellationToken);

        if (!getDeskproTicketQueryResult.IsSuccess)
        {
            _logger.LogError($"Could not get data from Deskpro for ticket ID {databaseTicket.DeskproId}");
            return;
        }

        (string Name, string Email) agent = new(string.Empty, string.Empty);

        // Get Deskpro agent
        if (getDeskproTicketQueryResult.Value.Agent is not null && getDeskproTicketQueryResult.Value.Agent.Id > 0)
        {
            agent = await _deskproHelper.GetAgent(mediator, getDeskproTicketQueryResult.Value.Agent.Id, cancellationToken);
        }
        else
        {
            _logger.LogWarning($"Deskpro ticket {databaseTicket.DeskproId} has no agents assigned");
        }

        // Create queue item
        var payload = new
        {
            Sagsnummer = caseNumber,
            MailModtager = agent.Email,
            DeskProID = databaseTicket.DeskproId,
            DeskProTitel = getDeskproTicketQueryResult.Value.Subject,
            PodioID = job.PodioItemId,
            Overmappe = databaseTicket.SharepointFolderName,
            Undermappe = caseSharepointFolderName,
            GeoSag = !IsNovaCase(caseNumber),
            NovaSag = IsNovaCase(caseNumber),
            AktSagsURL = databaseTicket.CaseUrl
        };

        BackgroundJob.Enqueue<CreateOpenOrchestratorQueueItem>(x => x.Run(openOrchestratorQueueName, $"PodioItemID {job.PodioItemId}", payload.ToJson(), CancellationToken.None));
    }

    private bool IsNovaCase(string caseNumber)
    {
        string pattern = @"^S\d{4}-\d{1,5}$";
        Regex regex = new Regex(pattern);
        return regex.IsMatch(caseNumber);
    }
}