using AktBob.Database.Contracts;
using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Entities;
using AktBob.Database.Extensions;
using Ardalis.GuardClauses;
using Ardalis.Result;
using Dapper;
using MassTransit.Mediator;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace AktBob.Database.UseCases.Messages;
public class GetMessagesQueryHandler(IConfiguration configuration) : MediatorRequestHandler<GetMessagesQuery, Result<IEnumerable<MessageDto>>>
{
    private readonly IConfiguration _configuration = configuration;

    protected override async Task<Result<IEnumerable<MessageDto>>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("Database"));

        using (var connection = new SqlConnection(connectionString))
        {
            var storedProcedure = request.IncludeJournalized ? Constants.SP_MESSAGE_GET_ALL : Constants.SP_MESSAGE_GET_ALL_NOT_JOURNALIZED;
            var rows = await connection.QueryAsync<Message>(storedProcedure, commandType: CommandType.StoredProcedure);

            return Result.Success(rows.Select(x => x.ToDto()));
        }
    }
}
