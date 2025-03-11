using AktBob.Database.Contracts;
using AktBob.Database.DataAccess;
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

    public async Task<bool> Add(Case @case)
    {
        var parameters = new DynamicParameters();
        parameters.Add("TicketId", @case.TicketId);
        parameters.Add("PodioItemId", @case.PodioItemId);
        parameters.Add("FilArkivCaseId", @case.FilArkivCaseId);
        parameters.Add("CaseNumber", @case.CaseNumber);
        parameters.Add("Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var rowsAffected = await _sqlDataAccess.ExecuteProcedure("spCase_Create", parameters);
        @case.Id = parameters.Get<int?>("Id") ?? 0;
        return rowsAffected == 1;
    }

    public async Task<Case?> Get(int id) => await _sqlDataAccess.QuerySingle<Case>("SELECT * FROM v_Cases WHERE Id = @Id", new { Id = id });

    public async Task<IEnumerable<Case>> GetAll(long? podioItemId, Guid? filArkivCaseId)
    {
        var filter = new List<string>();

        if (podioItemId != null)
        {
            filter.Add($"v_Cases.PodioItemId = {podioItemId}");
        }

        if (filArkivCaseId != null)
        {
            filter.Add($"v_Cases.FilArkivCaseId = '{filArkivCaseId}'");
        }

        var filterString = string.Join(" AND ", filter);
        var sql = @$"SELECT v_Cases.* FROM v_Cases ";

        if (filterString.Length != 0)
        {
            sql += $" WHERE {filterString}";
        }

        return await _sqlDataAccess.Query<Case>(sql, null);
    }

    public async Task<Case?> GetByPodioItemId(long podioItemId) => await _sqlDataAccess.QuerySingle<Case>("SELECT * FROM v_Cases WHERE PodioItemId = @PodioItemId", new { PodioItemId = podioItemId });

    public async Task<Case?> GetByTicketId(int ticketId) => await _sqlDataAccess.QuerySingle<Case>("SELECT * FROM v_Cases WHERE TicketId = @TicketId", new { TicketId = ticketId });

    public async Task<bool> Update(Case @case)
    {
        var sql = """
            UPDATE Cases SET
                TicketId = @TicketId,
                PodioItemId = @PodioItemId,
                CaseNumber = @CaseNumber,
                FilArkivCaseId = @FilArkivCaseId,
                SharepointFolderName = @SharepointFolderName
            WHERE Id = @Id
            """;

        return await _sqlDataAccess.Execute(sql, @case) == 1;
    }
}
