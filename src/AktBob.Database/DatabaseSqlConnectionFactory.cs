using AktBob.Shared;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AktBob.Database;

internal class DatabaseSqlConnectionFactory(IAppConfig appConfig) : IDatabaseSqlConnectionFactory
{
    private readonly string _connectionString = appConfig.GetConnectionString("Database")!;
    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
