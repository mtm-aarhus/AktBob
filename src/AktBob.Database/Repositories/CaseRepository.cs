﻿using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Database.Validators;
using AktBob.Shared.DataAccess;
using FluentValidation;
using System.Data;

namespace AktBob.Database.Repositories;
internal class CaseRepository : ICaseRepository
{
    private readonly ISqlDataAccess<IDatabaseSqlConnectionFactory> _sqlDataAccess;

    public CaseRepository(ISqlDataAccess<IDatabaseSqlConnectionFactory> sqlDataAccess)
    {
        _sqlDataAccess = sqlDataAccess;
    }

    public async Task<bool> Add(Case @case)
    {
        var valiator = new CaseValidator();
        valiator.ValidateAndThrow(@case);

        var parameters = new DynamicParameters();
        parameters.Add("TicketId", @case.TicketId);
        parameters.Add("PodioItemId", @case.PodioItemId);
        parameters.Add("FilArkivCaseId", @case.FilArkivCaseId);
        parameters.Add("CaseNumber", @case.CaseNumber);
        parameters.Add("Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var rowsAffected = await _sqlDataAccess.ExecuteProcedure("spCase_Create", parameters);
        @case.Id = parameters.Get<int?>("Id") ?? default;
        return rowsAffected == 1;
    }

    public async Task<Case?> Get(int id) => await _sqlDataAccess.QuerySingle<Case>("SELECT * FROM v_Cases WHERE Id = @Id", new { Id = id });

    public async Task<IReadOnlyCollection<Case>> GetAll(long? podioItemId, Guid? filArkivCaseId)
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

        var result = await _sqlDataAccess.Query<Case>(sql, null);
        return result.ToList();
    }

    public async Task<bool> Update(Case @case)
    {
        var valiator = new CaseValidator();
        valiator.ValidateAndThrow(@case);

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
