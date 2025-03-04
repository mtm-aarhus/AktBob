using AAK.Podio.Models;
using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Podio.Contracts;
using AktBob.Shared.Jobs;

namespace AktBob.JobHandlers.Processes;
internal class RegisterPodioCase(ILogger<RegisterPodioCase> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory) : IJobHandler<RegisterPodioCaseJob>
{
    private readonly ILogger<RegisterPodioCase> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(RegisterPodioCaseJob job, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        // Services
        var podio = scope.ServiceProvider.GetRequiredService<IPodioModule>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Variables
        var podioAppId = Guard.Against.Null(_configuration.GetValue<int?>("Podio:AppId"));
        var podioFields = Guard.Against.Null(Guard.Against.NullOrEmpty(_configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => int.Parse(x.Key), x => x.Get<PodioFieldConfigurationSection>())));
        var podioFieldDeskproId = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value!.AppId == podioAppId && x.Value.Label == "DeskproId"));
        var podioFieldCaseNumber = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value!.AppId == podioAppId && x.Value.Label == "CaseNumber"));
        Guard.Against.Null(podioFieldDeskproId.Value);

        
        // Get metadata from Podio
        var podioItemResult = await podio.GetItem(podioAppId, job.PodioItemId, cancellationToken);
        if (!podioItemResult.IsSuccess)
        {
            _logger.LogError("Could not get item {itemId} from Podio", job.PodioItemId);
            return;
        }

        var caseNumber = podioItemResult.Value.GetField(podioFieldCaseNumber.Key)?.GetValues<FieldValueText>()?.Value ?? string.Empty;

        if (string.IsNullOrEmpty(caseNumber))
        {
            _logger.LogError("Could not get case number field value from Podio Item {id}", job.PodioItemId);
            return;
        }

        var deskproIdString = podioItemResult.Value.GetField(podioFieldDeskproId.Key)?.GetValues<FieldValueText>()?.Value ?? string.Empty;
        if (string.IsNullOrEmpty(deskproIdString))
        {
            _logger.LogError("Could not get Deskpro Id field value from Podio Item {itemId}", job.PodioItemId);
            return;
        }

        if (!int.TryParse(deskproIdString, out int deskproId))
        {
            _logger.LogError("Could not parse Deskpro Id field value as integer from Podio Item {itemId}", job.PodioItemId);
            return;
        }

        // Get ticket from repository
        var databaseTicket = await unitOfWork.Tickets.GetByDeskproTicketId(deskproId);

        if (databaseTicket is null)
        {
            _logger.LogWarning("No tickets found in database for DeskproId '{deskproId}'", deskproId);
            return;
        }

        // Add case to database
        var @case = new Case
        {
            TicketId = databaseTicket.Id,
            PodioItemId = job.PodioItemId,
            CaseNumber = caseNumber
        };

        await unitOfWork.Cases.Add(@case);
    }
}
