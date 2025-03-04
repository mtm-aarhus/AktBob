using AAK.Podio.Models;
using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.JobHandlers.Utils;
using AktBob.OpenOrchestrator.Contracts;
using AktBob.Podio.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Shared.Jobs;
using System.Text.RegularExpressions;

namespace AktBob.PodioHookProcessor.UseCases;
internal class CreateToFilArkivQueueItem(ILogger<CreateToFilArkivQueueItem> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory) : IJobHandler<CreateGoToFilArkivQueueItemJob>
{
    private readonly ILogger<CreateToFilArkivQueueItem> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(CreateGoToFilArkivQueueItemJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        // Services
        var jobDispatcher = scope.ServiceProvider.GetRequiredService<IJobDispatcher>();
        var openOrchestrator = scope.ServiceProvider.GetRequiredService<IOpenOrchestratorModule>();
        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        var deskproHelper = scope.ServiceProvider.GetRequiredService<DeskproHelper>();
        var getPodioItemHandler = scope.ServiceProvider.GetRequiredService<IGetPodioItemHandler>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Variables
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateToFilArkivQueueItemJobHandler:OpenOrchestratorQueueName"));
        var podioAppId = Guard.Against.Null(_configuration.GetValue<int?>("Podio:AppId"));
        var podioFields = Guard.Against.Null(Guard.Against.NullOrEmpty(_configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => int.Parse(x.Key), x => x.Get<PodioFieldConfigurationSection>())));
        var podioFieldCaseNumber = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value!.AppId == podioAppId && x.Value.Label == "CaseNumber"));
        Guard.Against.Null(podioFieldCaseNumber.Value);


        // Get metadata from Podio
        var getPodioItemResult = await getPodioItemHandler.Handle(podioAppId, job.PodioItemId, cancellationToken);

        if (!getPodioItemResult.IsSuccess)
        {
            _logger.LogError("Could not get item {itemId} from Podio", job.PodioItemId);
            return;
        }

        var caseNumber = getPodioItemResult.Value.GetField(podioFieldCaseNumber.Key)?.GetValues<FieldValueText>()?.Value ?? string.Empty;
        if (string.IsNullOrEmpty(caseNumber))
        {
            _logger.LogError("Could not get case number field value from Podio Item {itemId}", job.PodioItemId);
            return;
        }

        // Find database ticket by PodioItemId
        var databaseTicket = await unitOfWork.Tickets.GetByPodioItemId(job.PodioItemId);
        if (databaseTicket is null)
        {
            _logger.LogError("Ticket related to PodioItemId {id} not found in database", job.PodioItemId);
            return;
        }

        // Assert that the SharepointFolderName is not empty
        if (string.IsNullOrEmpty(databaseTicket.SharepointFolderName))
        {
            _logger.LogError("Database ticket related to Deskpro ticket {id}: SharepointFolderName is null or empty", databaseTicket.DeskproId);
            return;
        }

        // Find database cases
        var databaseCase = await unitOfWork.Cases.GetByPodioItemId(job.PodioItemId);
        if (databaseCase is null)
        {
            _logger.LogError("Case related to PodioItemId {id} not found in database", job.PodioItemId);
            return;
        }

        // Assert that the SharepointFolderName is not empty
        if (string.IsNullOrEmpty(databaseCase.SharepointFolderName))
        {
            _logger.LogError("Case related to PodioItemId {id}: SharepointFolderName is null or empty", job.PodioItemId);
        }

        // Get data from Deskpro
        var deskproTicketResult = await deskpro.GetTicket(databaseTicket.DeskproId, cancellationToken);

        if (!deskproTicketResult.IsSuccess)
        {
            _logger.LogError($"Could not get data from Deskpro for ticket ID {databaseTicket.DeskproId}");
            return;
        }

        // Get Deskpro agent
        var agent = await deskproHelper.GetAgent(deskpro, deskproTicketResult.Value.Agent?.Id ?? 0, cancellationToken);

        // Create queue item
        var payload = new
        {
            Sagsnummer = caseNumber,
            MailModtager = agent.Email,
            DeskProID = databaseTicket.DeskproId,
            DeskProTitel = deskproTicketResult.Value.Subject,
            PodioID = job.PodioItemId,
            Overmappe = databaseTicket.SharepointFolderName,
            Undermappe = databaseCase.SharepointFolderName,
            GeoSag = !IsNovaCase(caseNumber),
            NovaSag = IsNovaCase(caseNumber),
            AktSagsURL = databaseTicket.CaseUrl
        };

        openOrchestrator.CreateQueueItem(openOrchestratorQueueName, $"PodioItemID {job.PodioItemId}", payload.ToJson());
    }

    private bool IsNovaCase(string caseNumber)
    {
        string pattern = @"^[A-Za-z]\d{4}-\d{1,10}$";
        Regex regex = new Regex(pattern);
        return regex.IsMatch(caseNumber);
    }
}