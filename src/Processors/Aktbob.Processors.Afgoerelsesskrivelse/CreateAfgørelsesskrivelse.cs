using AktBob.Database.Contracts;
using AktBob.Shared;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using AktBob.Shared.Exceptions;
using AktBob.Shared.Extensions;
using AktBob.Shared.ModuleClients.DeskproModule;
using AktBob.Shared.ModuleClients.OpenOrchestratorModule;
using Ardalis.GuardClauses;
using Azure.Messaging.ServiceBus;
using ErrorOr;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.Afgørelsesskrivelse;

public class CreateAfgørelsesskrivelse(
    ILogger<CreateAfgørelsesskrivelse> logger,
    IConfiguration configuration,
    IDeskproModuleClient deskpro,
    IOpenOrchestratorModuleClient openOrchestrator,
    ITicketRepository ticketRepository)
{
    
    [Function("CreateAfgoerelsesskrivelse")]
    public async Task Run(
        [ServiceBusTrigger("%QueueName%", Connection = "AzureServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Message ID: {id} Body: {body} Content-Type: {contentType}", message.MessageId,  message.Body, message.ContentType);
        
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("OpenOrchestratorQueueName"));
        var deskproModtagelsesdatoFieldId = Guard.Against.Null(configuration.GetValue<int>("ModtagelsesdatoFieldId"));
        var deskproLovgivningFieldId = Guard.Against.Null(configuration.GetValue<int>("LovgivningFieldId"));
        
        // Deserialize message body to expected job type
        var job = MessageDeserializer.Deserialize<CreateAfgørelsesskrivelseJob>(message);
        
        // Get data from Deskpro
        var deskproTicket = await deskpro.GetTicket(job.TicketId, cancellationToken);
        if (deskproTicket.IsError) throw new BusinessException(deskproTicket.Errors.ToCommaDelimitedString());

        // Deskpro ticket fields
        string lovgivning = GetChoiceFieldValue(deskproTicket.Value, deskproLovgivningFieldId);
        DateTime? modtagelsesdato = GetDateTimeFieldValue(deskproTicket.Value, deskproModtagelsesdatoFieldId);

        // Person
        var getPerson = deskproTicket.Value.Person != null
            ? deskpro.GetPersonById(deskproTicket.Value.Person.Id, cancellationToken)
            : Task.FromResult(Error.NotFound().ToErrorOr<PersonDto>());

        // Agent
        var getAgent = deskproTicket.Value.Agent != null
            ? deskpro.GetPersonById(deskproTicket.Value.Agent.Id, cancellationToken)
            : Task.FromResult(Error.NotFound().ToErrorOr<PersonDto>());

        // Team
        var getTeam = deskproTicket.Value.AgentTeamId != null
            ? deskpro.GetTeam((int)deskproTicket.Value.AgentTeamId, cancellationToken)
            : Task.FromResult(Error.NotFound().ToErrorOr<TeamDto>());

        // Database ticket
        var getDatabaseTicket = ticketRepository.GetByDeskproTicketId(job.TicketId);

        await Task.WhenAll([
            getPerson,
            getAgent,
            getDatabaseTicket,
            getTeam]);
        
        getPerson.Result.LogResultErrors(logger);
        getAgent.Result.LogResultErrors(logger);
        getTeam.Result.LogResultErrors(logger);

        if (getDatabaseTicket.Result is null)
        {
            logger.LogError("Deskpro ticket {ticketId} not found in database. Moving message to DLQ.", job.TicketId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: $"Deskpro ticket {job.TicketId} not found in database", cancellationToken: cancellationToken);
            return;
        }

        var ansøger = getPerson.Result.IsError ? null : getPerson.Result.Value;
        var team = getTeam.Result.IsError ? null : getTeam.Result.Value;
        var agent = getAgent.Result.IsError ? null : getAgent.Result.Value;
        
        // Create OpenOrchestrator queue item
        var payload = new
        {
            AnsøgerNavn = ansøger?.FullName,
            AnsøgerEmail = ansøger?.Email,
            Afdeling = team?.Name,
            Aktindsigtsovermappe = getDatabaseTicket.Result?.SharepointFolderName,
            SagsbehandlerEmail = agent?.Email,
            DeskProID = job.TicketId,
            AktindsigtsDato = modtagelsesdato,
            Lovgivning = lovgivning
        };
        
        await openOrchestrator.AddQueueItem(openOrchestratorQueueName, $"DeskproID {job.TicketId.ToString()}", payload, cancellationToken);
    }
    
    private static DateTime? GetDateTimeFieldValue(TicketDto deskproTicket, int fieldId)
    {
        var fieldValue = deskproTicket.Fields.FirstOrDefault(x => x.Id == fieldId)?.Values.FirstOrDefault();
        return fieldValue.TryParseDeskproDateTime(out var datetime) ? datetime : null;
    }

    private static string GetChoiceFieldValue(TicketDto deskproTicket, int fieldId)
    {
        var choiceId = Convert.ToInt32(deskproTicket.Fields.FirstOrDefault(x => x.Id == fieldId)?.Values.FirstOrDefault());
        var choices = deskproTicket.Fields.FirstOrDefault(x => x.Id == fieldId)?.Choices;
        if (choices != null && choices.TryGetValue(choiceId, out var value))
        {
            return value;
        }

        return string.Empty;
    }
}