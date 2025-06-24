using Aktbob.Processors.Cleanup.Jobs;
using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.ModuleClients.DeskproModule;
using Ardalis.GuardClauses;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.Cleanup.Functions;

public class DispatchCleanupJobs(
    ILogger<DispatchCleanupJobs> logger,
    IConfiguration configuration,
    IDeskproModuleClient deskpro,
    IMessageBus messageBus)
{
    [Function("dispatch-cleanup-jobs")]
    public async Task Run(
        [ServiceBusTrigger("%ServiceBusQueueNameCleanUp%", Connection = "AzureServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Message ID: {id}", message.MessageId);
        logger.LogInformation("Message Body: {body}", message.Body);
        logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        var job = MessageDeserializer.Deserialize<CleanupJob>(message);
        
        var cleanupFilArkivQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("ServiceBusQueueNameCleanUpFilArkiv"));
        var cleanupSharepointQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("ServiceBusQueueNameCleanUpSharepoint"));
        var validWorkflowChoices = Guard.Against.Null(configuration.GetValue<string>("DispatchCleanupJobs:ValidWorkflowChoices")).Split(",");
        var afslutningsdatoFieldId = Guard.Against.Zero(configuration.GetValue<int>("DispatchCleanupJobs:AfslutningsdatoFieldId"));
        var workflowFieldId = Guard.Against.Zero(configuration.GetValue<int>("DispatchCleanupJobs:WorkflowFieldId"));
       
        // Check Deskpro to ensure
        // A. it's time to execute
        // B. the ticket is still closed
        
        var deskproTicket = await Shared.GetDeskproTicket(job.DeskproId, deskpro, cancellationToken);
        if (deskproTicket.IsError)
        {
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason:$"Error getting Deskpro ticket {job.DeskproId}", cancellationToken: cancellationToken);
            return;
        }

        var workflowFieldValue =  Shared.ParseWorkflowValue(deskproTicket.Value, workflowFieldId);
        var afslutningsdatoFieldValue = Shared.ParseAfslutningsdatoValue(deskproTicket.Value, afslutningsdatoFieldId);


        // If the ticket is not closed -> exit the job
        if (workflowFieldValue is null || !validWorkflowChoices.Contains(workflowFieldValue.ToString()))
        {
            logger.LogWarning("Cannot create cleanup queue items since Deskpro ticket {ticketId} is not closed.", job.DeskproId);
            return;
        }

        // If there is no timestamp in Deskpro -> exit the job
        if (afslutningsdatoFieldValue is null)
        {
            logger.LogWarning("Cannot create cleanup queue items since Deskpro ticket {ticketId} does not have a value for afslutningsdato.", job.DeskproId);
            return;
        }

        // If it's too early, reschedule the job (Deskpro timestamp may have changed since the job initially was scheduled)
        if (await JobHasBeenRescheduled(job.DeskproId, (DateTime)afslutningsdatoFieldValue))
        {
            return;
        }

        // If it's time -> create the cleanup jobs
        await messageBus.SendMessage(cleanupFilArkivQueueName, new CreateCleanupFilArkivQueueItemJob(job.DeskproId), cancellationToken);
        await messageBus.SendMessage(cleanupSharepointQueueName, new CreateCleanupSharepointQueueItemJob(job.DeskproId), cancellationToken);
    }
    
    private async Task<bool> JobHasBeenRescheduled(int ticketId, DateTime afslutningsdato)
    {
        var executionDelayDays = Guard.Against.Zero(configuration.GetValue<int>("DispatchCleanupJobs:ExecutionDelayDays"));

        // If cleanup jobs are due, do not reschedule
        if (DateTime.UtcNow >= afslutningsdato.AddDays(executionDelayDays))
        {
            logger.LogInformation("Creating cleanup jobs for Deskpro ticket {ticketId} are due.", ticketId);
            return false;
        }

        await Reschedule(afslutningsdato, ticketId);
        return true;
    }

    private async Task Reschedule(DateTime afslutningsdato, int ticketId)
    {
        var executionDelayDays = Guard.Against.Zero(configuration.GetValue<int>("DispatchCleanupJobs:ExecutionDelayDays"));
        var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("ServiceBusQueueNameCleanUp"));
        var offset = new DateTimeOffset(afslutningsdato.AddDays(executionDelayDays));
        var now = DateTimeOffset.UtcNow;
        var daysDifference = offset - now;
        
        logger.LogInformation("Rescheduling cleanup job for Deskpro ticket {ticketId}. Next try in {days} days.", ticketId, Math.Round(daysDifference.TotalDays, 2));

        var rescheduledJob = new CleanupJob(ticketId);
        await messageBus.ScheduleMessage(queueName, rescheduledJob, offset.AddHours(1));
    }
    
    
}