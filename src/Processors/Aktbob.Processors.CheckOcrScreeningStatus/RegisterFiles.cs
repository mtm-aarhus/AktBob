using System.Collections.ObjectModel;
using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using Aktbob.Processors.CheckOcrScreeningStatus.Jobs;
using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Extensions;
using AktBob.Shared.ModuleClients.FilArkiv;
using Ardalis.GuardClauses;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.CheckOcrScreeningStatus;

public class RegisterFiles(
    ILogger<RegisterFiles> logger,
    IFilArkivModuleClient filArkiv,
    IOcrScreeningStatusRepository repository,
    IConfiguration configuration,
    IMessageBus messageBus)
{

    [Function("register-files")]
    public async Task Run(
        [ServiceBusTrigger("%QueueNames:RegisterFiles%", Connection = "AzureServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Message ID: {id} Body: {body} Content-Type: {contentType}", message.MessageId,  message.Body, message.ContentType);
        var job = MessageDeserializer.Deserialize<OcrScreeningStatusRegisterFilesJob>(message);
        var fileIds = new Collection<Guid>();
        var queryFileQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("QueueNames:QueryFile"));
        
        // Get document from FilArkiv
        var documents = await filArkiv.GetDocumentsByCaseId(job.FilArkivCaseId, cancellationToken);
        if (documents.IsError)
        {
            logger.LogError("Error getting documents for case {filArkivCaseId} from FilArkiv. Moving to DLQ.", job.FilArkivCaseId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: $"Error getting documents for case {job.FilArkivCaseId} from FilArkiv", deadLetterErrorDescription: documents.Errors.ToCommaDelimitedString(), cancellationToken: cancellationToken);
            return;
        }
        
        // Persist each file in database
        foreach (var document in documents.Value)
        {
            var documentFileIds = document.Files.Select(f => f.Id);
            foreach (var documentFileId in documentFileIds)
            {
                fileIds.Add(documentFileId);
                var file = new OcrScreeningStatus
                {
                    PodioItemId = job.PodioItemId,
                    FilArkivCaseId = job.FilArkivCaseId,
                    FilArkivFileId = documentFileId
                };
                
                await repository.Add(file);
            }
        }
        
        // Dispatch message for each files
        await messageBus.SendMessages(queryFileQueueName, fileIds.Select(x => new QueryFileJob(x)).ToArray<object>(), cancellationToken: cancellationToken);
        logger.LogInformation("Case {caseId}: {count} files registered", job.FilArkivCaseId, fileIds.Count);

        // Maybe update Podio
        if (Settings.ShouldPodioItemBeUpdatedImmediately(configuration))
        {
            var updatePodioItemQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("QueueNames:UpdatePodioItem"));
            await messageBus.SendMessage(updatePodioItemQueueName, new UpdatePodioItemJob(job.PodioItemId), cancellationToken: cancellationToken);
        }
    }
}