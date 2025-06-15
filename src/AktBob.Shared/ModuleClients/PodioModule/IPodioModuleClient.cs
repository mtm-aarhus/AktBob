using AktBob.Shared.Contracts.Modules.Podio;
using ErrorOr;

namespace AktBob.Shared.ModuleClients.PodioModule;

public interface IPodioModuleClient
{
    Task<ErrorOr<ItemDto>> GetItem(int appId, long itemId, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> PostComment(int appId, long itemId, PostCommentRequest request,  CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> UpdateField(int appId, long itemId, UpdateFieldRequest request,  CancellationToken cancellationToken = default);
}