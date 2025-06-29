using System.Collections.ObjectModel;
using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using Aktbob.Modules.FilArkiv.Features.GetDocumentsByCaseId;
using Aktbob.Processors.CheckOcrScreeningStatus.Features.NotificationDispatcher;
using Aktbob.Processors.CheckOcrScreeningStatus.Features.QueryFile;
using Aktbob.Processors.CheckOcrScreeningStatus.Features.UpdatePodioItem;
using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using AktBob.Shared.Extensions;
using Ardalis.GuardClauses;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.CheckOcrScreeningStatus.Features.RegisterFiles;

public class RegisterFilesHandler(
    ILogger<RegisterFilesHandler> logger,
    IGetDocumentsByCaseIdHandler filArkivGetDocumentsByCaseIdHandler,
    IOcrScreeningStatusRepository repository,
    IConfiguration configuration,
    IMessageBus messageBus)
{

    public async Task<ErrorOr<Success>> Run(OcrScreeningStatusRegisterFilesJob job, CancellationToken cancellationToken)
    {
        var fileIds = new Collection<Guid>();
        var queryFileQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("CheckOcrScreeningStatus:ServiceBusQueueNames:QueryFile"));
        var dispatchNotificationQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("CheckOcrScreeningStatus:ServiceBusQueueNames:DispatchNotification"));
        
        // Get document from FilArkiv
        var documents = await filArkivGetDocumentsByCaseIdHandler.Handle(job.FilArkivCaseId, cancellationToken);
        if (documents.IsError)
        {
            logger.LogError("Error getting documents for case {filArkivCaseId} from FilArkiv. Moving to DLQ.", job.FilArkivCaseId);
            return Error.Failure($"Error getting documents for case {job.FilArkivCaseId} from FilArkiv", documents.Errors.ToCommaDelimitedString());
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

                var existing = await repository.Get(file.FilArkivFileId);
                if (existing == null) await repository.Add(file);
                else await repository.Update(file);
            }
        }
        
        logger.LogInformation("Case {caseId}: {count} files registered", job.FilArkivCaseId, fileIds.Count);
        
        // Dispatch message for each files
        await messageBus.SendMessages(queryFileQueueName, fileIds.Select(x => new QueryFileJob(x)).ToArray<object>(), cancellationToken: cancellationToken);

        // Dispatch notification message
        await messageBus.SendMessage(dispatchNotificationQueueName, new DispatchNotificationJob(job.PodioItemId, job.FilArkivCaseId), cancellationToken);
        
        // Maybe update Podio
        if (Settings.ShouldPodioItemBeUpdatedImmediately(configuration))
        {
            var updatePodioItemQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("CheckOcrScreeningStatus:ServiceBusQueueNames:UpdatePodioItem"));
            await messageBus.SendMessage(updatePodioItemQueueName, new UpdatePodioItemJob(job.PodioItemId, job.FilArkivCaseId), cancellationToken: cancellationToken);
        }

        return Result.Success;
    }
}