﻿using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace AktBob.Database;

internal class DatabaseSqlConnectionFactory(IConfiguration configuration) : IDatabaseSqlConnectionFactory
{
    private readonly string _connectionString = configuration.GetConnectionString("Database")!;
    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
