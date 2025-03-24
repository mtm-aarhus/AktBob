using Dapper;
using System.Data;

namespace AktBob.Shared.DataAccess;

public class SqlDataAccess<TConnection> : ISqlDataAccess<TConnection> where TConnection : ISqlConnectionFactory
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public SqlDataAccess(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }
    public async Task<T?> QuerySingle<T>(string sql, object? parameters)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters, commandType: CommandType.Text);
    }

    public async Task<IReadOnlyCollection<T>> Query<T>(string sql, object? parameters)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<T>(sql, parameters, commandType: CommandType.Text);
        return result.AsList();
    }

    public async Task<IReadOnlyCollection<T>> Query<T, U>(string sql, object parameters, string splitOn, Func<T, U, T> map)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync(sql: sql, map: map, param: parameters, splitOn: splitOn, commandType: CommandType.Text);
        return result.AsList();
    }

    public async Task<int> Execute<T>(string sql, T? parameters)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, parameters, commandType: CommandType.Text);
    }

    public async Task<int> ExecuteProcedure(string procedureName, DynamicParameters? parameters)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        return await connection.ExecuteAsync(procedureName, parameters, commandType: CommandType.StoredProcedure);
    }
}