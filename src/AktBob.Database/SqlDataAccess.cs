using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace AktBob.Database;

internal class SqlDataAccess : ISqlDataAccess
{
    private readonly IConfiguration _configuration;

    public SqlDataAccess(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string GetConnectionString() => _configuration.GetConnectionString("Database")!;

    public async Task<T?> QuerySingle<T>(string sql, object? parameters)
    {
        using (var connection = new SqlConnection(GetConnectionString()))
        {
            var entity = await connection.QuerySingleOrDefaultAsync<T>(sql, parameters, commandType: CommandType.Text);
            return entity;
        }
    }

    public async Task<IEnumerable<T>> Query<T>(string sql, object? parameters)
    {
        using (var connection = new SqlConnection(GetConnectionString()))
        {
            return await connection.QueryAsync<T>(sql, parameters, commandType: CommandType.Text);
        }
    }

    public async Task<IEnumerable<T>> Query<T, U>(string sql, object parameters, string splitOn, Func<T, U, T> map)
    {
        using (var connection = new SqlConnection(GetConnectionString()))
        {
            return await connection.QueryAsync(
                sql: sql,
                map: map,
                param: parameters,
                splitOn: splitOn,
                commandType: CommandType.Text);
        }
    }


    public async Task<int> ExecuteProcedure(string procedureName, DynamicParameters? parameters)
    {
        var connectionString = GetConnectionString();

        using (var connection = new SqlConnection(connectionString))
        {
            return await connection.ExecuteAsync(procedureName, parameters, commandType: CommandType.StoredProcedure);
        }
    }

    public async Task<int> Execute<T>(string sql, T? parameters)
    {
        var connectionString = GetConnectionString();

        using (var connection = new SqlConnection(connectionString))
        {
            return await connection.ExecuteAsync(sql, parameters, commandType: CommandType.Text);
        }
    }
}