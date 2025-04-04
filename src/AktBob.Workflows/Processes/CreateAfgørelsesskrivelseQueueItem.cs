using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts;
using AktBob.OpenOrchestrator.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Shared.Jobs;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Workflows.Extensions;

namespace AktBob.Workflows.Processes;
internal class CreateAfgørelsesskrivelseQueueItem(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration) : IJobHandler<CreateAfgørelsesskrivelseQueueItemJob>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IConfiguration _configuration = configuration;

    public async Task Handle(CreateAfgørelsesskrivelseQueueItemJob job, CancellationToken cancellationToken = default)
    {
        Guard.Against.NegativeOrZero(job.DeskproTicketId);

        using var scope = _serviceScopeFactory.CreateScope();
        var deskpro = scope.ServiceProvider.GetRequiredServiceOrThrow<IDeskproModule>();
        var unitOfWork = scope.ServiceProvider.GetRequiredServiceOrThrow<IUnitOfWork>();
        var openOrchestrator = scope.ServiceProvider.GetRequiredServiceOrThrow<IOpenOrchestratorModule>();

        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("CreateAfgørelsesskrivelseQueueItemJobHandler:OpenOrchestratorQueueName"));
        var deskproAfdelingFieldId = Guard.Against.Null(_configuration.GetValue<int>("CreateAfgørelsesskrivelseQueueItemJobHandler:AfdelingFieldId"));
        var deskproModtagelsesdatoFieldId = Guard.Against.Null(_configuration.GetValue<int>("CreateAfgørelsesskrivelseQueueItemJobHandler:ModtagelsesdatoFieldId"));

        // Get data from Deskpro
        var deskproTicketResult = await deskpro.GetTicket(job.DeskproTicketId, cancellationToken);
        if (!deskproTicketResult.IsSuccess) throw new BusinessException("Unable to get ticket from Deskpro");

        var deskproTicket = deskproTicketResult.Value;

        // Field: afdeling
        var afdelingChoiceId = Convert.ToInt32(deskproTicket.Fields.FirstOrDefault(x => x.Id == deskproAfdelingFieldId)?.Values.FirstOrDefault());
        string afdeling = string.Empty;
        var afdelingChoices = deskproTicket.Fields.FirstOrDefault(x => x.Id == deskproAfdelingFieldId)?.Choices;
        if (afdelingChoices != null && afdelingChoices.TryGetValue(afdelingChoiceId, out string? value))
        {
            afdeling = value;
        }

        // Field: modtagelsesdato
        DateTime? modtagelsesdato = deskproTicket?.CreatedAt;
        var modtagelsesdatoFieldValue = deskproTicket?.Fields.FirstOrDefault(x => x.Id == deskproModtagelsesdatoFieldId)?.Values.FirstOrDefault();
        if (modtagelsesdatoFieldValue.TryParseDeskproDateTime(out DateTime? date))
        {
            modtagelsesdato = date;
        }

        var getPerson = deskproTicket?.Person != null
            ? deskpro.GetPerson(deskproTicket.Person.Id, cancellationToken)
            : Task.FromResult(Result<PersonDto>.Error());

        var getAgent = deskproTicket?.Agent != null
            ? deskpro.GetPerson(deskproTicket.Agent.Id, cancellationToken)
            : Task.FromResult(Result<PersonDto>.Error());

        var getDatabaseTicket = unitOfWork.Tickets.GetByDeskproTicketId(job.DeskproTicketId);

        await Task.WhenAll([
            getPerson,
            getAgent,
            getDatabaseTicket]);

        if (getDatabaseTicket.Result is null) throw new BusinessException("Unable to get ticket from database");

        // Create OpenOrchestrator queue item
        var payload = new
        {
            AnsøgerNavn = getPerson.Result.Value?.FullName,
            AnsøgerEmail = getPerson.Result.Value?.Email,
            Afdeling = afdeling,
            Aktindsigtsovermappe = getDatabaseTicket.Result?.SharepointFolderName,
            SagsbehandlerEmail = getAgent.Result.Value?.Email,
            DeskProID = job.DeskproTicketId,
            AktindsigtsDato = modtagelsesdato
        };

        var command = new CreateQueueItemCommand(openOrchestratorQueueName, $"DeskproID {job.DeskproTicketId}", payload.ToJson());
        openOrchestrator.CreateQueueItem(command);
    }

}
