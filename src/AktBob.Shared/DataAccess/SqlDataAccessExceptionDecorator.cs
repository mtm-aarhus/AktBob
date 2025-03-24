using Dapper;
using Microsoft.Extensions.Logging;

namespace AktBob.Shared.DataAccess;

public class SqlDataAccessExceptionDecorator<TConnection> : ISqlDataAccess<TConnection> where TConnection : ISqlConnectionFactory
{
    private readonly ISqlDataAccess<TConnection> _inner;
    private readonly ILogger<SqlDataAccess<TConnection>> _logger;

    public SqlDataAccessExceptionDecorator(ISqlDataAccess<TConnection> inner, ILogger<SqlDataAccess<TConnection>> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<int> Execute<T>(string sql, T? parameters)
    {
        try
        {
            return await _inner.Execute(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(Execute));
            throw;
        }
    }

    public async Task<int> ExecuteProcedure(string procedureName, DynamicParameters? parameters)
    {
        try
        {
            return await _inner.ExecuteProcedure(procedureName, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(ExecuteProcedure));
            throw;
        }
    }

    public async Task<IReadOnlyCollection<T>> Query<T>(string sql, object? parameters)
    {
        try
        {
            return await _inner.Query<T>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(Query));
            throw;
        }
    }

    public async Task<IReadOnlyCollection<T>> Query<T, U>(string sql, object parameters, string splitOn, Func<T, U, T> map)
    {
        try
        {
            return await _inner.Query(sql, parameters, splitOn, map);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(Query));
            throw;
        }
    }

    public async Task<T?> QuerySingle<T>(string sql, object? parameters)
    {
        try
        {
            return await _inner.QuerySingle<T>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(QuerySingle));
            throw;
        }
    }
}
