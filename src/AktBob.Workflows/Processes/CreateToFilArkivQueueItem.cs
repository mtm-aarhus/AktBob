using AAK.Podio.Models;
using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.OpenOrchestrator.Contracts;
using AktBob.Podio.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Shared.Jobs;
using AktBob.Workflows.Extensions;

namespace AktBob.Workflows.Processes;
internal class CreateToFilArkivQueueItem(ILogger<CreateToFilArkivQueueItem> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory) : IJobHandler<CreateGoToFilArkivQueueItemJob>
{
    private readonly ILogger<CreateToFilArkivQueueItem> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(CreateGoToFilArkivQueueItemJob job, CancellationToken cancellationToken = default)
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
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateToFilArkivQueueItemJobHandler:OpenOrchestratorQueueName"));
        var podioAppId = Guard.Against.Null(_configuration.GetValue<int?>("Podio:AppId"));
        var podioFields = Guard.Against.Null(Guard.Against.NullOrEmpty(_configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => int.Parse(x.Key), x => x.Get<PodioFieldConfigurationSection>())));
        var podioFieldCaseNumber = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value!.AppId == podioAppId && x.Value.Label == "CaseNumber"));
        Guard.Against.Null(podioFieldCaseNumber.Value);

        // Begin
        var getPodioItem = podio.GetItem(job.PodioItemId, cancellationToken);
        var getDatabaseCase = unitOfWork.Cases.GetAll(job.PodioItemId.Id, null);
        var getDatabaseTicket = unitOfWork.Tickets.GetByPodioItemId(job.PodioItemId.Id);

        Task.WaitAll([
            getPodioItem,
            getDatabaseCase,
            getDatabaseTicket]);

        if (!getPodioItem.Result.IsSuccess) throw new BusinessException("Unable to get item from Podio");
        if (getDatabaseCase.Result.FirstOrDefault() is null) throw new BusinessException("Unable to get case from database");
        if (getDatabaseTicket.Result is null) throw new BusinessException("Unable to get ticket from database");

        if (string.IsNullOrEmpty(getDatabaseTicket.Result.SharepointFolderName)) throw new BusinessException($"SharepointFolderName is null or empty for case (PodioItem: {job.PodioItemId})");

        var caseNumber = getPodioItem.Result.Value.GetField(podioFieldCaseNumber.Key)?.GetValues<FieldValueText>()?.Value ?? string.Empty;
        if (string.IsNullOrEmpty(caseNumber))
        {
            _logger.LogWarning("Unable to get case number field value from Podio Item {itemId}", job.PodioItemId);
        }

        // Get data from Deskpro
        var deskproTicketResult = await deskpro.GetTicket(getDatabaseTicket.Result.DeskproId, cancellationToken);
        if (!deskproTicketResult.IsSuccess) throw new BusinessException("Unable to get ticket from Deskpro");

        // Get Deskpro agent
        var agent = deskproTicketResult.Value.Agent?.Id != null
            ? await deskpro.GetPerson(deskproTicketResult.Value.Agent.Id, cancellationToken)
            : Result<PersonDto>.Error();

        // Create queue item
        var payload = new
        {
            Sagsnummer = caseNumber,
            MailModtager = agent.Value.Email,
            DeskProID = getDatabaseTicket.Result.DeskproId,
            DeskProTitel = deskproTicketResult.Value.Subject,
            PodioID = job.PodioItemId.Id,
            Overmappe = getDatabaseTicket.Result.SharepointFolderName,
            Undermappe = getDatabaseCase.Result.First().SharepointFolderName,
            GeoSag = !caseNumber.IsNovaCase(),
            NovaSag = caseNumber.IsNovaCase(),
            AktSagsURL = getDatabaseTicket.Result.CaseUrl
        };

        var command = new CreateQueueItemCommand(openOrchestratorQueueName, $"Podio {job.PodioItemId}", payload.ToJson());
        openOrchestrator.CreateQueueItem(command);
    }
}