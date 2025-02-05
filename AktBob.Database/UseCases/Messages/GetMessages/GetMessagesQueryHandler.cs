using AktBob.Database.Entities;
using Ardalis.GuardClauses;
using Ardalis.Result;
using Dapper;
using MassTransit.Mediator;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace AktBob.Database.UseCases.Messages.GetMessages;
internal class GetMessagesQueryHandler(IConfiguration configuration) : MediatorRequestHandler<GetMessagesQuery, Result<IEnumerable<Message>>>
{
    private readonly IConfiguration _configuration = configuration;

    protected override async Task<Result<IEnumerable<Message>>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("Database"));

        using (var connection = new SqlConnection(connectionString))
        {
            var storedProcedure = request.IncludeJournalized ? Constants.SP_MESSAGE_GET_ALL : Constants.SP_MESSAGE_GET_ALL_NOT_JOURNALIZED;
            var rows = await connection.QueryAsync<Message>(storedProcedure, commandType: CommandType.StoredProcedure);

            return Result.Success(rows);
        }
    }
}
