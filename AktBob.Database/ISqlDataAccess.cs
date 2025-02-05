using Ardalis.Result;
using Dapper;

namespace AktBob.Database;
internal interface ISqlDataAccess
{
    Task<Result<IEnumerable<T>>> ExecuteProcedure<T>(string procedureName, DynamicParameters parameters);
    Task<Result> ExecuteProcedure(string procedureName, DynamicParameters parameters);
}