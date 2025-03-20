using System.Data;

namespace AktBob.Shared;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
