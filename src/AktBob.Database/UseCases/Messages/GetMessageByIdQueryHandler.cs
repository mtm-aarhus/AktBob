using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Entities;
using AktBob.Database.Extensions;
using Ardalis.GuardClauses;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AktBob.Database.UseCases.Messages;

internal record GetMessageByDeskproMessageIdQuery(int Id) : IRequest<Result<MessageDto>>;
internal class GetMessageByIdQueryHandler(IConfiguration configuration) : IRequestHandler<GetMessageByDeskproMessageIdQuery, Result<MessageDto>>
{
    private readonly IConfiguration _configuration = configuration;

    public async Task<Result<MessageDto>> Handle(GetMessageByDeskproMessageIdQuery request, CancellationToken cancellationToken)
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

            return Result.Success(messages.First().ToDto());
        }
    }
}
