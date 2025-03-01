using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
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

    public async Task<Ticket?> Get(int id)
    {
        var where = "t.Id = @Id";
        var tickets = await GetTicketsWithCases(where, new { Id = id });
        return tickets.FirstOrDefault();
    }

    public async Task<Ticket?> GetByDeskproTicketId(int deskproTicketId)
    {
        var where = "t.DeskproId = @DeskproId";
        var tickets = await GetTicketsWithCases(where, new { DeskproId = deskproTicketId });
        return tickets.FirstOrDefault();
    }

    public async Task<Ticket?> GetByPodioItemId(long podioItemId)
    {
        var where = "c.PodioItemId = @PodioItemId";
        var tickets = await GetTicketsWithCases(where, new { PodioItemId = podioItemId });
        return tickets.FirstOrDefault();
    }

    public async Task<int> Update(Ticket ticket)
    {
        var sql = @"UPDATE Tickets 
            SET
				CaseNumber = @CaseNumber,
				SharepointFolderName = @SharepointFolderName,
				CaseUrl = @CaseUrl
			WHERE Id = @Id";

        return await _sqlDataAccess.Execute(sql, ticket);
    }

    //public async Task<IEnumerable<Ticket>> GetAll(int? deskproId, long? podioItemId, Guid? filArkivCaseId)
    //{
    //    // Prepare filter
    //    var filter = new List<string>();

    //    if (deskproId != null)
    //    {
    //        filter.Add("t.DeskproId = @DeskproId");
    //    }

    //    if (podioItemId != null)
    //    {
    //        filter.Add("c.PodioItemId = @PodioItemId");
    //    } 

    //    if (filArkivCaseId != null)
    //    {
    //        filter.Add("c.FilArkivCaseId = @FilArkivCaseId");
    //    }

    //    var filterString = string.Join(" AND ", filter);

    //    if (!string.IsNullOrEmpty(filterString))
    //    {
    //        return await GetTicketsWithCases(filterString, new 
    //        {
    //            DeskproId = deskproId,
    //            PodioItemId = podioItemId,
    //            FilArkivCaseId = filArkivCaseId
    //        });
    //    }

    //    return await GetTicketsWithCases(string.Empty, new {});
    //}

    private async Task<IEnumerable<Ticket>> GetTicketsWithCases(string where, object parameters)
    {
        var sql = @$"
                SELECT 

                    t.Id
	                ,t.DeskproId
	                ,t.CaseNumber
	                ,t.CaseUrl
	                ,t.SharepointFolderName

                    ,c.TicketId
                    ,c.Id
                    ,c.PodioItemId
                    ,c.CaseNumber
                    ,c.FilArkivCaseId
                    ,c.SharepointFolderName

                FROM v_Tickets t
                LEFT JOIN v_Cases c ON t.Id = c.TicketId";

        if (!string.IsNullOrEmpty(where))
        {
            sql += $" WHERE {where}";
        }

        var ticketDictionary = new Dictionary<int, Ticket>();
        var tickets = await _sqlDataAccess.Query<Ticket, Case>(sql, parameters, "TicketId", (ticket, @case) =>
        {
            if (!ticketDictionary.TryGetValue(ticket.Id, out var existingTicket))
            {
                existingTicket = ticket;
                existingTicket.Cases = new List<Case>();
                ticketDictionary.Add(ticket.Id, existingTicket);
            }

            if (@case != null)
            {
                existingTicket.Cases.Add(@case);
            }

            return existingTicket;
        });

        return ticketDictionary.Values;
    }
}
