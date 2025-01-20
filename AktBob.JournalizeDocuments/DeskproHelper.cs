using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AktBob.JournalizeDocuments;
internal class DeskproHelper(IMediator mediator, ILogger<DeskproHelper> logger, IMemoryCache cache)
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<DeskproHelper> _logger = logger;
    private readonly IMemoryCache _cache = cache;

    public async Task<Result<TicketDto>> GetDeskproTicket(int ticketId)
    {
        _logger.LogInformation("Getting Deskpro ticket #{id}", ticketId);

        var getDeskproTicketQuery = new GetDeskproTicketByIdQuery(ticketId);
        var getDeskproTicketQueryResult = await _mediator.Send(getDeskproTicketQuery);

        if (!getDeskproTicketQueryResult.IsSuccess)
        {
            _logger.LogError("Error requesting Deskpro ticket #{id}", ticketId);
            return Result.Error();
        }

        return getDeskproTicketQueryResult.Value;
    }


    public async Task<Result<PersonDto>> GetDeskproPerson(int? personId)
    {
        if (personId == null)
        {
            return Result.Error();
        }

        var cacheKey = "DESKPRO_PERSON_" + personId;

        if (_cache.TryGetValue(cacheKey, out PersonDto? person))
        {
            if (person != null)
            {
                return person;
            }
        }

        _logger.LogInformation("Getting Deskpro person #{id}", personId);

        var query = new GetDeskproPersonQuery((int)personId);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Could not get Deskpro getting person #{id}", personId);
            return Result.Error();
        }

        if (string.IsNullOrEmpty(result.Value?.Email))
        {
            _logger.LogWarning("No email for Deskpro person #{id}", personId);
        }

        _cache.Set(cacheKey, result.Value, TimeSpan.FromHours(1));
        return result.Value!;
    }


    public async Task<IEnumerable<AttachmentDto>> GetDeskproMessageAttachments(int deskproTicketId, int deskproMessageId)
    {
        _logger.LogInformation("Getting Deskpro message #{id} attachments", deskproMessageId);

        var query = new GetDeskproMessageAttachmentsQuery(deskproTicketId, deskproMessageId);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            _logger.LogError("Error getting attachments for Deskpro message #{id}.", deskproMessageId);
            return Enumerable.Empty<AttachmentDto>();
        }

        return result.Value;
    }


    public async Task<Result<MessageDto>> GetDeskproMessage(int ticketId, int messageId)
    {
        _logger.LogInformation("Getting Deskpro message #{id}", messageId);

        var query = new GetDeskproMessageByIdQuery(ticketId, messageId);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return Result.Error();
        }

        return result;
    }

    public async Task<IEnumerable<MessageDto>> GetDeskproMessages(int ticketId)
    {
        var query = new GetDeskproMessagesQuery(ticketId);
        var result = await _mediator.Send(query);
        return result;        
    }

    public string GenerateHtml(string templateFileName, Dictionary<string, string> dictionary)
    {
        var template = File.ReadAllText(templateFileName); // TODO: cache
        var html = template.ReplacePlaceholders(dictionary);
        return html;
    }

    public IEnumerable<string> GenerateListOfFieldValues(int[] fieldIds, TicketDto ticketDto, string templateFileName)
    {
        List<string> items = new();

        foreach (var fieldId in fieldIds)
        {
            var values = ticketDto.Fields.FirstOrDefault(f => f.Id == fieldId)?.Values ?? Enumerable.Empty<string>();
            var value = string.Join(", ", values);
            var kvp = new KeyValuePair<string, string>("value", value);
            var html = GenerateHtml(templateFileName, kvp.ToDictionary());
            items.Add(html);
        }

        return items;
    }

    public string GenerateMessageHtml(MessageDto deskproMessageDto, IEnumerable<AttachmentDto> attachments, string goCaseNumber, string caseTitle, int messageNumber)
    {
        var htmlTemplate = File.ReadAllText("message.html") ?? string.Empty;
        var attachmentFileNames = attachments.Select(a =>
            GenerateHtml(
                "message-attachments.html",
                new KeyValuePair<string, string>("filename", a.FileName).ToDictionary()));

        var dictionary = new Dictionary<string, string>
        {
            { "caseNumber",  goCaseNumber },
            { "title", caseTitle },
            { "messageNumber", messageNumber.ToString() ?? string.Empty },
            { "timestamp", deskproMessageDto.CreatedAt.ToString("dd-MM-yyyy HH:mm:ss") },
            { "fromName", deskproMessageDto.Person.FullName },
            { "fromEmail", deskproMessageDto.Person.Email },
            { "attachments", string.Join("", attachmentFileNames) },
            { "messageContent", deskproMessageDto.Content }
        };

        var html = htmlTemplate.ReplacePlaceholders(dictionary);
        return html;
    }
}
