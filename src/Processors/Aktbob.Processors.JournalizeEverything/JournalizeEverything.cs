using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Shared;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Extensions;
using AktBob.Shared.ModuleClients.DeskproModule;
using AktBob.Shared.ModuleClients.OpenOrchestratorModule;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Ardalis.GuardClauses;

namespace Aktbob.Processors.JournalizeEverything;

public class JournalizeEverything(
    ILogger<JournalizeEverything> logger,
    IConfiguration configuration,
    IDeskproModuleClient deskpro,
    IOpenOrchestratorModuleClient openOrchestrator,
    ITicketRepository ticketRepository)
{
    [Function("JournalizeEverything")]
    public async Task Run(
        [ServiceBusTrigger("%ServiceBusQueueName%", Connection = "AzureServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Message ID: {id} Body: {body} Content-Type: {contentType}", message.MessageId,  message.Body, message.ContentType);
        var job = MessageDeserializer.Deserialize<JournalizeEverythingJob>(message);
        
        // Get data
        var databaseTicket = GetDatabaseTicket(job.TicketId, cancellationToken);
        var agent = DeskproHelpers.GetTicketAgent(deskpro, job.TicketId, cancellationToken);
        await Task.WhenAll([databaseTicket, agent]);

        if (databaseTicket.Result is null)
        {
            logger.LogError("Database ticket by Deskpro ID {ticketId} not found. Moving message to DLQ.", job.TicketId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: $"Database ticket by Deskpro ID {job.TicketId} not found", cancellationToken: cancellationToken);
            return;
        }
        
        agent.Result.LogResultErrors(logger);
        
        // Create OpenOrchestrator queue item
        await CreateOpenOrchestratorQueueItem(
            ticketId: job.TicketId,
            caseNumber: databaseTicket.Result?.CaseNumber,
            sharepointFolderName: databaseTicket.Result?.SharepointFolderName,
            agent: agent.Result.Value,
            cancellationToken);
    }

    private async Task CreateOpenOrchestratorQueueItem(int ticketId, string? caseNumber, string? sharepointFolderName, PersonDto? agent, CancellationToken cancellationToken)
    {
        var payload = new
        {
            Aktindsigtssag = caseNumber,
            Email = agent?.Email,
            Navn = agent?.FullName,
            DeskproID = ticketId,
            Overmappenavn = sharepointFolderName
        };

        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>($"OpenOrchestratorQueueName"));
        await openOrchestrator.AddQueueItem(openOrchestratorQueueName, $"DeskproID {ticketId.ToString()}", payload, cancellationToken);
    }

    private async Task<Ticket?> GetDatabaseTicket(int ticketId, CancellationToken cancellationToken)
    {
        var data = await ticketRepository.GetByDeskproTicketId(ticketId);
        if (string.IsNullOrEmpty(data?.CaseNumber))
        {
            logger.LogWarning("GetOrganized aktindsigtssagsnummer not registered for Deskpro Id {id}", ticketId);
        }

        return data;
    }
}