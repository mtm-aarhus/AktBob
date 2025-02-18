using AktBob.Database.Contracts.Messages;
using Ardalis.GuardClauses;
using Ardalis.Result;
using Dapper;
using MassTransit.Mediator;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace AktBob.Database.UseCases.Messages;
public class AddMessagesCommandHandler(IConfiguration configuration) : MediatorRequestHandler<AddMessageCommand, Result<int>>
{
    private readonly IConfiguration _configuration = configuration;

    protected override async Task<Result<int>> Handle(AddMessageCommand command, CancellationToken cancellationToken)
    {
        var connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("Database"));

        using (var connection = new SqlConnection(connectionString))
        {
            // The stored procedure prevents from persisting duplicates, so we don't need to check this before called the database
            var parameters = new DynamicParameters();
            parameters.Add(Constants.T_MESSAGES_TICKET_ID, command.TicketId);
            parameters.Add(Constants.T_MESSAGES_DESKPRO_ID, command.DeskproMessageId);
            parameters.Add(Constants.T_MESSAGES_ID, dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.QueryAsync(Constants.SP_MESSAGE_CREATE, parameters, commandType: CommandType.StoredProcedure);

            var id = parameters.Get<int>(Constants.T_MESSAGES_ID);
            return id;
        }
    }
}
