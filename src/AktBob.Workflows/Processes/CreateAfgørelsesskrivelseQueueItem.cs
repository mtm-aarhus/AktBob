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
        var deskproLovgivningFieldId = Guard.Against.Null(_configuration.GetValue<int>("CreateAfgørelsesskrivelseQueueItemJobHandler:LovgivningFieldId"));

        // Get data from Deskpro
        var deskproTicketResult = await deskpro.GetTicket(job.DeskproTicketId, cancellationToken);
        if (!deskproTicketResult.IsSuccess || deskproTicketResult.Value is null) throw new BusinessException("Unable to get ticket from Deskpro");

        // Deskpro ticket fields
        string lovgivning = GetChoiceFieldValue(deskproTicketResult.Value, deskproLovgivningFieldId);
        DateTime? modtagelsesdato = GetDateTimeFieldValue(deskproTicketResult.Value, deskproModtagelsesdatoFieldId);

        
        // Person
        var getPerson = deskproTicketResult.Value.Person != null
            ? deskpro.GetPerson(deskproTicketResult.Value.Person.Id, cancellationToken)
            : Task.FromResult(Result<PersonDto>.Error());

        // Agent
        var getAgent = deskproTicketResult.Value.Agent != null
            ? deskpro.GetPerson(deskproTicketResult.Value.Agent.Id, cancellationToken)
            : Task.FromResult(Result<PersonDto>.Error());

        // Team
        var getTeam = deskproTicketResult.Value.AgentTeamId != null
            ? deskpro.GetTeam((int)deskproTicketResult.Value.AgentTeamId, cancellationToken)
            : Task.FromResult(Result<TeamDto>.Error());

        // Database ticket
        var getDatabaseTicket = unitOfWork.Tickets.GetByDeskproTicketId(job.DeskproTicketId);

        await Task.WhenAll([
            getPerson,
            getAgent,
            getDatabaseTicket,
            getTeam]);

        if (getDatabaseTicket.Result is null) throw new BusinessException("Unable to get ticket from database");


        // Create OpenOrchestrator queue item
        var payload = new
        {
            AnsøgerNavn = getPerson.Result.Value?.FullName,
            AnsøgerEmail = getPerson.Result.Value?.Email,
            Afdeling = getTeam.Result.Value?.Name,
            Aktindsigtsovermappe = getDatabaseTicket.Result?.SharepointFolderName,
            SagsbehandlerEmail = getAgent.Result.Value?.Email,
            DeskProID = job.DeskproTicketId,
            AktindsigtsDato = modtagelsesdato,
            Lovgivning = lovgivning
        };

        var command = new CreateQueueItemCommand(openOrchestratorQueueName, $"DeskproID {job.DeskproTicketId}", payload.ToJson());
        openOrchestrator.CreateQueueItem(command);
    }

    private static DateTime? GetDateTimeFieldValue(TicketDto deskproTicket, int fieldId)
    {
        var fieldValue = deskproTicket.Fields.FirstOrDefault(x => x.Id == fieldId)?.Values.FirstOrDefault();
        if (fieldValue.TryParseDeskproDateTime(out DateTime? datetime))
        {
            return datetime;
        }

        return null;
    }

    private static string GetChoiceFieldValue(TicketDto deskproTicket, int fieldId)
    {
        var choiceId = Convert.ToInt32(deskproTicket.Fields.FirstOrDefault(x => x.Id == fieldId)?.Values.FirstOrDefault());
        var choices = deskproTicket.Fields.FirstOrDefault(x => x.Id == fieldId)?.Choices;
        if (choices != null && choices.TryGetValue(choiceId, out string? value))
        {
            return value ?? string.Empty;
        }

        return string.Empty;
    }
}
