using AktBob.Database.Contracts;
using AktBob.Shared.DataAccess;

namespace AktBob.Database.Repositories;
internal class FilArkivCleanUpQueueRepository : IFilArkivFilesCleanUpQueueRepository
{
    private readonly ISqlDataAccess<IDatabaseSqlConnectionFactory> _sqlDataAccess;

    public FilArkivCleanUpQueueRepository(ISqlDataAccess<IDatabaseSqlConnectionFactory> sqlDataAccess)
    {
        _sqlDataAccess = sqlDataAccess;
    }


    public async Task Add(Guid filArkivFileId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("FilArkivFileId", filArkivFileId, dbType: System.Data.DbType.Guid);
        await _sqlDataAccess.ExecuteProcedure("spFilArkivFilesCleanUpQueue_Create", parameters);
    }
}
