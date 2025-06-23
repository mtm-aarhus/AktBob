using System.Data;
using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Shared.DataAccess;

namespace AktBob.Database.Repositories;

internal class OcrScreeningStatusRepository(ISqlDataAccess<IDatabaseSqlConnectionFactory> sqlDataAccess) : IOcrScreeningStatusRepository
{
    private readonly ISqlDataAccess<IDatabaseSqlConnectionFactory> _sqlDataAccess = sqlDataAccess;

    public async Task<bool> Add(OcrScreeningStatus ocrScreeningStatus)
    {
        var parameters = new DynamicParameters();
        parameters.Add("PodioItemId", ocrScreeningStatus.PodioItemId, dbType: DbType.Int64, direction: ParameterDirection.Input);
        parameters.Add("FilArkivCaseId", ocrScreeningStatus.PodioItemId, dbType: DbType.Guid, direction: ParameterDirection.Input);
        parameters.Add("FilArkivFileId", ocrScreeningStatus.PodioItemId, dbType: DbType.Guid, direction: ParameterDirection.Input);
        
        var rowsAffected = await _sqlDataAccess.ExecuteProcedure("spOCRScreeningStatus_Create", parameters);
        return rowsAffected == 1;
    }

    public Task<bool> Update(OcrScreeningStatus ocrScreeningStatus)
    {
        throw new NotImplementedException();
    }
}