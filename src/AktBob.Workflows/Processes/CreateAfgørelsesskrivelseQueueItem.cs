using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.Workflows.Helpers;
using AktBob.OpenOrchestrator.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Shared.Jobs;
using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Workflows.Processes;
internal class CreateAfgørelsesskrivelseQueueItem(IServiceScopeFactory serviceScopeFactory,
                                                  ILogger<CreateAfgørelsesskrivelseQueueItem> logger,
                                                  IConfiguration configuration) : IJobHandler<CreateAfgørelsesskrivelseQueueItemJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<CreateAfgørelsesskrivelseQueueItem> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    public async Task Handle(CreateAfgørelsesskrivelseQueueItemJob job, CancellationToken cancellationToken = default)
    {
        Guard.Against.NegativeOrZero(job.DeskproTicketId);

        using var scope = _serviceScopeFactory.CreateScope();
        var deskproHelper = scope.ServiceProvider.GetRequiredService<DeskproHelper>();
        var deskpro = scope.ServiceProvider.GetRequiredService<IDeskproModule>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var openOrchestrator = scope.ServiceProvider.GetRequiredService<IOpenOrchestratorModule>();

        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateAfgørelsesskrivelseQueueItemJobHandler:OpenOrchestratorQueueName"));
        var deskproAfdelingFieldId = Guard.Against.Null(_configuration.GetValue<int>("CreateAfgørelsesskrivelseQueueItemJobHandler:AfdelingFieldId"));

        // Get data from Deskpro
        var getDeskproTicketResult = await deskpro.GetTicket(job.DeskproTicketId, cancellationToken);

        if (!getDeskproTicketResult.IsSuccess)
        {
            _logger.LogError("Error getting Deskpro ticket {id}", job.DeskproTicketId);
            return;
        }

        var deskproTicket = getDeskproTicketResult.Value;

        var getPerson = GetPerson(deskproHelper, deskpro, deskproTicket.Person, cancellationToken);
        var getAgent = GetAgent(deskproHelper, deskpro, deskproTicket.Agent, cancellationToken);
        var getDatabaseTicket = unitOfWork.Tickets.GetByDeskproTicketId(job.DeskproTicketId);

        Task.WaitAll([
            getPerson,
            getAgent,
            getDatabaseTicket]);

        // Create OpenOrchestrator queue item
        var payload = new
        {
            AnsøgerNavn = getPerson.Result.Value?.FullName,
            AnsøgerEmail = getPerson.Result.Value?.Email,
            Afdeling = deskproTicket.Fields.FirstOrDefault(x => x.Id == deskproAfdelingFieldId)?.Values.FirstOrDefault(),
            Aktindsigtsovermappe = getDatabaseTicket?.Result?.SharepointFolderName,
            SagsbehandlerEmail = getAgent.Result.Email
        };

        var command = new CreateQueueItemCommand(openOrchestratorQueueName, $"DeskproID {job.DeskproTicketId}", payload.ToJson());
        openOrchestrator.CreateQueueItem(command);
    }

    private async Task<Result<PersonDto>> GetPerson(DeskproHelper deskproHelper, IDeskproModule deskpro, PersonDto? person, CancellationToken cancellationToken)
    {
        if (person != null)
        {
            return await deskproHelper.GetPerson(deskpro, person.Id, cancellationToken);
        }

        return Result.Error();
    }

    private async Task<(string? Name, string? Email)> GetAgent(DeskproHelper deskproHelper, IDeskproModule deskpro, PersonDto? agent, CancellationToken cancellationToken)
    {
        if (agent != null)
        {
            return await deskproHelper.GetAgent(deskpro, agent.Id, cancellationToken);
        }

        return (null, null);
    }
}
