//using System.Data;
//using AktBob.Shared.DataAccess;
//using Dapper;

//namespace AktBob.Shared;
//public class DapperSqlExecutor<TConnection> : ISqlExecutor<TConnection> where TConnection : ISqlConnectionFactory
//{
//    private readonly TConnection _sqlConnectionFactory;

//    public DapperSqlExecutor(TConnection connection)
//    {
//        _sqlConnectionFactory = connection;
//    }

//    public Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null, CommandType? commandType = null)
//    {
//        using var connection = _sqlConnectionFactory.CreateConnection();
//        return connection.ExecuteAsync(sql, param, transaction, timeout, commandType);
//    }
//}
