using AktBob.Database.Entities;
using Ardalis.GuardClauses;
using Ardalis.Result;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace AktBob.Database.UseCases.Tickets.GetTickets;
internal class GetTicketsQueryHandler : IRequestHandler<GetTicketsQuery, Result<IEnumerable<Ticket>>>
{
    private readonly IConfiguration _configuration;

    public GetTicketsQueryHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Result<IEnumerable<Ticket>>> Handle(GetTicketsQuery request, CancellationToken cancellationToken)
    {
        var connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("Database"));

        using (var connection = new SqlConnection(connectionString))
        {
            // Prepare filter
            var filter = new List<string>();

            if (request.DeskproId != null)
            {
                filter.Add($"{Constants.T_TICKETS}.{Constants.T_TICKETS_DESKPRO_ID} = {request.DeskproId.ToString()}");
            }

            if (!request.IncludeClosedTickets)
            {
                filter.Add($"{Constants.T_TICKETS}.{Constants.T_TICKETS_CLOSED_AT} IS NULL");
            }

            if (request.PodioItemId != null)
            {
                filter.Add($"{Constants.T_CASES}.{Constants.T_CASES_PODIO_ITEM_ID} = '{request.PodioItemId.ToString()}'");
            }

            if (request.FilArkivCaseId != null)
            {
                filter.Add($"{Constants.T_CASES}.{Constants.T_CASES_FILARKIV_CASE_ID} = '{request.FilArkivCaseId.ToString()}'");
            }

            var filterString = string.Join(" AND ", filter);

            var getTicketIdsSql = $"SELECT {Constants.T_TICKETS}.{Constants.T_TICKETS_ID} FROM {Constants.T_TICKETS} LEFT JOIN {Constants.T_CASES} ON {Constants.T_TICKETS}.{Constants.T_TICKETS_ID} = {Constants.T_CASES}.{Constants.T_CASES_TICKET_ID}";

            if (!string.IsNullOrEmpty(filterString))
            {
                getTicketIdsSql += " WHERE " + filterString;
            }


            var ticketIds = await connection.QueryAsync<int>(getTicketIdsSql, commandType: CommandType.Text);

            if (ticketIds != null && ticketIds.Count() > 0)
            {
                var getDataSql = @$"SELECT {Constants.T_TICKETS}.*, {Constants.T_CASES}.* 
                       FROM {Constants.T_TICKETS}
                       LEFT JOIN {Constants.T_CASES} ON {Constants.T_TICKETS}.{Constants.T_TICKETS_ID} = {Constants.T_CASES}.{Constants.T_CASES_TICKET_ID}
                       WHERE {Constants.T_TICKETS}.{Constants.T_TICKETS_ID} IN ({string.Join(",", ticketIds)})";

                // Query database
                var dictionary = new Dictionary<int, Ticket>();
                var tickets = await connection.QueryAsync<Ticket, Case, Ticket>(
                    getDataSql,
                    (ticket, @case) =>
                    {
                        // Map tickets with case children
                        Ticket t;

                        if (!dictionary.TryGetValue(ticket.Id, out t))
                        {
                            t = ticket;
                            t.Cases = new List<Case>();
                            dictionary.Add(t.Id, t);
                        }

                        if (@case is not null)
                        {
                            t.Cases.Add(@case);
                        }

                        return t;

                    },
                    splitOn: Constants.T_CASES_ID,
                    commandType: CommandType.Text);

                // Return
                return Result.Success(tickets.Distinct());
            }

            return Result.Success(Enumerable.Empty<Ticket>());
        }
    }
}
