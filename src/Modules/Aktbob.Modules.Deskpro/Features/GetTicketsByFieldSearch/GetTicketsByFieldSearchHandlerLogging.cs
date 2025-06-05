using AktBob.Shared.Extensions;

namespace Aktbob.Modules.Deskpro.Features.GetTicketsByFieldSearch;
internal class GetTicketsByFieldSearchHandlerLogging(IGetTicketsByFieldSearchHandler inner, ILogger<GetTicketsByFieldSearchHandler> logger) : IGetTicketsByFieldSearchHandler
{
    private readonly IGetTicketsByFieldSearchHandler _inner = inner;
    private readonly ILogger<GetTicketsByFieldSearchHandler> _logger = logger;

    public async Task<ErrorOr<IReadOnlyCollection<TicketDto>>> Handle(int[] fields, string searchValue, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Deskpro tickets by searching fields {fields} with search value = {searchValue}", fields, searchValue);

        var result = await _inner.Handle(fields, searchValue, cancellationToken);

        result.Switch(
            _ => _logger.LogInformation("Deskpro tickets by searching fields {fields} with search value {searchValue} retrieved", fields, searchValue),
            errors => _logger.LogWarning("{name}: {errors}", nameof(GetTicketsByFieldSearchHandler), errors.ToCommaDelimitedString()));

        return result;
    }
}