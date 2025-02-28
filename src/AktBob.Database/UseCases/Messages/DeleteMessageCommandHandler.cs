using AktBob.Database.Contracts.Messages;
using Ardalis.GuardClauses;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AktBob.Database.UseCases.Messages;
internal class DeleteMessageCommandHandler(IConfiguration configuration) : IRequestHandler<DeleteMessageCommand>
{
    private readonly IConfiguration _configuration = configuration;

    public async Task Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
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
