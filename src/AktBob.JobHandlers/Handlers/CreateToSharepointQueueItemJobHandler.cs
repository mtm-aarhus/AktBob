using AAK.Podio.Models;
using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.JobHandlers.Handlers;
using AktBob.JobHandlers.Utils;
using AktBob.Podio.Contracts;
using AktBob.Shared.Contracts;
using System.Text.RegularExpressions;

namespace AktBob.PodioHookProcessor.UseCases;
internal class CreateToSharepointQueueItemJobHandler(ILogger<CreateToSharepointQueueItemJobHandler> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory) : IJobHandler<CreateToSharepointQueueItemJob>
{
    private readonly ILogger<CreateToSharepointQueueItemJobHandler> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(CreateToSharepointQueueItemJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        // Services
        var deskproHelper = scope.ServiceProvider.GetRequiredService<DeskproHelper>();
        var getPodioItemHandler = scope.ServiceProvider.GetRequiredService<IGetPodioItemHandler>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var getDeskproTicketHandler = scope.ServiceProvider.GetRequiredService<IGetDeskproTicketHandler>();
        var getDeskproPersonHandler = scope.ServiceProvider.GetRequiredService<IGetDeskproPersonHandler>();

        // Variables
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateToSharepointQueueItemJobHandler:OpenOrchestratorQueueName"));
        var podioAppId = Guard.Against.Null(_configuration.GetValue<int?>("Podio:AppId"));
        var podioFields = Guard.Against.Null(Guard.Against.NullOrEmpty(_configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => int.Parse(x.Key), x => x.Get<PodioFieldConfigurationSection>())));
        var podioFieldCaseNumber = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value!.AppId == podioAppId && x.Value.Label == "CaseNumber"));
        Guard.Against.Null(podioFieldCaseNumber.Value);


        // Get metadata from Podio
        var podioItemResult = await getPodioItemHandler.Handle(podioAppId, job.PodioItemId, cancellationToken);

        if (!podioItemResult.IsSuccess)
        {
            _logger.LogError("Could not get item {itemId} from Podio", job.PodioItemId);
            return;
        }

        var caseNumber = podioItemResult.Value.GetField(podioFieldCaseNumber.Key)?.GetValues<FieldValueText>()?.Value ?? string.Empty;
        if (string.IsNullOrEmpty(caseNumber))
        {
            _logger.LogError("Could not get case number field value from Podio Item {itemId}", job.PodioItemId);
            return;
        }

        // Find database case from PodioItemID
        var databaseCase = await unitOfWork.Cases.GetByPodioItemId(job.PodioItemId);

        if (databaseCase is null)
        {
            _logger.LogError("No database case found by PodioItemId {podioItemId}", job.PodioItemId);
            return;
        }

        if (string.IsNullOrEmpty(databaseCase.SharepointFolderName))
        {
            _logger.LogError("Case related to PodioItemId {id}: SharepointFolderName is null or empty", job.PodioItemId);
            return;
        }

        // Find database ticket from PodioItemId
        var databaseTicket = await unitOfWork.Tickets.GetByPodioItemId(job.PodioItemId);

        if (databaseTicket is null)
        {
            _logger.LogError("No database ticket found for PodioItemId {podioItemId}.", job.PodioItemId);
            return;
        }

        var filArkivCaseId = databaseTicket.Cases?.FirstOrDefault(c => c.PodioItemId == job.PodioItemId)?.FilArkivCaseId;
        if (filArkivCaseId == null)
        {
            _logger.LogError("FilArkivCaseId not found for PodioItemId {podioItemId}", job.PodioItemId);
            return;
        }

        // Get ticket from Deskpro
        var deskproTicketResult = await getDeskproTicketHandler.Handle(databaseTicket.DeskproId, cancellationToken);
        if (!deskproTicketResult.IsSuccess)
        {
            _logger.LogError("Error getting ticket {id} from Deskpro", databaseTicket.DeskproId);
            return;
        }

        var agent = await deskproHelper.GetAgent(getDeskproPersonHandler, deskproTicketResult.Value.Agent?.Id ?? 0, cancellationToken);

        // Create queue item
        var payload = new
        {
            Sagsnummer = caseNumber,
            MailModtager = agent.Email,
            DeskProID = deskproTicketResult.Value.Id,
            DeskProTitel = deskproTicketResult.Value.Subject,
            PodioID = job.PodioItemId,
            Overmappe = databaseTicket.SharepointFolderName,
            Undermappe = databaseCase.SharepointFolderName,
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