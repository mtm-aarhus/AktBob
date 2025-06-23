using Aktbob.Processors.CheckOcrScreeningStatus.Jobs;
using AktBob.Shared;
using AktBob.Shared.Contracts.Modules.Podio;
using AktBob.Shared.ModuleClients.PodioModule;
using Ardalis.GuardClauses;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aktbob.Processors.CheckOcrScreeningStatus.Functions;

public class UpdatePodioItem(
    ILogger<UpdatePodioItem> logger,
    IConfiguration configuration,
    IPodioModuleClient podio)
{
    [Function("update-podio-item")]
    public async Task Run(
        [ServiceBusTrigger("%QueueNames:UpdatePodioItem%", Connection = "AzureServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        logger.LogInformation("Message ID: {id} Body: {body} Content-Type: {contentType}", message.MessageId,  message.Body, message.ContentType);
        var job = MessageDeserializer.Deserialize<UpdatePodioItemJob>(message);
        
        var podioAppId = Guard.Against.Null(configuration.GetValue<int>("Podio:AppId"));
        var filArkivCaseIdFieldId = Guard.Against.Null(configuration.GetValue<int>("Podio:Fields:FilArkivCaseId"));
        var filArkivLinkFieldId = Guard.Against.Null(configuration.GetValue<int>("Podio:Fields:FilArkivLink"));

        await Task.WhenAll([
            podio.UpdateField(podioAppId, job.PodioItemId, new UpdateFieldRequest(filArkivLinkFieldId, job.FilArkivCaseId.ToString())),
            podio.UpdateField(podioAppId, job.PodioItemId, new UpdateFieldRequest(filArkivCaseIdFieldId, $"https://aarhus.filarkiv.dk/archives/case/{job.FilArkivCaseId}"))
        ]);
    }
}