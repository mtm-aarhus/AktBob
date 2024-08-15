using AktBob.CheckOCRScreeningStatus.Events;
using AktBob.CheckOCRScreeningStatus.UseCases.GetDeskproPerson;
using AktBob.CheckOCRScreeningStatus.UseCases.GetDeskproTickets;
using AktBob.CheckOCRScreeningStatus.UseCases.GetFileStatus;
using AktBob.CheckOCRScreeningStatus.UseCases.GetPodioItem;
using AktBob.CheckOCRScreeningStatus.UseCases.PostMessageToDeskproTicket;
using AktBob.CheckOCRScreeningStatus.UseCases.RegisterDocuments;
using AktBob.CheckOCRScreeningStatus.UseCases.RemoveCaseFromCache;
using AktBob.CheckOCRScreeningStatus.UseCases.UpdatePodioItem;
using AktBob.Email.Contracts;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.CheckOCRScreeningStatus;
internal class CaseAddedConsumer : INotificationHandler<CaseAdded>
{
    private readonly ILogger<CaseAddedConsumer> _logger;
    private readonly IData _data;
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public CaseAddedConsumer(
        ILogger<CaseAddedConsumer> logger,
        IData data,
        IMediator mediator,
        IConfiguration configuration)
    {
        _logger = logger;
        _data = data;
        _mediator = mediator;
        _configuration = configuration;
    }

    public async Task Handle(CaseAdded notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting processing case {id}", notification.CaseId);

        var registerFilesCommand = new RegisterFilesCommand(notification.CaseId);
        var registerFilesResult = await _mediator.Send(registerFilesCommand);

        if (!registerFilesResult.IsSuccess)
        {
            LogErrors(registerFilesResult.Errors);
            return;
        }

        await Task.WhenAll(_data.GetCase(notification.CaseId)!.Files.Select(f => _mediator.Send(new GetFileStatusQuery(f.FileId))));

        var updatePodioCommand = new UpdatePodioItemCommand(notification.CaseId);
        var updatePodioResult = await _mediator.Send(updatePodioCommand);

        if (!updatePodioResult.IsSuccess)
        {
            LogErrors(updatePodioResult.Errors);
            return;
        }


        // ** POST EMAIL AND ADD DESKPRO AGENT NOTE **

        var ticketFields = _configuration.GetSection("Deskpro:PodioItemIdFields").Get<int[]>();

        if (ticketFields == null)
        {
            _logger.LogError("No PodioItemIdFields found in appsettings.");
        }
        else
        {
            // Get Deskpro tickets by searching the specified custom fields for the PodioItemId 
            var getTicketsQuery = new GetDeskproTicketsByFieldSearchQuery(ticketFields, _data.GetCase(notification.CaseId)!.PodioItemId.ToString());
            var deskproTickets = await _mediator.Send(getTicketsQuery);

            // Queue email
            foreach (var deskproTicket in deskproTickets.Value)
            {
                // Skip if the Deskpro ticket has no assigned agent
                if (deskproTicket.AgentId is null || deskproTicket.AgentId == 0)
                {
                    continue;
                }

                // Get agent email address from Deskpro
                var getAgentQuery = new GetDeskproPersonQuery((int)deskproTicket.AgentId);
                var getAgentResult = await _mediator.Send(getAgentQuery);

                // Queue email if the Deskpro ticket has an assigned agent
                if (getAgentResult.IsSuccess && !string.IsNullOrEmpty(getAgentResult.Value.Email) && getAgentResult.Value.IsAgent)
                {
                    var filArkivCase = _data.GetCase(notification.CaseId);

                    // Get Podio Item
                    var getPodioItemQuery = new GetPodioItemQuery(_data.GetCase(notification.CaseId)!.PodioItemId);
                    var getPodioItemResult = await _mediator.Send(getPodioItemQuery);

                    if (getPodioItemResult.IsSuccess)
                    {
                        var podioItem = getPodioItemResult.Value;
                        var sagsnummer = podioItem.Fields.FirstOrDefault(x => x.Id == 262643381)?.Value.FirstOrDefault() ?? string.Empty;
                        var aktindsigtssagsnummer = podioItem.Fields.FirstOrDefault(x => x.Id == 262643386)?.Value.FirstOrDefault() ?? string.Empty;
                        var filArkivLink = podioItem.Fields.FirstOrDefault(x => x.Id == 263817471)?.Value.FirstOrDefault() ?? string.Empty;

                        var queueEmailCommand = new QueueEmailCommand(
                            getAgentResult.Value.Email,
                            $"OCR screening af {sagsnummer} er færdig (aktindsigtsanmodning {aktindsigtssagsnummer})",
                            $"<p>Hej {getAgentResult.Value.FirstName}</p><p>OCR screening af dokumenterne fra {sagsnummer} på FilArkiv er færdig.</p><p><ul><li>Link til sagen på Podio: {podioItem.Link}</li><li>Link til dokumenterne på FilArkiv: {filArkivLink}</li></ul></p>");
                        await _mediator.Send(queueEmailCommand);

                        // Post Deskpro agent note
                        foreach (var ticket in deskproTickets.Value)
                        {
                            var postMessageToDeskproTicketCommand = new PostMessageToDeskproTicketCommand(
                                deskproTicket.Id,
                                $"<p>OCR screening af dokumenterne fra {sagsnummer} på FilArkiv er færdig.</p><p><ul><li>Link til sagen på Podio: <a href=\"{podioItem.Link}\">{podioItem.Link}</a></li><li>Link til dokumenterne på FilArkiv: <a href=\"{filArkivLink}\">{filArkivLink}</a></li></ul></p>",
                                isAgentNote: true);
                            await _mediator.Send(postMessageToDeskproTicketCommand);
                        }
                        
                        break;
                    }
                }
            }
        }

        var removeCaseFromCacheCommand = new RemoveCaseFromCacheCommand(notification.CaseId);
        await _mediator.Send(removeCaseFromCacheCommand);

        _logger.LogInformation("Case {id} processed", notification.CaseId);
    }


    
    private void LogErrors(IEnumerable<string> errors)
    {
        foreach (var error in errors)
        {
            _logger.LogError(error);
        }
    }
}