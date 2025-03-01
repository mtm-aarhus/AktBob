using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using System.Data;

namespace AktBob.Database.Repositories;
internal class TicketRepository : ITicketRepository
{
    private readonly ISqlDataAccess _sqlDataAccess;

    public TicketRepository(ISqlDataAccess sqlDataAccess)
    {
        _sqlDataAccess = sqlDataAccess;
    }

    public async Task<int> Add(Ticket ticket)
    {
        var parameters = new DynamicParameters();
        parameters.Add("DeskproId", ticket.DeskproId, dbType: DbType.Int32, direction: ParameterDirection.Input);
        parameters.Add("Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var rowsAffected = await _sqlDataAccess.ExecuteProcedure("spTicket_Create", parameters);

        if (rowsAffected == 0)
        {
            // TODO
            throw new Exception($"Error inserting ticket {ticket}");
        }

        var id = parameters.Get<int>("Id");
        return id;
    }

    public async Task<Ticket?> GetByDeskproTicketId(int deskproTicketId) => await _sqlDataAccess.QuerySingle<Ticket>("SELECT * FROM v_Tickets WHERE DeskproId = @DeskproId", new { DeskproId = deskproTicketId });

    public async Task<Ticket?> Get(int id) => await _sqlDataAccess.QuerySingle<Ticket>("SELECT * FROM v_Tickets WHERE Id = @Id", new { Id = id });

    public async Task<int> Update(Ticket ticket)
    {
        var sql = @"UPDATE Tickets SET
				CaseNumber = @CaseNumber,
				SharepointFolderName = @SharepointFolderName,
				JournalizedAt = @JournalizedAt,
				TicketClosedAt = @TicketClosedAt,
				CaseUrl = @CaseUrl
			WHERE Id = @Id";

        return await _sqlDataAccess.Execute(sql, ticket);
    }

    public async Task<IEnumerable<Ticket>> GetAll(int? deskproId, long? podioItemId, Guid? filArkivCaseId, bool includeClosedTickets = true)
    {
        // Prepare filter
        var filter = new List<string>();

        if (deskproId != null)
        {
            filter.Add($"Tickets.DeskproId = {deskproId}");
        }

        if (!includeClosedTickets)
        {
            filter.Add($"Tickets.TicketClosedAt IS NULL");
        }

        if (podioItemId != null)
        {
            filter.Add($"Cases.PodioItemId = '{podioItemId}'");
        }

        if (filArkivCaseId != null)
        {
            filter.Add($"Cases.FilArkivCaseId = '{filArkivCaseId}'");
        }

        var filterString = string.Join(" AND ", filter);

        var getTicketIdsSql = $"SELECT Tickets.Id FROM Tickets LEFT JOIN Cases ON Tickets.Id = Cases.TicketId";

        if (!string.IsNullOrEmpty(filterString))
        {
            getTicketIdsSql += " WHERE " + filterString;
        }

        var ticketIds = await _sqlDataAccess.Query<int>(getTicketIdsSql, null);

        if (ticketIds != null && ticketIds.Count() > 0)
        {
            var ticketsSql = $"SELECT Tickets.* FROM Tickets WHERE Tickets.Id IN ({string.Join(",", ticketIds)})";
            var tickets = await _sqlDataAccess.Query<Ticket>(ticketsSql, null);

            foreach (var ticket in tickets)
            {
                var caseSql = $"SELECT Cases.* FROM Cases WHERE Cases.TicketId = @TicketId";
                var cases = await _sqlDataAccess.Query<Case>(caseSql, new { TicketId = ticket.Id });

                ticket.Cases = cases.ToList();
            }
            // Return
            return tickets;
        }

        return Enumerable.Empty<Ticket>();
    }

    public Task<Ticket?> GetByPodioItemId(long podioItemId)
    {
        throw new NotImplementedException();
    }
}
