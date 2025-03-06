using Microsoft.Extensions.Logging;

namespace AktBob.Database.DataAccess;

internal class SqlDataAccessLoggingDecorator(ISqlDataAccess inner, ILogger<SqlDataAccessLoggingDecorator> logger) : ISqlDataAccess
{
    private readonly ISqlDataAccess _inner = inner;
    private readonly ILogger<SqlDataAccessLoggingDecorator> _logger = logger;

    public async Task<int> Execute<T>(string sql, T? parameters)
    {
        _logger.LogDebug("Executing {sql} with {parameters}", sql, parameters);

        var rowsAffected = await _inner.Execute(sql, parameters);
        if (rowsAffected == 0)
        {
            _logger.LogDebug("No rows were affected when executing {sql} with {parameters}", sql, parameters);
        }

        return rowsAffected;
    }

    public async Task<int> ExecuteProcedure(string procedureName, DynamicParameters? parameters)
    {
        _logger.LogDebug("Executing procedure {procedureName} with {parameters}", procedureName, parameters);

        var rowsAffected = await _inner.ExecuteProcedure(procedureName, parameters);
        if (rowsAffected == 0)
        {
            _logger.LogDebug("No rows were affected when executing stored {procedureName} with {parameters}", procedureName, parameters);
        }

        return rowsAffected;
    }

    public async Task<IEnumerable<T>> Query<T>(string sql, object? parameters)
    {
        _logger.LogDebug("Querying {sql} with {parameters}", sql, parameters);

        var result = await _inner.Query<T>(sql, parameters);
        if (!result.Any())
        {
            _logger.LogDebug("Empty result when querying {sql} with {parameters}", sql, parameters);
        }

        return result;
    }

    public async Task<IEnumerable<T>> Query<T, U>(string sql, object parameters, string splitOn, Func<T, U, T> map)
    {
        _logger.LogDebug("Querying {sql} with {parameters} splitting on {splitOn}", sql, parameters, splitOn);

        var result = await _inner.Query(sql, parameters, splitOn, map);
        if (!result.Any())
        {
            _logger.LogDebug("Empty result when querying {sql} with {parameters} splitting on {splitOn}", sql, parameters, splitOn);
        }

        return result;
    }

    public async Task<T?> QuerySingle<T>(string sql, object? parameters)
    {
        _logger.LogDebug("Querying single row by {sql} with {parameters}", sql, parameters);
        
        var result = await _inner.QuerySingle<T>(sql, parameters);
        if (result is null)
        {
            _logger.LogDebug("Nothing found querying single row by {sql} with {parameters}", sql, parameters);
        }

        return result;

    }
}
