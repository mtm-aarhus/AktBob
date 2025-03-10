using AAK.Podio.Models;
using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.OpenOrchestrator.Contracts;
using AktBob.Podio.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Shared.Jobs;
using AktBob.Workflows.Extensions;
using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Workflows.Processes;
internal class CreateToSharepointQueueItem(ILogger<CreateToSharepointQueueItem> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory) : IJobHandler<CreateToSharepointQueueItemJob>
{
    private readonly ILogger<CreateToSharepointQueueItem> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(CreateToSharepointQueueItemJob job, CancellationToken cancellationToken = default)
    {
        Guard.Against.NegativeOrZero(job.PodioItemId.AppId);
        Guard.Against.NegativeOrZero(job.PodioItemId.Id);

        using var scope = _serviceScopeFactory.CreateScope();

        // Services
        var openOrchestrator = scope.ServiceProvider.GetRequiredService<IOpenOrchestratorModule>();
        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        var podio = scope.ServiceProvider.GetRequiredService<IPodioModule>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Variables
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateToSharepointQueueItemJobHandler:OpenOrchestratorQueueName"));
        var podioAppId = Guard.Against.Null(_configuration.GetValue<int?>("Podio:AppId"));
        var podioFields = Guard.Against.Null(Guard.Against.NullOrEmpty(_configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => int.Parse(x.Key), x => x.Get<PodioFieldConfigurationSection>())));
        var podioFieldCaseNumber = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value!.AppId == podioAppId && x.Value.Label == "CaseNumber"));
        Guard.Against.Null(podioFieldCaseNumber.Value);


        // Get metadata from Podio
        var getPodioItem = podio.GetItem(job.PodioItemId, cancellationToken);
        var getDatabaseCase = unitOfWork.Cases.GetByPodioItemId(job.PodioItemId.Id);
        var getDatabaseTicket = unitOfWork.Tickets.GetByPodioItemId(job.PodioItemId.Id);

        Task.WaitAll([
            getPodioItem,
            getDatabaseCase,
            getDatabaseTicket]);

        if (!getPodioItem.Result.IsSuccess
            || getDatabaseCase.Result is null
            || getDatabaseTicket.Result is null)
        {
            _logger.LogCritical("Failed with {job}", job);
            return;
        }

        var caseNumber = getPodioItem.Result.Value.GetField(podioFieldCaseNumber.Key)?.GetValues<FieldValueText>()?.Value ?? string.Empty;
        if (string.IsNullOrEmpty(caseNumber))
        {
            _logger.LogWarning("Could not get case number field value from Podio Item {itemId}", job.PodioItemId);
        }

        if (string.IsNullOrEmpty(getDatabaseCase.Result.SharepointFolderName))
        {
            _logger.LogWarning("Case related to PodioItemId {id}: SharepointFolderName is null or empty", job.PodioItemId);
        }

        var filArkivCaseId = getDatabaseTicket.Result.Cases?.FirstOrDefault(c => c.PodioItemId == job.PodioItemId.Id)?.FilArkivCaseId;
        if (filArkivCaseId == null)
        {
            _logger.LogWarning("FilArkivCaseId not found for PodioItemId {podioItemId}", job.PodioItemId);
        }

        // Get ticket from Deskpro
        var deskproTicketResult = await deskpro.GetTicket(getDatabaseTicket.Result.DeskproId, cancellationToken);
        if (!deskproTicketResult.IsSuccess)
        {
            _logger.LogCritical("Failed with {job} failed", job);
            return;
        }

        var agent = deskproTicketResult.Value.Agent?.Id != null
            ? await deskpro.GetPerson(deskproTicketResult.Value.Agent.Id, cancellationToken)
            : Result<PersonDto>.Error();

        // Create queue item
        var payload = new
        {
            Sagsnummer = caseNumber,
            MailModtager = agent.Value.Email,
            DeskProID = deskproTicketResult.Value.Id,
            DeskProTitel = deskproTicketResult.Value.Subject,
            PodioID = job.PodioItemId.Id,
            Overmappe = getDatabaseTicket.Result.SharepointFolderName,
            Undermappe = getDatabaseCase.Result.SharepointFolderName,
            GeoSag = !caseNumber.IsNovaCase(),
            NovaSag = caseNumber.IsNovaCase(),
            AktSagsURL = getDatabaseTicket.Result.CaseUrl,
            FilarkivCaseID = getDatabaseCase.Result.FilArkivCaseId
        };

        var command = new CreateQueueItemCommand(openOrchestratorQueueName, $"Podio {job.PodioItemId}", payload.ToJson());
        openOrchestrator.CreateQueueItem(command);
    }
}