using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Exceptions;
using AktBob.Shared.ModuleClients.DeskproModule;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;

namespace Aktbob.Processors.Cleanup;

public class Notify(
    ILogger<Notify> logger,
    IConfiguration configuration,
    IDeskproModuleClient deskpro,
    IMessageBus messageBus)
{
    [Function("clean-up-notify")]
    public async Task Run(
        [ServiceBusTrigger("%ServiceBusQueueNameCleanUpNotify%", Connection = "AzureServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Message ID: {id}", message.MessageId);
        logger.LogInformation("Message Body: {body}", message.Body);
        logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
    
        var job = MessageDeserializer.Deserialize<NotifyJob>(message);
        
        Guard.Against.Zero(job.DeskproId);

        // Variables
        var validWorkflowChoices = Guard.Against.Null(configuration.GetValue<string>("DispatchCleanupJobs:ValidWorkflowChoices")).Split(",");
        var afslutningsdatoFieldId = Guard.Against.Zero(configuration.GetValue<int>("DispatchCleanupJobs:AfslutningsdatoFieldId"));
        var workflowFieldId = Guard.Against.Zero(configuration.GetValue<int>("DispatchCleanupJobs:WorkflowFieldId"));

        // Check Deskpro to ensure
        // A. it's time to execute
        // B) the ticket is still closed
        
        var deskproTicket = await Shared.GetDeskproTicket(job.DeskproId, deskpro, cancellationToken);
        if (deskproTicket.IsError) throw new BusinessException($"Error getting Deskpro ticket {job.DeskproId}");

        var workflowFieldValue = Shared.ParseWorkflowValue(deskproTicket.Value, workflowFieldId);
        var afslutningsdatoFieldValue = Shared.ParseAfslutningsdatoValue(deskproTicket.Value, afslutningsdatoFieldId);


        // If there is no timestamp in Deskpro or the ticket is not closed, exit the job
        if (workflowFieldValue is null || !validWorkflowChoices.Contains(workflowFieldValue.ToString()))
        {
            logger.LogWarning("Cannot notify about upcoming deletion since Deskpro ticket {ticketId} is not closed.", job.DeskproId);
            return;
        }

        if (afslutningsdatoFieldValue is null)
        {
            logger.LogWarning("Cannot notify about upcoming deleton since Deskpro ticket {ticketId} does not have a value for afslutningsdato.", job.DeskproId);
            return;
        }

        // If it's too early, reschedule the job (Deskpro timestamp have changed since the job initially was scheduled)
        if (await JobHasBeenRescheduled(job.DeskproId, (DateTime)afslutningsdatoFieldValue))
        {
            return;
        }

        // Get notification recipient
        var agentId = deskproTicket.Value.Agent?.Id;
        if (agentId == null)
        {
            logger.LogError("Cannot notify about upcoming cleanup. Deskpro ticket {deskproId} has no agent, moving message to dead letter queue", job.DeskproId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: $"Cannot notify about upcoming cleanup. Deskpro ticket {job.DeskproId} has no agent.", cancellationToken: cancellationToken);
            return;
        }

        var agent = await deskpro.GetPersonById((int)agentId, cancellationToken);
        if (agent.IsError) throw new BusinessException($"Agent {agentId} not found in Deskpro");

        // Send notification
        var notificationQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("ServiceBusQueueNameNotification"));
        var subject = $"{job.DeskproId}: Midlertidige dokumenter i screenings- og udleveringsmapperne bliver snart slettet";
        var fields = new Dictionary<string, string>
        {
            { "ticketId", job.DeskproId.ToString() },
            { "ticketSubject", deskproTicket.Value.Subject }
        };
        
        const string templateName = "clean-up-notification"; // TODO: make this a shared constant 
        var notificationJob = new NotificationJob(agent.Value.Email, templateName, subject, fields);
        await messageBus.SendMessage(notificationQueueName, notificationJob, cancellationToken);
    }
    
    private async Task<bool> JobHasBeenRescheduled(int ticketId, DateTime afslutningsdato)
    {
        var executionDelayDays = Guard.Against.Zero(configuration.GetValue<int>("DispatchCleanupJobs:NotificationDelayDays"));

        // If cleanup jobs are due, do not reschedule
        if (DateTime.UtcNow >= afslutningsdato.AddDays(executionDelayDays))
        {
            logger.LogInformation("Notification about upcoming deletion for Deskpro ticket {ticketId} is due.", ticketId);
            return false;
        }

        await Reschedule(afslutningsdato, ticketId);
        return true;
    }

    private async Task Reschedule(DateTime afslutningsdato, int ticketId)
    {
        var executionDelayDays = Guard.Against.Zero(configuration.GetValue<int>("DispatchCleanupJobs:NotificationDelayDays"));
        var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("ServiceBusQueueNameCleanUpNotify"));
        var offset = new DateTimeOffset(afslutningsdato.AddDays(executionDelayDays));
        var now = DateTimeOffset.UtcNow;
        var daysDifference = offset - now;
        
        logger.LogInformation("Rescheduling cleanup job for Deskpro ticket {ticketId}. Next try in {days} days.", ticketId, Math.Round(daysDifference.TotalDays, 2));

        var rescheduledJob = new NotifyJob(ticketId);
        await messageBus.ScheduleMessage(queueName, rescheduledJob, offset.AddHours(1));
    }
}