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

public class PodioNotification(
    ILogger<PodioNotification> logger,
    IConfiguration configuration,
    IPodioModuleClient podio)
{
    [Function(nameof(PodioNotification))]
    public async Task Run(
        [ServiceBusTrigger("%QueueNames:PodioNotification%", Connection = "AzureServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Message ID: {id} Body: {body} Content-Type: {contentType}", message.MessageId,  message.Body, message.ContentType);
        var job = MessageDeserializer.Deserialize<PodioNotificationJob>(message);
        
        var podioAppId = Guard.Against.Null(configuration.GetValue<int?>("Podio:AppId"));
        var request = new PostCommentRequest("Screening af dokumenterne er færdig.");
        
        await podio.PostComment(podioAppId, job.PodioItemId, request, cancellationToken);
    }
}