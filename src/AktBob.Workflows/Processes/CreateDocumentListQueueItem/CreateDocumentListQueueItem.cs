using AAK.Podio.Models;
using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.OpenOrchestrator.Contracts;
using AktBob.Podio.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Shared.Jobs;
using AktBob.UiPath.Contracts;
using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Workflows.Processes.CreateDocumentListQueueItem;

internal class CreateDocumentListQueueItem(
    ILogger<CreateDocumentListQueueItem> logger,
    IConfiguration configuration,
    IServiceScopeFactory serviceScopeFactory) : IJobHandler<CreateDocumentListQueueItemJob>
{
    private readonly ILogger<CreateDocumentListQueueItem> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly string _configurationObjectName = "CreateDocumentListQueueItemJobHandler";

    public async Task Handle(CreateDocumentListQueueItemJob job, CancellationToken cancellationToken = default)
    {
        // Validate job parameters
        Guard.Against.NegativeOrZero(job.PodioItemId.AppId);

        using var scope = _serviceScopeFactory.CreateScope();

        // Services
        var openOrchestrator = scope.ServiceProvider.GetRequiredService<IOpenOrchestratorModule>();
        var uiPath = scope.ServiceProvider.GetRequiredService<IUiPathModule>();
        var podio = scope.ServiceProvider.GetRequiredService<IPodioModule>();
        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<ITimeProvider>();
        var jobDispatcher = scope.ServiceProvider.GetRequiredService<IJobDispatcher>();

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

        // Get data
        var getPodioItem = podio.GetItem(job.PodioItemId, cancellationToken);
        var getDatabaseTicket = unitOfWork.Tickets.GetByPodioItemId(job.PodioItemId.Id);

        Task.WaitAll([getPodioItem, getDatabaseTicket]);

        if (getPodioItem.Result is null) throw new BusinessException($"Could not get valid data from Podio.");

        // RETRY: Trying to get the ticket data from the database might fail initially since Podio triggers both the create event
        // and DocumentListTrigger event almost at the same time. Because of this race condition, the database might not have
        // the ticket data at this point in time yet.
        //
        // We handle this by rescheduling the job. However, we do not want to reschedule forever, so after 4 retries we stop the rescheduling and exit with an error
        // The retry is scheduled with an exponential delay of 3 seconds raised to the power of the count. So 4 retries = 3s + 9s + 27s + 81s = 120s = 2 minutes.
        // If the database still haven't got the ticket data after 2 minutes something else is wrong and there is no need to keep rescheduling.
        if (getDatabaseTicket.Result is null)
        {
            var count = ReschedulingCounter.Instance.IncrementAndGet(job.PodioItemId);
            if (count <= 4)
            {
                _logger.LogInformation("Scheduling retry (count: {count}): ticket data for PodioItem {podioItem} not found in database", count, job.PodioItemId);
                jobDispatcher.Dispatch(new CreateDocumentListQueueItemJob(job.PodioItemId), TimeSpan.FromSeconds(Math.Pow(3, count)));
                return;
            }

            throw new BusinessException($"Reached maximum retries for getting ticket data from database.");
        }
        
        ReschedulingCounter.Instance.Remove(job.PodioItemId);

        var deskproTicket = await deskpro.GetTicket(getDatabaseTicket.Result.DeskproId, cancellationToken);

        var agent = deskproTicket.Value?.Agent?.Id != null
            ? await deskpro.GetPerson(deskproTicket.Value.Agent.Id, cancellationToken) 
            : Result<PersonDto>.Error();

        var caseNumber = getPodioItem.Result.Value?.GetField(podioFieldCaseNumber.Key)?.GetValues<FieldValueText>()?.Value;

        if (useOpenOrchestrator)
        {
            var payload = new
            {
                SagsNummer = caseNumber,
                agent.Value.Email,
                Navn = agent.Value.FullName,
                PodioID = job.PodioItemId.Id,
                DeskproID = deskproTicket.Value?.Id,
                Titel = deskproTicket.Value?.Subject
            };

            var command = new CreateQueueItemCommand(openOrchestratorQueueName, $"Podio {job.PodioItemId}", payload.ToJson());
            openOrchestrator.CreateQueueItem(command);
        }
        else
        {
            var payload = new
            {
                SagsNummer = caseNumber,
                agent.Value.FullName,
                Navn = agent.Value.FullName,
                PodioID = job.PodioItemId.Id,
                DeskproID = deskproTicket.Value?.Id,
                Titel = deskproTicket.Value?.Subject
            };

            uiPath.CreateQueueItem(uiPathQueueName, $"Podio {job.PodioItemId}", payload.ToJson());
        }
    }
}