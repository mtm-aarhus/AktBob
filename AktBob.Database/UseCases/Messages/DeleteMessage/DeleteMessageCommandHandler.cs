using Ardalis.GuardClauses;
using Dapper;
using MassTransit.Mediator;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AktBob.Database.UseCases.Messages.DeleteMessage;
internal class DeleteMessageCommandHandler(IConfiguration configuration) : MediatorRequestHandler<DeleteMessageCommand>
{
    private readonly IConfiguration _configuration = configuration;

    protected override async Task Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("Database"));

        using (var connection = new SqlConnection(connectionString))
        {
            var getMessageParameters = new DynamicParameters();
            getMessageParameters.Add(Constants.T_MESSAGES_ID, request.Id);

            await connection.QueryAsync(Constants.SP_MESSAGE_DELETE, getMessageParameters, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
