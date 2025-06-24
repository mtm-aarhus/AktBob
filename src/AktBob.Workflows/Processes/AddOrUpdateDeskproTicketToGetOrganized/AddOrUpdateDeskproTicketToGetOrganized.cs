using AktBob.Database.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.GetOrganized.Contracts;
using AktBob.Shared.Extensions;
using AktBob.Shared.Jobs;
using System.Text;
using AktBob.Workflows.Helpers;
using System.Globalization;
using AktBob.CloudConvert.Contracts;
using AktBob.Deskpro.Contracts;
using HtmlHelper = AktBob.Workflows.Helpers.HtmlHelper;

namespace AktBob.Workflows.Processes.AddOrUpdateDeskproTicketToGetOrganized;
internal class AddOrUpdateDeskproTicketToGetOrganized(ILogger<AddOrUpdateDeskproTicketToGetOrganized> logger, IServiceScopeFactory serviceScopeFactory) : IJobHandler<AddOrUpdateDeskproTicketToGetOrganizedJob>
{
    private readonly ILogger<AddOrUpdateDeskproTicketToGetOrganized> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    
    private const string HtmlTemplateTicketCaseNumbers = "HtmlTemplates/ticket-case-numbers.html";
    private const string HtmlTemplateTicket = "HtmlTemplates/ticket.html";
    record ContentElement(DateTime Timestamp, byte[] Bytes);

    public async Task Handle(AddOrUpdateDeskproTicketToGetOrganizedJob job, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding/updating Deskpro ticket {ticketId} document on GetOrganized case {case}", job.TicketId, job.GOCaseNumber);
        
        // Validate job parameters
        Guard.Against.NegativeOrZero(job.TicketId);
        Guard.Against.NullOrEmpty(job.GOCaseNumber);

        using var scope = _serviceScopeFactory.CreateScope();
        var pendingTickets = PendingsTickets.Instance;

        var deskpro = scope.ServiceProvider.GetRequiredServiceOrThrow<IDeskproModule>();
        var messageRepository = scope.ServiceProvider.GetRequiredServiceOrThrow<IMessageRepository>();
        var cloudConvertModule = scope.ServiceProvider.GetRequiredServiceOrThrow<ICloudConvertModule>();
        var getOrganized = scope.ServiceProvider.GetRequiredServiceOrThrow<IGetOrganizedModule>();
        var currentPendingTicket = new PendingTicket(job.TicketId, job.SubmittedAt);

        pendingTickets.AddPendingTicket(currentPendingTicket);

        // Check if this submission is the most recent for the specified ticket
        if (!IsMostRecentSubmission(currentPendingTicket, pendingTickets))
        {
            return;
        }

        // Get data
        var (ticket, getTicketCustomFields, getAgent, getUser) = await GetData(job.TicketId, deskpro, cancellationToken);
        if (getTicketCustomFields.IsError) throw new BusinessException("Unable to get Deskpro custom field specifications");

        // Map ticket fields
        var customFields = GenerateCustomFieldValues(job.CustomFieldIds, getTicketCustomFields.Value, ticket.Value);
        var caseNumbers = HtmlHelper.GenerateListOfFieldValues(job.CaseNumberFieldIds, ticket.Value, HtmlTemplateTicketCaseNumbers);

        var ticketDictionary = new Dictionary<string, string>
        {
            { "ticketId", ticket.Value.Id.ToString() },
            { "caseTitle", ticket.Value.Subject },
            { "userName", getUser.Value?.FullName ?? string.Empty },
            { "userEmail", getUser.Value?.Email ?? string.Empty },
            { "userPhone", string.Join(", ", getUser.Value?.PhoneNumbers ?? []) },
            { "agentName", getAgent.Value?.FullName ?? string.Empty},
            { "agentEmail", getAgent.Value ?.Email ?? string.Empty },
            { "custom-fields", string.Join("", customFields) },
            { "caseNumbers", string.Join("", caseNumbers) }
        };

        List<ContentElement> contentElements = [];

        var ticketHtml = HtmlHelper.GenerateHtml(ticketDictionary, HtmlTemplateTicket);
        contentElements.Add(new ContentElement(DateTime.MaxValue, Encoding.UTF8.GetBytes(ticketHtml)));
        
        await GetMessagesContent(job.GOCaseNumber, deskpro, messageRepository, ticket.Value, cancellationToken)
            .Switch(
                value => contentElements.AddRange(value),
                errors => throw new BusinessException($"Error handling messages: {errors.ToCommaDelimitedString()}"));

        var fileResult = await GeneratePdf(cloudConvertModule, contentElements, cancellationToken);
        if (fileResult.IsError) throw new BusinessException($"Unable to generate PDF document using CloudConvert: {fileResult.Errors.ToCommaDelimitedString()}");

        // Check if this submission is the most recent for the specified ticket. Check this as late as possible.
        if (IsMostRecentSubmission(currentPendingTicket, pendingTickets))
        {
            // Upload to GO
            pendingTickets.RemovePendingTicket(currentPendingTicket);
            var uploadDocumentResult = await getOrganized.UploadDocument(
                fileResult.Value,
                job.GOCaseNumber,
                "Samlet korrespondance.pdf",
                string.Empty,
                DateTime.UtcNow.UtcToDanish(),
                UploadDocumentCategory.Internal,
                true,
                cancellationToken);
            
            if (uploadDocumentResult.IsError) throw new BusinessException(uploadDocumentResult.Errors.ToCommaDelimitedString());
        }
        
        _logger.LogInformation("Deskpro ticket {ticketId} document on GetOrganized case {case} added/updated", job.TicketId, job.GOCaseNumber);
    }

    /// <summary>
    /// Get data from Deskpro
    /// </summary>
    private static async Task<(
        ErrorOr<TicketDto> ticket,
        ErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>> getTicketCustomFields,
        ErrorOr<PersonDto> getAgent,
        ErrorOr<PersonDto> getUser)> 
        GetData(
            int ticketId,
            IDeskproModule deskpro,
            CancellationToken cancellationToken)
    {
        var ticketResult = await deskpro.GetTicket(ticketId, cancellationToken);
        if (ticketResult.IsError) throw new BusinessException("Unable to get ticket from Deskpro");

        var getTicketCustomFields = deskpro.GetCustomFieldSpecifications(cancellationToken);

        var getAgent = ticketResult.Value.Agent != null
            ? deskpro.GetPersonById(ticketResult.Value.Agent.Id, cancellationToken)
            : Task.FromResult(Error.NotFound().ToErrorOr<PersonDto>());

        var getUser = ticketResult.Value.Person != null
            ? deskpro.GetPersonById(ticketResult.Value.Person.Id, cancellationToken)
            : Task.FromResult(Error.NotFound().ToErrorOr<PersonDto>());

        await Task.WhenAll([
            getTicketCustomFields,
            getAgent,
            getUser]);
        
        return (ticketResult, getTicketCustomFields.Result, getAgent.Result, getUser.Result);
    }

    /// <summary>
    /// Returns the HTML elements representing the ticket messages 
    /// </summary>
    private async Task<ErrorOr<ContentElement[]>> GetMessagesContent(string caseNumber, IDeskproModule deskpro, IMessageRepository messageRepository, TicketDto ticket, CancellationToken cancellationToken)
    {
        return await
            deskpro.GetMessages(ticket.Id, cancellationToken)
                .Then(value => value.OrderByDescending(x => x.CreatedAt))
                .ThenAsync(value => Task.WhenAll(value.Select(async message =>
                {
                    var agent = await deskpro.GetPersonById(message.Person.Id, cancellationToken);
                    
                    var recipient = message.Recipients.FirstOrDefault() != null
                        ? await deskpro.GetPersonByEmail(message.Recipients.First(), cancellationToken)
                        : Error.NotFound().ToErrorOr<PersonDto>();

                    var attachments = await GetAttachments(message, deskpro, cancellationToken);

                    var messageHtml = HtmlHelper.GenerateMessageHtml(
                        message.IsAgentNote, 
                        message.CreatedAt,
                        agent.Value.FullName,
                        agent.Value.Email,
                        recipient.Value?.FullName ?? string.Empty,
                        recipient.Value?.Email ?? message.Recipients.FirstOrDefault() ?? string.Empty,
                        message.Content,
                        caseNumber,
                        ticket.Subject,
                        await GetMessageNumber(messageRepository, message.TicketId, message.Id),
                        attachments.Value);

                    return new ContentElement(message.CreatedAt, Encoding.UTF8.GetBytes(messageHtml));
                })
            )
        );
    }

    
    /// <summary>
    /// Get attachments metadata for a specific message
    /// </summary>
    private static async Task<ErrorOr<IReadOnlyCollection<AttachmentDto>>> GetAttachments(MessageDto message, IDeskproModule deskpro, CancellationToken cancellationToken)
    {
        if (!message.AttachmentIds.Any())
        {
            return Array.Empty<AttachmentDto>().ToErrorOr<IReadOnlyCollection<AttachmentDto>>();
        }

        return await deskpro.GetMessageAttachments(message.TicketId, message.Id, cancellationToken);
    }

    
    /// <summary>
    /// Get the user-friendly message number for a specific message
    /// </summary>
    private async Task<int> GetMessageNumber(IMessageRepository messageRepository, int ticketId, int messageId)
    {
        var databaseMessage = await messageRepository.GetByDeskproMessageId(messageId);
        if (databaseMessage is null)
        {
            _logger.LogWarning("No message found in database for Deskpro ticket {ticketId} message ID {id}", ticketId, messageId);
        }
        
        return databaseMessage?.MessageNumber ?? 0;
    }

    
    /// <summary>
    /// Generate the PDF file using CloudConvert
    /// </summary>
    private static async Task<ErrorOr<byte[]>> GeneratePdf(ICloudConvertModule cloudConvertModule, IList<ContentElement> contentElements, CancellationToken cancellationToken)
    {
        return await 
            contentElements.OrderByDescending(x => x.Timestamp).Select(x => x.Bytes).ToErrorOr()
            .Then(cloudConvertModule.GenerateTasks)
            .ThenAsync(async value => await cloudConvertModule.ConvertHtmlToPdf(value, cancellationToken))
            .ThenAsync(async value => await cloudConvertModule.GetDownloadUrl(value, cancellationToken))
            .ThenAsync(async value => await cloudConvertModule.DownloadFile(value, cancellationToken));
    }

    
    /// <summary>
    /// Checks if the current submission is the most recent
    /// </summary>
    private static bool IsMostRecentSubmission(PendingTicket pendingTicket, PendingsTickets pendingsTickets)
    {
        if (pendingsTickets.IsMostRecent(pendingTicket))
        {
            return true;
        }

        pendingsTickets.RemovePendingTicket(pendingTicket);
        return false;

    }

    
    /// <summary>
    /// Generate the HTML elements containing the custom field data
    /// </summary>
    private static IEnumerable<string> GenerateCustomFieldValues(int[] customFieldIds, IEnumerable<CustomFieldSpecificationDto> customFieldSpecificationDtos, TicketDto ticketDto)
    {
        return customFieldIds.Select(customFieldId =>
        {
            // Get the custom field specification
            var customField = customFieldSpecificationDtos.FirstOrDefault(c => c.Id == customFieldId);
            if (customField is null)
            {
                return string.Empty;
            }

            var values = ticketDto.Fields.FirstOrDefault(f => f.Id == customFieldId)?.Values ?? [];
            var value = customField.Choices.Any()
                ? GetChoiceValue(values, customField)
                : values.FirstOrDefault() ?? string.Empty;

            var dictionary = new Dictionary<string, string>
            {
                { "title", customField.Title },
                { "value", TryParseAndFormatDateTime(value) }
            };

            return HtmlHelper.GenerateHtml(dictionary, "HtmlTemplates/custom-field.html");
        }).Where(x => !string.IsNullOrEmpty(x)).ToList();
    }

    
    /// <summary>
    /// Returns the user-friendly choice value from a Deskpro choice-type field
    /// </summary>
    private static string GetChoiceValue(IEnumerable<string> values, CustomFieldSpecificationDto customField)
    {
        var choiceKeys = values.Select(int.Parse);

        // Get the choices title from the custom field specification based on the ticket field choices
        var choiceTitles = customField.Choices
            .Where(kv => choiceKeys.Contains(kv.Key))
            .Select(kv => kv.Value);

        return string.Join(",", choiceTitles);
    }

    
    /// <summary>
    /// Formats the Deskpro timestamp to a user-friendly timestamp string value
    /// </summary>
    private static string TryParseAndFormatDateTime(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        if (DateTime.TryParse(input, CultureInfo.InvariantCulture, out var dateTime))
        {
            return dateTime.Date.TimeOfDay == TimeSpan.Zero
                ? dateTime.Date.ToString("dd-MM-yyyy")
                : dateTime.Date.ToString("dd-MM-yyyy HH:mm:ss");
        }

        return input;
    }
}