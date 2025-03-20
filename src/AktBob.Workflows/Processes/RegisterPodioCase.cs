using AAK.Podio.Models;
using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Podio.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Shared.Jobs;

namespace AktBob.Workflows.Processes;
internal class RegisterPodioCase(ILogger<RegisterPodioCase> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory) : IJobHandler<RegisterPodioCaseJob>
{
    private readonly ILogger<RegisterPodioCase> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(RegisterPodioCaseJob job, CancellationToken cancellationToken = default)
    {
        Guard.Against.NegativeOrZero(job.PodioItemId.AppId);
        Guard.Against.NegativeOrZero(job.PodioItemId.Id);

        using var scope = _serviceScopeFactory.CreateScope();

        // Services
        var podio = scope.ServiceProvider.GetRequiredServiceOrThrow<IPodioModule>();
        var unitOfWork = scope.ServiceProvider.GetRequiredServiceOrThrow<IUnitOfWork>();

        // Variables
        var podioAppId = Guard.Against.Null(_configuration.GetValue<int?>("Podio:AppId"));
        var podioFields = Guard.Against.Null(Guard.Against.NullOrEmpty(_configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => int.Parse(x.Key), x => x.Get<PodioFieldConfigurationSection>())));
        var podioFieldDeskproId = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value!.AppId == podioAppId && x.Value.Label == "DeskproId"));
        var podioFieldCaseNumber = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value!.AppId == podioAppId && x.Value.Label == "CaseNumber"));
        Guard.Against.Null(podioFieldDeskproId.Value);

        
        // Get metadata from Podio
        var podioItemResult = await podio.GetItem(job.PodioItemId, cancellationToken);
        if (!podioItemResult.IsSuccess) throw new BusinessException("Unable to get item from Podio");

        var caseNumber = podioItemResult.Value.GetField(podioFieldCaseNumber.Key)?.GetValues<FieldValueText>()?.Value ?? string.Empty;
        if (string.IsNullOrEmpty(caseNumber)) throw new BusinessException("Case number field value from Podio Item is null or empty");

        var deskproIdString = podioItemResult.Value.GetField(podioFieldDeskproId.Key)?.GetValues<FieldValueText>()?.Value ?? string.Empty;
        if (string.IsNullOrEmpty(deskproIdString)) throw new BusinessException("Deskpro Id field value from Podio Item is null or empty");

        if (!int.TryParse(deskproIdString, out int deskproId)) throw new BusinessException("Unable to parse Podio item Deskpro Id field value as integer");

        // Get ticket from repository
        var databaseTicket = await unitOfWork.Tickets.GetByDeskproTicketId(deskproId);
        if (databaseTicket is null) throw new BusinessException($"Unable to get Deskpro ticket {deskproId} from database");

        // Add case to database
        var @case = new Case
        {
            TicketId = databaseTicket.Id,
            PodioItemId = job.PodioItemId.Id,
            CaseNumber = caseNumber
        };

        await unitOfWork.Cases.Add(@case);
    }
}
