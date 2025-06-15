using System.Text.Json;
using AktBob.Shared.Contracts.Modules.Podio;
using AktBob.Shared.Extensions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.Shared.ModuleClients.PodioModule;

internal class PodioModuleClientLogging(IPodioModuleClient next, ILogger<PodioModuleClient> logger) : IPodioModuleClient
{
    public async Task<ErrorOr<ItemDto>> GetItem(int appId, long itemId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting Podio app {appId} item {itemId}", appId, itemId);
        
        var result = await next.GetItem(appId, itemId, cancellationToken);
        result.Switch(
            _ => logger.LogInformation("Podio app {appId} item {itemId} retrieved successfully", appId, itemId),
            _ => result.LogResultErrors(logger));
        
        return result;
    }

    public async Task<ErrorOr<Success>> PostComment(int appId, long itemId, PostCommentRequest request, CancellationToken cancellationToken = default)
    {
        var body = JsonSerializer.Serialize(request, SerializerConfiguration.SerializerOptions());
        logger.LogInformation("Posting comment on Podio app {appId} item {itemId}. Payload={body}", appId, itemId, body);
        
        var result = await next.PostComment(appId, itemId, request, cancellationToken);
        result.Switch(
            _ => logger.LogInformation("Comment posted on Podio app {appId} item {itemId} successfully", appId, itemId),
            _ => result.LogResultErrors(logger));
        
        return result;
    }

    public async Task<ErrorOr<Success>> UpdateField(int appId, long itemId, UpdateFieldRequest request, CancellationToken cancellationToken = default)
    {
        var body = JsonSerializer.Serialize(request, SerializerConfiguration.SerializerOptions());
        logger.LogInformation("Updating field on Podio app {appId} item {itemId}. Payload={body}", appId, itemId, body);
        
        var result = await next.UpdateField(appId, itemId, request, cancellationToken);
        result.Switch(
            _ => logger.LogInformation("Field on Podio app {appId} item {itemId} updated successfully", appId, itemId),
            _ => result.LogResultErrors(logger));
        
        return result;
    }
}