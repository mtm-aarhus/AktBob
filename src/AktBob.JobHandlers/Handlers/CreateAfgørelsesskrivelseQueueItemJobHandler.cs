using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.Shared.Contracts;

namespace AktBob.JobHandlers.Handlers;
internal class CreateAfgørelsesskrivelseQueueItemJobHandler(IServiceScopeFactory serviceScopeFactory,
                                                            ILogger<CreateAfgørelsesskrivelseQueueItemJobHandler> logger,
                                                            IConfiguration configuration) : IJobHandler<CreateAfgørelsesskrivelseQueueItemJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<CreateAfgørelsesskrivelseQueueItemJobHandler> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    public async Task Handle(CreateAfgørelsesskrivelseQueueItemJob job, CancellationToken cancellationToken = default)
    {
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateAfgørelsesskrivelseQueueItemJobHandler:OpenOrchestratorQueueName"));
        var deskproAfdelingFieldId = Guard.Against.Null(_configuration.GetValue<int>("CreateAfgørelsesskrivelseQueueItemJobHandler:AfdelingFieldId"));

        using var scope = _serviceScopeFactory.CreateScope();
        var queryDispatcher = scope.ServiceProvider.GetRequiredService<IQueryDispatcher>();

        // Get data from Deskpro
        var getDeskproTicketQuery = new GetDeskproTicketByIdQuery(job.DeskproId);
        var getDeskproTicketResult = await queryDispatcher.Dispatch(getDeskproTicketQuery, cancellationToken);

        if (!getDeskproTicketResult.IsSuccess)
        {
            _logger.LogError("Error getting Deskpro ticket {id}", job.DeskproId);
            return;
        }


        var deskproTicket = getDeskproTicketResult.Value;
        var afdeling = deskproTicket.Fields.FirstOrDefault(x => x.Id == deskproAfdelingFieldId)?.Values.FirstOrDefault();
        if (string.IsNullOrEmpty(afdeling))
        {
            _logger.LogWarning("Deskpro ticket {id} field 'Afdeling' is null or empty", job.DeskproId);
        }


        // Get person from Deskpro
        if (deskproTicket.Person == null)
        {
            _logger.LogWarning("Deskpro ticket {id}: person is null", job.DeskproId);
        }

        var getPersonResult = await GetDeskproPerson(queryDispatcher, deskproTicket.Person?.Id, cancellationToken);
        var person = getPersonResult.Value;


        // Get agent from Deskpro
        if (deskproTicket.Agent == null)
        {
            _logger.LogWarning("Deskpro ticket {id}: agent is null", job.DeskproId);
        }

        var getAgentResult = await GetDeskproPerson(queryDispatcher, deskproTicket.Agent?.Id, cancellationToken);
        var agent = getAgentResult.Value;


        // Get data from database
        var getDatabaseTicketQuery = new GetTicketsQuery(job.DeskproId, null, null);
        var getDatabaseTicketResult = await queryDispatcher.Dispatch(getDatabaseTicketQuery, cancellationToken);

        if (!getDatabaseTicketResult.IsSuccess || getDatabaseTicketResult.Value == null || !getDatabaseTicketResult.Value.Any())
        {
            _logger.LogError("Error ticket from database (DeskproId {id})", job.DeskproId);
        }


        var databaseTicket = getDatabaseTicketResult?.Value?.FirstOrDefault();
        if (string.IsNullOrEmpty(databaseTicket?.SharepointFolderName))
        {
            _logger.LogError("Sharepoint folder name is null or empty (database ticket id {id}", job.DeskproId);
        }


        // Create OpenOrchestrator queue item
        var payload = new
        {
            AnsøgerNavn = person?.FullName,
            AnsøgerEmail = person?.Email,
            Afdeling = afdeling,
            Aktindsigtsovermappe = databaseTicket?.SharepointFolderName,
            SagsbehandlerEmail = agent?.Email
        };

        BackgroundJob.Enqueue<CreateOpenOrchestratorQueueItem>(x => x.Run(openOrchestratorQueueName, $"DeskproID {job.DeskproId}", payload.ToJson(), CancellationToken.None));
    }

    private async Task<Result<Deskpro.Contracts.DTOs.PersonDto>> GetDeskproPerson(IQueryDispatcher queryDispatcher, int? personId, CancellationToken cancellationToken)
    {
        if (personId is null)
        {
            return Result.Error();
        }

        var getPersonQuery = new GetDeskproPersonQuery((int)personId);
        var getPersonResult = await queryDispatcher.Dispatch(getPersonQuery, cancellationToken);
        if (!getPersonResult.IsSuccess)
        {
            _logger.LogWarning("Error getting person {id} from Deskpro", personId);
        }

        return getPersonResult;
    }
}
