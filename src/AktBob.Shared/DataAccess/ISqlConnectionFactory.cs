using System.Data;

namespace AktBob.Shared.DataAccess;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
