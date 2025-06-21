using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Contracts;
using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Exceptions;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;

namespace Aktbob.Processors.OS2FormsSubmissions;

public class RegisterSubmission(
    ILogger<RegisterSubmission> logger,
    IConfiguration configuration,
    IOS2FormsModule os2Forms,
    IUnitOfWork unitOfWork)
{
    [Function("register-submission")]
    public async Task Run(
        [ServiceBusTrigger("%RegisterSubmissionQueueName%", Connection = "AzureServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Message ID: {id}", message.MessageId);
        logger.LogInformation("Message Body: {body}", message.Body);
        logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        var job = MessageDeserializer.Deserialize<RegisterOS2FormsSubmissionJob>(message);
        
        Guard.Against.Null(job.SubmissionId);
        Guard.Against.Zero(job.TicketId);
        
        var webformId = Guard.Against.NullOrEmpty(configuration.GetValue<string>("WebformId"));
        var descriptionFieldId = Guard.Against.NullOrEmpty(configuration.GetValue<string>("DescriptionFieldId"));

        var existingSubmission = await unitOfWork.OS2FormsSubmissions.GetBySubmissionId(job.SubmissionId);
        if (existingSubmission != null)
        {
            // All done
            logger.LogInformation("Submission {submissionId} already registered.", job.SubmissionId);
            return;
        }

        var submission = await os2Forms.GetSubmission(job.SubmissionId, webformId, cancellationToken);
        if (submission.IsError)
        {
            throw new BusinessException($"{submission.FirstError.Code}: {submission.FirstError.Description}");
        }

        // Save submission to database
        var entity = new OS2FormsSubmission
        {
            SubmissionId = job.SubmissionId,
            DeskproTicketId = job.TicketId,
            DescriptionFieldValue = submission.Value.Data.FirstOrDefault(x => x.Key == descriptionFieldId).Value ?? string.Empty
        };

        logger.LogInformation("Persisting OS2Forms submission {id} DeskproTicketId {deskproTicketId}", job.SubmissionId, job.TicketId); // temp - remove after adding logging decorator to the repository methods

        var success = await unitOfWork.OS2FormsSubmissions.Add(entity);
        if (!success)
        {
            throw new BusinessException($"Failure persisting OS2Forms submission {job.SubmissionId}");
        }
    }
}