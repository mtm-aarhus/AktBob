using AktBob.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace AktBob.OpenOrchestrator;

internal class OpenOrchestratorSqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    private readonly string _connectionString = configuration.GetConnectionString("OpenOrchestratorDb")!;
    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
