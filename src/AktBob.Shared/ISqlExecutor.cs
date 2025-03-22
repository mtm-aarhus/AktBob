using System.Data;

namespace AktBob.Shared;
public interface ISqlExecutor<TConnection> where TConnection : ISqlConnectionFactory
{
    Task<int> ExecuteAsync(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null);
}