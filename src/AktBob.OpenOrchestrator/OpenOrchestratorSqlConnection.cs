using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace AktBob.OpenOrchestrator;

internal class OpenOrchestratorSqlConnection(IConfiguration configuration) : IOpenOrchestratorSqlConnection
{
    private readonly string _connectionString = configuration.GetConnectionString("OpenOrchestratorDb")!;
    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
