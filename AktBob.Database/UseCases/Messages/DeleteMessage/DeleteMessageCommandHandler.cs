using Ardalis.GuardClauses;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AktBob.Database.UseCases.Messages.DeleteMessage;
internal class DeleteMessageCommandHandler : IRequestHandler<DeleteMessageCommand>
{
    private readonly IConfiguration _configuration;

    public DeleteMessageCommandHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

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
