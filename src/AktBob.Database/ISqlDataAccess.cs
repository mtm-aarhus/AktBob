
namespace AktBob.Database;
public interface ISqlDataAccess
{
    /// <summary>
    /// Query database returning a single entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns>Entity of T</returns>
    Task<T?> QuerySingle<T>(string sql, object? parameters);

    /// <summary>
    /// Query database returning number of rows affected
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns>Number of rows affected</returns>
    Task<int> Execute<T>(string sql, T? parameters);

    /// <summary>
    /// Executes a stored procedure returning number of rows affected
    /// Value of output parameter can be fetched from the DynamicParameters
    /// </summary>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    /// <returns>Number of rows affected</returns>
    Task<int> ExecuteProcedure(string procedureName, DynamicParameters? parameters);

    /// <summary>
    /// Query database returning enumerable of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns>Enumerable of T</returns>
    Task<IEnumerable<T>> Query<T>(string sql, object? parameters);
    Task<IEnumerable<T>> Query<T, U>(string sql, object parameters, string splitOn, Func<T, U, T> map);
}