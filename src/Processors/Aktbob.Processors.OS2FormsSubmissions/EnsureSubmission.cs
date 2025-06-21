using AktBob.Database.Contracts;
using AktBob.Shared;
using AktBob.Shared.Exceptions;
using AktBob.Shared.ModuleClients.DeskproModule;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ardalis.GuardClauses;

namespace Aktbob.Processors.OS2FormsSubmissions;

public class EnsureSubmission(
    ILogger<EnsureSubmission> logger,
    IConfiguration configuration,
    IOS2FormsSubmissionRepository repository,
    IDeskproModuleClient deskpro,
    IMessageBus messageBus)
{
    [Function(nameof(EnsureSubmission))]
    public async Task Run(
        [ServiceBusTrigger("%EnsureSubmission:ServiceBusQueueName%", Connection = "AzureServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Message ID: {id}", message.MessageId);
        logger.LogInformation("Message Body: {body}", message.Body);
        logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
        
        var job = MessageDeserializer.Deserialize<EnsureSubmissionJob>(message);
        
        var maxCount = configuration.GetValue<int?>("EnsureSubmission:MaxRetries") ?? 3;
        
        // Check if submission has been registered in database
        var submission = await repository.GetBySubmissionId(job.SubmissionId);
        if (submission is not null && submission.DeskproTicketId > 0)
        {
            // Submission has been registered -> do nothing
            logger.LogInformation("({count}/{max}) OS2Forms submission {id} registration status: registered", job.Count, maxCount, job.SubmissionId);
            return;
        }
        
        // Submission has not been registered in database yet -> check Deskpro
        var deskproTicket = await deskpro.SearchTicketsByFields([191], job.SubmissionId.ToString(), cancellationToken);
        if (deskproTicket is { IsError: false, Value.Count: > 0 })
        {
            switch (deskproTicket.Value.Count)
            {
                case 1:
                    // A Deskpro ticket is registered with the submission ID -> all is good
                    logger.LogWarning("({count}/{max}) OS2Forms submission {id} registration status: not registered in database but is registered in Deskpro, ticket ID {ticketId}", job.Count, maxCount, job.SubmissionId, deskproTicket.Value.First().Id);
                    return;
                
                case > 1:
                    // Multiple Deskpro tickets exists for the specified OS2Forms submission ID -> should never be possible
                    await messageActions.DeadLetterMessageAsync(message, deadLetterReason: $"OS2Forms submission {job.SubmissionId} is registered to multiple Deskpro tickets: {string.Join(",", deskproTicket.Value.Select(x => x.Id))}", cancellationToken: cancellationToken);
                    return;
            }
        }
        
        var count = job.Count + 1;
        
        // Max retries -> dead letter message
        if (count > maxCount)
        {
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: $"({job.Count}/{maxCount}) OS2Forms submission {job.SubmissionId} registration status: NOT REGISTERED!", cancellationToken: cancellationToken);
            return;
        }
        
        // Submission not yet registered -> check again maximum times then fail
        var offset = DateTimeOffset.UtcNow.AddMinutes(2);
        var ensureSubmissionsQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("EnsureSubmission:ServiceBusQueueName"));
        logger.LogWarning("({count}/{max}) OS2Forms submission {id} registration status: not registered in database, not registered in Deskpro. Checking again {time}", job.Count, maxCount, job.SubmissionId, offset);
        await messageBus.ScheduleMessage(ensureSubmissionsQueueName, job with {Count = count}, offset, cancellationToken);
    }
}