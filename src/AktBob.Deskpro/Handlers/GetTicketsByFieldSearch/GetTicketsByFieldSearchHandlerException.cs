using System.Net;
using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Handlers.GetTicketsByFieldSearch;
internal class GetTicketsByFieldSearchHandlerException : IGetTicketsByFieldSearchHandler
{
    private readonly IGetTicketsByFieldSearchHandler _inner;
    private readonly ILogger<GetTicketsByFieldSearchHandler> _logger;

    public GetTicketsByFieldSearchHandlerException(IGetTicketsByFieldSearchHandler inner, ILogger<GetTicketsByFieldSearchHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ErrorOr<IReadOnlyCollection<TicketDto>>> Handle(int[] fields, string searchValue, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.Handle(fields, searchValue, cancellationToken);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Error.NotFound("GetTicketsByFieldSearchHandler.NotFound", $"Deskpro tickets by search value {searchValue} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetTicketsByFieldSearchHandler));
            return Error.Failure("GetTicketsByFieldSearchHandler.Failure", ex.Message);
        }
    }
}
