using System.Text.Json;
using Aktbob.Modules.Deskpro.Features.DownloadMessageAttachment;
using Aktbob.Modules.Deskpro.Features.GetCustomFieldSpecifications;
using Aktbob.Modules.Deskpro.Features.GetMessage;
using Aktbob.Modules.Deskpro.Features.GetMessageAttachments;
using Aktbob.Modules.Deskpro.Features.GetMessages;
using Aktbob.Modules.Deskpro.Features.GetPersonByEmail;
using Aktbob.Modules.Deskpro.Features.GetPersonById;
using Aktbob.Modules.Deskpro.Features.GetTeam;
using Aktbob.Modules.Deskpro.Features.GetTicket;
using Aktbob.Modules.Deskpro.Features.GetTicketsByFieldSearch;
using Aktbob.Modules.Deskpro.Features.InvokeWebhook;
using AktBob.Shared.Extensions;
using AktBob.Shared.Types.Deskpro;
using Microsoft.AspNetCore.Mvc;

namespace Aktbob.Modules.Deskpro;

internal static class EndpointsHandlers
{
    public static async Task<IResult> DownloadMessageAttachment([FromServices] IDownloadMessageAttachmentHandler handler, [FromQuery] string url, CancellationToken cancellationToken)
    {
        var result = await handler.Handle(url, cancellationToken);
        return result.Match(
            value => Results.Stream(value),
            _ => result.ToMinimalApiResponse());
    }
    
    
    public static async Task<IResult> GetCustomFieldSpecifications([FromServices] IGetCustomFieldSpecificationsHandler handler, CancellationToken cancellationToken) 
    {
        var result = await handler.Handle(cancellationToken);
        return result.ToMinimalApiResponse(values => values);
    }
    
    
    public static async Task<IResult> GetMessage(int ticketId, int messageId, [FromServices] IGetMessageHandler handler, CancellationToken cancellationToken)
    {
        var id = MessageId.Create(ticketId, messageId);
        var result = await handler.Handle(id, cancellationToken);
        return result.ToMinimalApiResponse(value => value);
    }
    
    
    public static async Task<IResult> GetMessageAttachments(int ticketId, int messageId, [FromServices] IGetMessageAttachmentsHandler handler, CancellationToken cancellationToken)
    {
        var id = MessageId.Create(ticketId, messageId);
        var result = await handler.Handle(id, cancellationToken);
        return result.ToMinimalApiResponse(value => value);
    }
    
    
    public static async Task<IResult> GetMessages(int ticketId, IGetMessagesHandler handler, CancellationToken cancellationToken)
    {
        var id = TicketId.Create(ticketId);
        var result = await handler.Handle(id, cancellationToken);
        return result.ToMinimalApiResponse(value => value);
    }

    
    public static async Task<IResult> GetPersonByEmail([FromQuery] string email, [FromServices] IGetPersonByEmailHandler handler, CancellationToken cancellationToken)
    {
        var result = await handler.Handle(email, cancellationToken);
        return result.ToMinimalApiResponse(values => values);
    }
    
    
    public static async Task<IResult> GetPersonById(int id, IGetPersonByIdHandler handler, CancellationToken cancellationToken)
    {
        var result = await handler.Handle(id, cancellationToken);
        return result.ToMinimalApiResponse(values => values);
    }


    public static async Task<IResult> GetTeam(int id, IGetTeamHandler handler, CancellationToken cancellationToken)
    {
        var result = await handler.Handle(id, cancellationToken);
        return result.ToMinimalApiResponse(values => values);
    }


    public static async Task<IResult> GetTicket([FromRoute] int id, [FromServices] IGetTicketHandler handler, CancellationToken cancellationToken)
    {
        var result = await handler.Handle(id, cancellationToken);
        return result.ToMinimalApiResponse(values => values);
    }

    
    public static async Task<IResult> SearchTicketsByFields([FromQuery] string fields, [FromQuery] string searchValue, IGetTicketsByFieldSearchHandler handler, CancellationToken cancellationToken)
    {
        int[] fieldIds;
        
        try
        {
            fieldIds = fields
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(f => int.Parse(f.Trim()))
                .ToArray();
        }
        catch (FormatException)
        {
            return Results.BadRequest("Invalid format in 'fields'. Must be a comma-separated list of integers.");
        }
        
        var result = await handler.Handle(fieldIds, searchValue, cancellationToken);
        return result.ToMinimalApiResponse(values => values);
    }

    
    public static async Task<IResult> InvokeWebhook([FromRoute] string webhookId, [FromBody] JsonDocument? request, IInvokeWebhookHandler handler, CancellationToken cancellationToken)
    {
        var payload = request?.RootElement.GetRawText() ?? string.Empty;
        var result = await handler.Handle(webhookId, payload, cancellationToken);
        return result.ToMinimalApiResponse();
    }
}