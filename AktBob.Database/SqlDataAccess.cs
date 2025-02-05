using Ardalis.Result;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace AktBob.Database;

internal class SqlDataAccess : ISqlDataAccess
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SqlDataAccess> _logger;

    public SqlDataAccess(IConfiguration configuration, ILogger<SqlDataAccess> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private string GetConnectionString() => _configuration.GetConnectionString("Database")!;

    public async Task<Result<IEnumerable<T>>> ExecuteProcedure<T>(string procedureName, DynamicParameters parameters)
    {
        try
        {
            var connectionString = GetConnectionString();

            using (var connection = new SqlConnection(connectionString))
            {
                var rows = await connection.QueryAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
                
                if (rows.Any())
                {
                    return Result.Success(rows);
                }

                return Result.NotFound();
            }
        }
        catch (SqlException ex)
        {
            _logger.LogCritical("Error executing stored procedure {procedureName}. {message}", procedureName, ex.Message);
            return Result.CriticalError();
        }
    }

    public async Task<Result> ExecuteProcedure(string procedureName, DynamicParameters parameters)
    {
        try
        {
            var connectionString = GetConnectionString();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.QueryAsync(procedureName, parameters, commandType: CommandType.StoredProcedure);
                return Result.Success();
            }
        }
        catch (SqlException ex)
        {
            _logger.LogCritical("Error executing stored procedure {procedureName}. {message}", procedureName, ex.Message);
            return Result.CriticalError();
        }
    }
}