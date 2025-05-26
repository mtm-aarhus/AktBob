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

namespace AktBob.Workflows.Processes.AddOrUpdateDeskproTicketToGetOrganized;
internal class AddOrUpdateDeskproTicketToGetOrganized(ILogger<AddOrUpdateDeskproTicketToGetOrganized> logger, IServiceScopeFactory serviceScopeFactory) : IJobHandler<AddOrUpdateDeskproTicketToGetOrganizedJob>
{
    private readonly ILogger<AddOrUpdateDeskproTicketToGetOrganized> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    record ContentElement(DateTime Timestamp, byte[] Bytes);

    public async Task Handle(AddOrUpdateDeskproTicketToGetOrganizedJob job, CancellationToken cancellationToken = default)
    {
        // Validate job parameters
        Guard.Against.NegativeOrZero(job.TicketId);
        Guard.Against.NullOrEmpty(job.GOCaseNumber);

        var scope = _serviceScopeFactory.CreateScope();
        var pendingsTickets = PendingsTickets.Instance;

        var deskpro = scope.ServiceProvider.GetRequiredServiceOrThrow<IDeskproModule>();
        var messageRepository = scope.ServiceProvider.GetRequiredServiceOrThrow<IMessageRepository>();
        var cloudConvertModule = scope.ServiceProvider.GetRequiredServiceOrThrow<ICloudConvertModule>();
        var getOrganized = scope.ServiceProvider.GetRequiredServiceOrThrow<IGetOrganizedModule>();
        var currentPendingTicket = new PendingTicket(job.TicketId, job.SubmittedAt);

        pendingsTickets.AddPendingTicket(currentPendingTicket);

        // Check if this submission is the most recent for the specified ticket
        if (!IsMostRecentSubmission(currentPendingTicket, pendingsTickets))
        {
            return;
        }

        var ticketResult = await deskpro.GetTicket(job.TicketId, cancellationToken);
        if (ticketResult.IsError) throw new BusinessException("Unable to get ticket from Deskpro");
        var ticket = ticketResult.Value;

        var getTicketCustomFields = deskpro.GetCustomFieldSpecifications(cancellationToken);

        var getAgent = ticket.Agent != null
            ? deskpro.GetPersonById(ticket.Agent.Id, cancellationToken)
            : Task.FromResult(Error.NotFound().ToErrorOr<PersonDto>());

        var getUser = ticket.Person != null
            ? deskpro.GetPersonById(ticket.Person.Id, cancellationToken)
            : Task.FromResult(Error.NotFound().ToErrorOr<PersonDto>());

        await Task.WhenAll([
            getTicketCustomFields,
            getAgent,
            getUser]);

        if (getTicketCustomFields.Result.IsError) throw new BusinessException("Unable to get Deskpro custom field specifications");

        // Map ticket fields
        var customFields = GenerateCustomFieldValues(job.CustomFieldIds, getTicketCustomFields.Result.Value, ticket);
        var caseNumbers = HtmlHelper.GenerateListOfFieldValues(job.CaseNumberFieldIds, ticket, "HtmlTemplates/ticket-case-numbers.html");

        var ticketDictionary = new Dictionary<string, string>
        {
            { "ticketId", ticket.Id.ToString() },
            { "caseTitle", ticket.Subject },
            { "userName", getUser.Result.Value?.FullName ?? string.Empty },
            { "userEmail", getUser.Result.Value?.Email ?? string.Empty },
            { "userPhone", string.Join(", ", getUser.Result.Value?.PhoneNumbers ?? Enumerable.Empty<string>()) },
            { "agentName", getAgent.Result.Value?.FullName ?? string.Empty},
            { "agentEmail", getAgent.Result.Value ?.Email ?? string.Empty },
            { "custom-fields", string.Join("", customFields) },
            { "caseNumbers", string.Join("", caseNumbers) }
        };

        List<ContentElement> contentElements = new();

        var ticketHtml = HtmlHelper.GenerateHtml(ticketDictionary, "HtmlTemplates/ticket.html");
        contentElements.Add(new(DateTime.MaxValue, Encoding.UTF8.GetBytes(ticketHtml)));

        // Messages
        var getMessagesResult = await deskpro.GetMessages(ticket.Id, cancellationToken);
        if (getMessagesResult.IsError)
        {
            var messages = getMessagesResult.Value.OrderByDescending(x => x.CreatedAt);

            // Get and handle all messages at the same time
            await Task.WhenAll(messages.Select(async message =>
            {
                var person = await deskpro.GetPersonById(message.Person.Id, cancellationToken);
                message.Person = person.Value;

                // Get recipient
                var recipient = message.Recipients.FirstOrDefault() != null
                    ? await deskpro.GetPersonByEmail(message.Recipients.First(), cancellationToken)
                    : Error.NotFound().ToErrorOr<PersonDto>();

                var attachments = Enumerable.Empty<AttachmentDto>();
                if (message.AttachmentIds.Any())
                {

                    var getAttachmentsResult = await deskpro.GetMessageAttachments(message.Id, cancellationToken);
                    attachments = getAttachmentsResult.Value ?? Enumerable.Empty<AttachmentDto>();
                }

                // Get message number from API database
                var messageNumber = 0;
                var databaseMessage = await messageRepository.GetByDeskproMessageId(message.Id.Id);

                if (databaseMessage is null)
                {
                    _logger.LogWarning("No message found in database for Deskpro message ID {id}", message.Id);
                }
                else
                {
                    messageNumber = databaseMessage.MessageNumber ?? 0;
                }

                var messageHtml = HtmlHelper.GenerateMessageHtml(
                    message.IsAgentNote,
                    message.CreatedAt,
                    message.Person.FullName,
                    message.Person.Email,
                    recipient.Value?.FullName ?? string.Empty,
                    recipient.Value?.Email ?? message.Recipients.FirstOrDefault() ?? string.Empty,
                    message.Content,
                    job.GOCaseNumber,
                    ticket.Subject,
                    messageNumber,
                    attachments);
                contentElements.Add(new(message.CreatedAt, Encoding.UTF8.GetBytes(messageHtml)));
            }));
        }

        var fileResult = await GeneratePDF(cloudConvertModule, contentElements, cancellationToken);
        if (fileResult.IsError) throw new BusinessException($"Unable to generate PDF document using CloudConvert: {fileResult.Errors.ToCommaDelimitedString()}");

        // Check if this submission is the most recent for the specified ticket. Check this as late as possible.
        if (IsMostRecentSubmission(currentPendingTicket, pendingsTickets))
        {
            // Upload to GO
            var uploadDocumentCommand = new UploadDocumentCommand(
                fileResult.Value,
                job.GOCaseNumber,
                "Samlet korrespondance.pdf",
                string.Empty,
                DateTime.UtcNow.UtcToDanish(),
                UploadDocumentCategory.Internal,
                true);

            pendingsTickets.RemovePendingTicket(currentPendingTicket);
            var uploadDocumentResult = await getOrganized.UploadDocument(uploadDocumentCommand, cancellationToken);
            if (!uploadDocumentResult.IsSuccess) throw new BusinessException("Unable to uplaod ticket PDF document to GetOrganized");
        }
    }

    private async Task<ErrorOr<byte[]>> GeneratePDF(ICloudConvertModule cloudConvertModule, IList<ContentElement> contentElements, CancellationToken cancellationToken)
    {
        // Generate PDF
        var orderedContentElements = contentElements.OrderByDescending(x => x.Timestamp).Select(x => x.Bytes);
        var generateTasksResult = cloudConvertModule.GenerateTasks(orderedContentElements);
        if (generateTasksResult.IsError)
        {
            return generateTasksResult.Errors;
        }

        var jobIdResult = await cloudConvertModule.ConvertHtmlToPdf(generateTasksResult.Value, cancellationToken);
        if (jobIdResult.IsError)
        {
            return jobIdResult.Errors;
        }

        var urlResult = await cloudConvertModule.GetDownloadUrl(jobIdResult.Value, cancellationToken);
        if (urlResult.IsError)
        {
            return urlResult.Errors;
        }

        var fileResult = await cloudConvertModule.DownloadFile(urlResult.Value, cancellationToken);
        if (fileResult.IsError)
        {
            return fileResult.Errors;
        }

        return fileResult;
    }

    private bool IsMostRecentSubmission(PendingTicket pendingTicket, PendingsTickets pendingsTickets)
    {
        if (!pendingsTickets.IsMostRecent(pendingTicket))
        {
            pendingsTickets.RemovePendingTicket(pendingTicket);
            return false;
        }

        return true;
    }

    private IEnumerable<string> GenerateCustomFieldValues(int[] customFieldIds, IEnumerable<CustomFieldSpecificationDto> customFieldSpecificationDtos, TicketDto ticketDto)
    {
        List<string> items = new();

        foreach (var customFieldId in customFieldIds)
        {
            // Get the custom field specification
            var customField = customFieldSpecificationDtos.FirstOrDefault(c => c.Id == customFieldId);
            if (customField is null)
            {
                continue;
            }

            var values = ticketDto.Fields.FirstOrDefault(f => f.Id == customFieldId)?.Values ?? Enumerable.Empty<string>();
            var value = customField.Choices.Any()
                ? GetChoiceValue(values, customField)
                : values.FirstOrDefault() ?? string.Empty;


            var dictionary = new Dictionary<string, string>
                {
                    { "title", customField.Title },
                    { "value", TryParseAndFormatDateTime(value) }
                };

            var html = HtmlHelper.GenerateHtml(dictionary, "HtmlTemplates/custom-field.html");
            items.Add(html);
        }

        return items;
    }

    private string GetChoiceValue(IEnumerable<string> values, CustomFieldSpecificationDto customField)
    {
        var choiceKeys = values.Select(int.Parse);

        // Get the choices title from the custom field specification based on the ticket field choices
        var choiceTitles = customField.Choices
            .Where(kv => choiceKeys.Contains(kv.Key))
            .Select(kv => kv.Value);

        return string.Join(",", choiceTitles);
    }

    private string TryParseAndFormatDateTime(string? input)
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