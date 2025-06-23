using System.Data;
using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Shared.DataAccess;

namespace AktBob.Database.Repositories;

internal class OcrScreeningStatusRepository(ISqlDataAccess<IDatabaseSqlConnectionFactory> sqlDataAccess)
    : IOcrScreeningStatusRepository
{
    private readonly ISqlDataAccess<IDatabaseSqlConnectionFactory> _sqlDataAccess = sqlDataAccess;

    public async Task<bool> Add(OcrScreeningStatus ocrScreeningStatus)
    {
        var parameters = new DynamicParameters();
        parameters.Add("PodioItemId", ocrScreeningStatus.PodioItemId, dbType: DbType.Int64, direction: ParameterDirection.Input);
        parameters.Add("FilArkivCaseId", ocrScreeningStatus.FilArkivCaseId, dbType: DbType.Guid, direction: ParameterDirection.Input);
        parameters.Add("FilArkivFileId", ocrScreeningStatus.FilArkivFileId, dbType: DbType.Guid, direction: ParameterDirection.Input);

        var rowsAffected = await _sqlDataAccess.ExecuteProcedure("spOCRScreeningStatus_Create", parameters);
        return rowsAffected == 1;
    }

    public async Task<OcrScreeningStatus?> Get(Guid filArkivFileId)
    {
        const string sql = "SELECT * FROM OcrScreeningStatus WHERE FilArkivFileId = @FilArkivFileId";
        return await _sqlDataAccess.QuerySingle<OcrScreeningStatus>(sql, new { FilArkivFileId = filArkivFileId });
    }

    public async Task<bool> AllFilesAreProcessed(Guid filarkivCaseId)
    {
        const string sql = "SELECT * FROM OcrScreeningStatus WHERE FilArkivCaseId = @FilArkivCaseId AND ProcessedAt IS NULL";
        var items = await _sqlDataAccess.Query<OcrScreeningStatus>(sql, new { FilArkivCaseId = filarkivCaseId });
        return items.Count == 0;
    }

    public async Task<bool> AnyByCaseId(Guid filarkivCaseId)
    {
        const string sql = "SELECT COUNT(*) FROM OcrScreeningStatus WHERE FilArkivCaseId = @FilArkivCaseId";
        var count = await _sqlDataAccess.QuerySingle<int>(sql, new { FilArkivCaseId = filarkivCaseId });
        return count != 0;
    }
    
    public async Task RemoveByCaseId(Guid filArkivCaseId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("FilArkivCaseId",  filArkivCaseId, dbType: DbType.Guid, direction: ParameterDirection.Input);
        await _sqlDataAccess.ExecuteProcedure("spOCRScreeningStatus_RemoveByCaseId", parameters);
    }
    
    public async Task<bool> Update(OcrScreeningStatus ocrScreeningStatus)
    {
        const string sql = """
                           UPDATE OcrScreeningStatus
                           SET ProcessedAt = @ProcessedAt
                           WHERE FilArkivFileId = @FilArkivFileId
                           """;
        var rowsAffected = await _sqlDataAccess.Execute(sql, ocrScreeningStatus);
        return rowsAffected == 1;
    }
}