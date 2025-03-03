using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.JobHandlers.Utils;
using AktBob.OpenOrchestrator.Contracts;
using AktBob.Shared.Jobs;

namespace AktBob.JobHandlers.Handlers;
internal class CreateAfgørelsesskrivelseQueueItem(IServiceScopeFactory serviceScopeFactory,
                                                            ILogger<CreateAfgørelsesskrivelseQueueItem> logger,
                                                            IConfiguration configuration) : IJobHandler<CreateAfgørelsesskrivelseQueueItemJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<CreateAfgørelsesskrivelseQueueItem> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    public async Task Handle(CreateAfgørelsesskrivelseQueueItemJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var jobDispatcher = scope.ServiceProvider.GetRequiredService<IJobDispatcher>();
        var deskproHelper = scope.ServiceProvider.GetRequiredService<DeskproHelper>();
        var getDeskproPersonHandler = scope.ServiceProvider.GetRequiredService<IGetDeskproPersonHandler>();
        var getDeskproTicketHandler = scope.ServiceProvider.GetRequiredService<IGetDeskproTicketHandler>();
        var ticketRepository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();


        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateAfgørelsesskrivelseQueueItemJobHandler:OpenOrchestratorQueueName"));
        var deskproAfdelingFieldId = Guard.Against.Null(_configuration.GetValue<int>("CreateAfgørelsesskrivelseQueueItemJobHandler:AfdelingFieldId"));


        // Get data from Deskpro
        var getDeskproTicketResult = await getDeskproTicketHandler.Handle(job.DeskproTicketId, cancellationToken);

        if (!getDeskproTicketResult.IsSuccess)
        {
            _logger.LogError("Error getting Deskpro ticket {id}", job.DeskproTicketId);
            return;
        }


        var deskproTicket = getDeskproTicketResult.Value;
        var afdeling = deskproTicket.Fields.FirstOrDefault(x => x.Id == deskproAfdelingFieldId)?.Values.FirstOrDefault();
        if (string.IsNullOrEmpty(afdeling))
        {
            _logger.LogWarning("Deskpro ticket {id} field 'Afdeling' is null or empty", job.DeskproTicketId);
        }


        // Get person from Deskpro
        if (deskproTicket.Person == null)
        {
            _logger.LogWarning("Deskpro ticket {id}: person is null", job.DeskproTicketId);
        }

        var person = await deskproHelper.GetPerson(getDeskproPersonHandler, deskproTicket.Person?.Id ?? 0, cancellationToken);

        if (!person.IsSuccess || person.Value is null)
        {
            _logger.LogWarning("Error getting person ID {id} from Deskpro", deskproTicket.Person?.Id ?? 0);
        }

        // Get agent from Deskpro
        if (deskproTicket.Agent == null)
        {
            _logger.LogWarning("Deskpro ticket {id}: agent is null", job.DeskproTicketId);
        }

        var agent = await deskproHelper.GetAgent(getDeskproPersonHandler, deskproTicket.Agent?.Id ?? 0, cancellationToken);

        // Get data from database
        var databaseTicket = await ticketRepository.GetByDeskproTicketId(job.DeskproTicketId);

        if (databaseTicket is null)
        {
            _logger.LogError("Error ticket from database (DeskproId {id})", job.DeskproTicketId);
        }

        if (string.IsNullOrEmpty(databaseTicket?.SharepointFolderName))
        {
            _logger.LogWarning("Sharepoint folder name is null or empty (database ticket id {id}", job.DeskproTicketId);
        }


        // Create OpenOrchestrator queue item
        var payload = new
        {
            AnsøgerNavn = person.Value?.FullName,
            AnsøgerEmail = person.Value?.Email,
            Afdeling = afdeling,
            Aktindsigtsovermappe = databaseTicket?.SharepointFolderName,
            SagsbehandlerEmail = agent.Email
        };

        jobDispatcher.Dispatch(new CreateOpenOrchestratorQueueItemJob(openOrchestratorQueueName, $"DeskproID {job.DeskproTicketId}", payload.ToJson()));
    }
}
