using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using System.Data;

namespace AktBob.Database.Repositories;
internal class CaseRepository : ICaseRepository
{
    private readonly ISqlDataAccess _sqlDataAccess;

    public CaseRepository(ISqlDataAccess sqlDataAccess)
    {
        _sqlDataAccess = sqlDataAccess;
    }

    public async Task<int> Add(Case @case)
    {
        var parameters = new DynamicParameters();
        parameters.Add("TicketId", @case.TicketId);
        parameters.Add("PodioItemId", @case.PodioItemId);
        parameters.Add("FilArkivCaseId", @case.FilArkivCaseId);
        parameters.Add("CaseNumber", @case.CaseNumber);
        parameters.Add("Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var rowsAffected = await _sqlDataAccess.ExecuteProcedure("spCase_Create", parameters);
        var caseId = parameters.Get<int>("Id");

        if (rowsAffected == 0)
        {
            // TODO
            throw new Exception($"Error inserting case {@case}");
        }

        var id = parameters.Get<int>("Id");
        return id;
    }

    public async Task<Case?> Get(int id) => await _sqlDataAccess.QuerySingle<Case>("SELECT * FROM v_Cases WHERE Id = @Id", new { Id = id });

    public async Task<IEnumerable<Case>> GetAll(int? deskproId, long? podioItemId, Guid? filArkivCaseId)
    {
        var filter = new List<string>();

        if (deskproId != null)
        {
            filter.Add($"v_Tickets.DeskproId = {deskproId}");
        }

        if (podioItemId != null)
        {
            filter.Add($"v_Cases.PodioItemId = {podioItemId}");
        }

        if (filArkivCaseId != null)
        {
            filter.Add($"v_Cases.FilArkivCaseId = '{filArkivCaseId}'");
        }

        var filterString = string.Join(" AND ", filter);
        var getTicketIdsSql = @$"SELECT v_Cases.* FROM v_Cases LEFT JOIN v_Tickets ON v_Cases.TicketId = v_Tickets.Id";

        if (filterString.Length != 0)
        {
            getTicketIdsSql += $" WHERE {filterString}";
        }

        return await _sqlDataAccess.Query<Case>(getTicketIdsSql, null);
    }

    public Task<Case?> GetByPodioItemId(long podioItemId)
    {
        throw new NotImplementedException();
    }

    public async Task<Case?> GetByTicketId(int ticketId) => await _sqlDataAccess.QuerySingle<Case>("SELECT * FROM v_Cases WHERE TicketId = @TicketId", new { TicketId = ticketId });

    public async Task<int> Update(Case @case)
    {
        var sql = @"UPDATE Cases SET
				TicketId = @TicketId,
				PodioItemId = @PodioItemId,
				CaseNumber = @CaseNumber,
				FilArkivCaseId = @FilArkivCaseId,
				SharepointFolderName = @SharepointFolderName
			WHERE Id = @Id";

        return await _sqlDataAccess.Execute(sql, @case);
    }
}
