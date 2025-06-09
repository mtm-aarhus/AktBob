using System.Text.Json;
using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Shared;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Exceptions;
using AktBob.Shared.Extensions;
using AktBob.Shared.ModuleClients;
using AktBob.Shared.Processors;
using AktBob.Shared.Types.Deskpro;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Ardalis.GuardClauses;
using ErrorOr;
using Microsoft.Extensions.Configuration;

namespace Aktbob.Processors.JournalizeEverything;

public class JournalizeEverything(
    ILogger<JournalizeEverything> logger,
    IConfiguration configuration,
    DeskproModuleClient deskpro,
    OpenOrchestratorModuleClient openOrchestrator,
    ITicketRepository ticketRepository)
{
    [Function("JournalizeEverything")]
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
        var job = JsonSerializer.Deserialize<JournalizeEverythingJob>(message.Body, SerializerConfiguration.SerializerOptions());
        if (job is null)
        {
            throw new BusinessException($"{LogSnippets.MessageDeliveryCount(message.MessageId, message.DeliveryCount)}: Body could not be deserialized to type {nameof(JournalizeEverythingJob)}. Body content = {message.Body}");
        }
        
        // Start process
        var ticketId = TicketId.Create(job.TicketId);
        
        // Get data
        var databaseTicket = GetDatabaseTicket(ticketId, cancellationToken);
        var agent = GetAgent(ticketId, cancellationToken);
        await Task.WhenAll([databaseTicket, agent]);
        
        agent.Result.LogResultErrors(logger);
        
        // Create OpenOrchestrator queue item
        await CreateOpenOrchestratorQueueItem(
            ticketId: ticketId,
            caseNumber: databaseTicket.Result?.CaseNumber,
            sharepointFolderName: databaseTicket.Result?.SharepointFolderName,
            agent: agent.Result.Value,
            cancellationToken);
    }

    private async Task CreateOpenOrchestratorQueueItem(TicketId ticketId, string? caseNumber, string? sharepointFolderName, PersonDto? agent, CancellationToken cancellationToken)
    {
        var payload = new
        {
            Aktindsigtssag = caseNumber,
            Email = agent?.Email,
            Navn = agent?.FullName,
            DeskproID = ticketId.Value,
            Overmappenavn = sharepointFolderName
        };

        var openOrchestratorQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>($"OpenOrchestratorQueueName"));
        var result = await openOrchestrator.AddQueueItem(openOrchestratorQueueName, $"DeskproID {ticketId.ToString()}", payload, cancellationToken);
        logger.LogInformation("OpenOrchestrator Queue item created: {response}", result.Value);
    }

    private async Task<Ticket?> GetDatabaseTicket(TicketId ticketId, CancellationToken cancellationToken)
    {
        var data = await ticketRepository.GetByDeskproTicketId(ticketId);
        if (data is null) throw new BusinessException("Unable to get ticket from database");

        if (string.IsNullOrEmpty(data.CaseNumber))
        {
            logger.LogWarning("GetOrganized aktindsigtssagsnummer not registered for Deskpro Id {id}", ticketId);
        }
        
        return data;
    }

    private async Task<ErrorOr<PersonDto>> GetAgent(TicketId ticketId, CancellationToken cancellationToken)
    {
        var ticket = await deskpro.GetTicket(ticketId, cancellationToken);
        if (ticket.IsError) return ticket.Errors;
        
        var agent = ticket.Value.Agent?.Id != null
            ? await deskpro.GetPersonById(ticket.Value.Agent.Id, cancellationToken)
            : Error.NotFound().ToErrorOr<PersonDto>();

        return agent;
    }
}