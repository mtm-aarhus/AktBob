using AktBob.Database.Entities;
using Ardalis.GuardClauses;
using Ardalis.Result;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AktBob.Database.UseCases.Messages.GetMessageById;
internal class GetMessageByIdQueryHandler : IRequestHandler<GetMessageByIdQuery, Result<Message>>
{
    private readonly IConfiguration _configuration;

    public GetMessageByIdQueryHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Result<Message>> Handle(GetMessageByIdQuery request, CancellationToken cancellationToken)
    {
        var connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("Database"));

        using (var connection = new SqlConnection(connectionString))
        {
            var getMessageParameters = new DynamicParameters();
            getMessageParameters.Add(Constants.T_MESSAGES_ID, request.Id);

            var messages = await connection.QueryAsync<Message>(Constants.SP_MESSAGE_GET_BY_ID, getMessageParameters, commandType: System.Data.CommandType.StoredProcedure);

            if (messages is null || messages.Count() == 0)
            {
                return Result.NotFound();
            }

            if (messages.Count() > 1)
            {
                return Result.Error();
            }

            return Result.Success(messages.First());
        }
    }
}
