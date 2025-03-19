using Ardalis.GuardClauses;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace AktBob.Database.DataAccess;

internal class SqlDataAccess : ISqlDataAccess
{
    private readonly string _connectionString;

    public SqlDataAccess(IConfiguration configuration)
    {
        _connectionString = Guard.Against.NullOrEmpty(configuration.GetConnectionString("Database"));
    }

    public async Task<T?> QuerySingle<T>(string sql, object? parameters)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters, commandType: CommandType.Text);
        }
    }

    public async Task<IReadOnlyCollection<T>> Query<T>(string sql, object? parameters)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var result = await connection.QueryAsync<T>(sql, parameters, commandType: CommandType.Text);
            return result.AsList();
        }
    }

    public async Task<IReadOnlyCollection<T>> Query<T, U>(string sql, object parameters, string splitOn, Func<T, U, T> map)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var result = await connection.QueryAsync(sql: sql, map: map, param: parameters, splitOn: splitOn, commandType: CommandType.Text);
            return result.AsList();
        }
    }

    public async Task<int> Execute<T>(string sql, T? parameters)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            return await connection.ExecuteAsync(sql, parameters, commandType: CommandType.Text);
        }
    }

    public async Task<int> ExecuteProcedure(string procedureName, DynamicParameters? parameters)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            return await connection.ExecuteAsync(procedureName, parameters, commandType: CommandType.StoredProcedure);
        }
    }
}