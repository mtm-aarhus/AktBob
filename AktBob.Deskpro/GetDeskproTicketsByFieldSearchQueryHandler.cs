using AAK.Deskpro;
using AAK.Deskpro.Models;
using AktBob.Deskpro.Contracts;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.Deskpro;
internal class GetDeskproTicketsByFieldSearchQueryHandler : IRequestHandler<GetDeskproTicketsByFieldSearchQuery, Result<IEnumerable<Ticket>>>
{
    private readonly ILogger<GetDeskproTicketsByFieldSearchQueryHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly IDeskproClient _deskpro;

    public GetDeskproTicketsByFieldSearchQueryHandler(ILogger<GetDeskproTicketsByFieldSearchQueryHandler> logger, IConfiguration configuration, IDeskproClient deskpro)
    {
        _logger = logger;
        _configuration = configuration;
        _deskpro = deskpro;
    }

    public async Task<Result<IEnumerable<Ticket>>> Handle(GetDeskproTicketsByFieldSearchQuery request, CancellationToken cancellationToken)
    {
        var ticketsList = new List<Ticket>();

        foreach (var field in request.Fields)
        {
            var tickets = await _deskpro.GetTicketsByFieldValue(field, request.SearchValue, cancellationToken);

            if (tickets is not null && tickets.Count() > 0)
            {
                ticketsList.AddRange(tickets!);
            }
        };

        if (ticketsList.Count > 0)
        {
            return Result.Success(ticketsList.AsEnumerable());
        }

        return Result.NotFound();
    }
}
