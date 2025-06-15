using Aktbob.Modules.Podio.Features.GetItem;
using Aktbob.Modules.Podio.Features.PostComment;
using Aktbob.Modules.Podio.Features.UpdateTextField;
using AktBob.Shared.Contracts.Modules.Podio;
using AktBob.Shared.Extensions;
using AktBob.Shared.Types.Podio;
using Microsoft.AspNetCore.Mvc;

namespace Aktbob.Modules.Podio.Endpoints;

internal static class EndpointHandlers
{
    public static async Task<IResult> GetItem(int appId, long itemId, [FromServices] IGetItemHandler handler, CancellationToken cancellationToken = default)
    {
        var result = await handler.Handle(ItemId.Create(appId, itemId), cancellationToken);
        return result.ToMinimalApiResponse(value => value);
    }

    public static async Task<IResult> PostComment(int appId, long itemId, [FromBody] PostCommentRequest request, [FromServices] IPostCommentHandler handler, CancellationToken cancellationToken = default)
    {
        var result = await handler.Handle(ItemId.Create(appId, itemId), request.Value, cancellationToken);
        return result.ToMinimalApiResponse();
    }

    public static async Task<IResult> UpdateTextField(int appId, long itemId, [FromBody] UpdateFieldRequest request,
        [FromServices] IUpdateTextFieldHandler handler, CancellationToken cancellationToken = default)
    {
        var result = await handler.Handle(ItemId.Create(appId, itemId), request.FieldId, request.Value, cancellationToken);
        return result.ToMinimalApiResponse();
    }
}