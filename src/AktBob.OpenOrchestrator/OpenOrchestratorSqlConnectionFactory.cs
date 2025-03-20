using AktBob.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace AktBob.OpenOrchestrator;

internal class OpenOrchestratorSqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public OpenOrchestratorSqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("OpenOrchestratorDb")!;
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
