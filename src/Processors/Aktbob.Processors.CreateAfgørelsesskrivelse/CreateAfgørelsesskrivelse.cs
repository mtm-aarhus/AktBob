using System.Text.Json;
using AktBob.Database.Contracts;
using AktBob.Shared;
using AktBob.Shared.Contracts.Modules.Deskpro;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using AktBob.Shared.Contracts.Modules.OpenOrchestrator;
using AktBob.Shared.Exceptions;
using AktBob.Shared.Extensions;
using AktBob.Shared.ModuleClients;
using AktBob.Shared.Types.Deskpro;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Ardalis.GuardClauses;
using ErrorOr;
using Microsoft.Extensions.Configuration;

namespace Aktbob.Processors.CreateAfgørelsesskrivelse;

public class CreateAfgørelsesskrivelse(
    ILogger<CreateAfgørelsesskrivelse> logger,
    IConfiguration configuration,
    DeskproModuleClient deskpro,
    OpenOrchestratorModuleClient openOrchestrator,
    ITicketRepository ticketRepository)
{
    
    [Function("CreateAfgoerelsesskrivelse")]
    public async Task Run(
        [ServiceBusTrigger("%QueueName%", Connection = "AzureServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Message ID: {id}", message.MessageId);
        logger.LogInformation("Message Body: {body}", message.Body);
        logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
        
        // Deserialize message body to expected job type
        var job = JsonSerializer.Deserialize<CreateAfgørelsesskrivelseJob>(message.Body, SerializerConfiguration.SerializerOptions());
        if (job is null) throw new BusinessException($"Message {message.MessageId} error: Body could not be deserialized to type {nameof(CreateAfgørelsesskrivelseJob)}. Body content = {message.Body}");
        
        // Complete the message
        await messageActions.CompleteMessageAsync(message, cancellationToken);
        
        // Start process
        var ticketId = TicketId.Create(job.TicketId);
        
        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("OpenOrchestratorQueueName"));
        var deskproModtagelsesdatoFieldId = Guard.Against.Null(configuration.GetValue<int>("ModtagelsesdatoFieldId"));
        var deskproLovgivningFieldId = Guard.Against.Null(configuration.GetValue<int>("LovgivningFieldId"));
    
        
        // Get data from Deskpro
        var deskproTicket = await deskpro.GetTicket(ticketId, cancellationToken);
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
        var getDatabaseTicket = ticketRepository.GetByDeskproTicketId(ticketId);

        await Task.WhenAll([
            getPerson,
            getAgent,
            getDatabaseTicket,
            getTeam]);

        if (getDatabaseTicket.Result is null) throw new BusinessException("Unable to get ticket from database");
        
        var ansøger = getPerson.Result.IsError ? null : getPerson.Result.Value;
        var team = getTeam.Result.IsError ? null : getTeam.Result.Value;
        var agent = getAgent.Result.IsError ? null : getAgent.Result.Value;
        
        // Create OpenOrchestrator queue item
        var payload = new
        {
            AnsøgerNavn = ansøger?.FullName ?? string.Empty,
            AnsøgerEmail = ansøger?.Email ?? string.Empty,
            Afdeling = team?.Name ?? string.Empty,
            Aktindsigtsovermappe = getDatabaseTicket.Result?.SharepointFolderName,
            SagsbehandlerEmail = agent?.Email,
            DeskProID = ticketId.Value,
            AktindsigtsDato = modtagelsesdato,
            Lovgivning = lovgivning
        };
        
        var result = await openOrchestrator.AddQueueItem(openOrchestratorQueueName, $"DeskproID {job.TicketId.ToString()}", payload, cancellationToken);
        logger.LogInformation("Queue item added: {id}", result.Value);
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