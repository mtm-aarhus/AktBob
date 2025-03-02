using System.Data;

namespace AktBob.Database;

internal class SqlDataAccess : ISqlDataAccess
{
    private readonly IDbConnection _connection;

    public SqlDataAccess(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<T?> QuerySingle<T>(string sql, object? parameters) => await _connection.QuerySingleOrDefaultAsync<T>(sql, parameters, commandType: CommandType.Text);
    public async Task<IEnumerable<T>> Query<T>(string sql, object? parameters) => await _connection.QueryAsync<T>(sql, parameters, commandType: CommandType.Text);
    public async Task<IEnumerable<T>> Query<T, U>(string sql, object parameters, string splitOn, Func<T, U, T> map) => await _connection.QueryAsync(sql: sql, map: map, param: parameters, splitOn: splitOn, commandType: CommandType.Text);
    public async Task<int> Execute<T>(string sql, T? parameters) => await _connection.ExecuteAsync(sql, parameters, commandType: CommandType.Text);
    public async Task<int> ExecuteProcedure(string procedureName, DynamicParameters? parameters) => await _connection.ExecuteAsync(procedureName, parameters, commandType: CommandType.StoredProcedure);
}