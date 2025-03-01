using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace AktBob.JobHandlers.Utils;
internal class DeskproHelper(ILogger<DeskproHelper> logger, IMemoryCache cache)
{
    private readonly ILogger<DeskproHelper> _logger = logger;
    private readonly IMemoryCache _cache = cache;

    public async Task<Result<TicketDto>> GetTicket(IGetDeskproTicketHandler handler, int ticketId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro ticket #{id}", ticketId);

        var getDeskproTicketQueryResult = await handler.Handle(ticketId, cancellationToken);

        if (!getDeskproTicketQueryResult.IsSuccess)
        {
            _logger.LogError("Error requesting Deskpro ticket #{id}", ticketId);
            return Result.Error();
        }

        return getDeskproTicketQueryResult.Value;
    }


    public async Task<Result<PersonDto>> GetPerson(IGetDeskproPersonHandler handler, int personId, CancellationToken cancellationToken)
    {
        if (personId == 0)
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

        var result = await handler.Handle((int)personId, cancellationToken);

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


    public async Task<(string Name, string Email)> GetAgent(IGetDeskproPersonHandler handler, int agentId, CancellationToken cancellationToken)
    {
        if (agentId == 0)
        {
            _logger.LogWarning("No agent assigned to ticket");
            return (string.Empty, string.Empty);
        }

        var result = await handler.Handle(agentId, cancellationToken);

        if (result.IsSuccess && result.Value.IsAgent)
        {
            return (result.Value.FullName, result.Value.Email);
        }
        else
        {
            _logger.LogWarning($"Unable to get agent from Deskpro, agent id {agentId}");
        }

        return (string.Empty, string.Empty);
    }


    public async Task<IEnumerable<AttachmentDto>> GetMessageAttachments(IGetDeskproMessageAttachmentsHandler handler, int deskproTicketId, int deskproMessageId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro message #{id} attachments", deskproMessageId);

        var result = await handler.Handle(deskproTicketId, deskproMessageId, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogError("Error getting attachments for Deskpro message #{id}.", deskproMessageId);
            return Enumerable.Empty<AttachmentDto>();
        }

        return result.Value;
    }
}
