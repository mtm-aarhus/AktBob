using Aktbob.Modules.FilArkiv.Features.GetDocumentsByCaseId;
using Aktbob.Modules.FilArkiv.Features.GetFileProcessStatus;
using AktBob.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Aktbob.Modules.FilArkiv.Endpoints;

internal static class EndpointHandlers
{
    public static async Task<IResult> GetDocumentsByCaseId([FromRoute] Guid caseId, [FromServices] IGetDocumentsByCaseIdHandler handler, CancellationToken cancellationToken = default)
    {
        var result = await handler.Handle(caseId, cancellationToken);
        return result.ToMinimalApiResponse(value => value);
    }
    
    public static async Task<IResult> GetFileProcessStatus([FromRoute] Guid fileId, [FromServices] IGetFileProcessStatusHandler handler, CancellationToken cancellationToken = default)
    {
        var result = await handler.Handle(fileId, cancellationToken);
        return result.ToMinimalApiResponse(value => value);
    }
}