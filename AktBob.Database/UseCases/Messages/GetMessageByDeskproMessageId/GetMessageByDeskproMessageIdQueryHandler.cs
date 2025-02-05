using AktBob.Database.Entities;
using Ardalis.GuardClauses;
using Ardalis.Result;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace AktBob.Database.UseCases.Messages.GetMessageByDeskproMessageId;
internal class GetMessageByDeskproMessageIdQueryHandler(IConfiguration configuration) : IRequestHandler<GetMessageByDeskproMessageIdQuery, Result<IEnumerable<Message>>>
{
    private readonly IConfiguration _configuration = configuration;

    public async Task<Result<IEnumerable<Message>>> Handle(GetMessageByDeskproMessageIdQuery request, CancellationToken cancellationToken)
    {
        var connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("Database"));

        using (var connection = new SqlConnection(connectionString))
        {
            var parameters = new
            {
                DeskproMessageId = request.DeskproMessageId
            };

            var rows = await connection.QueryAsync<Message>(Constants.SP_MESSAGE_GET_BY_DESKPRO_MESSAGE_ID,
                                                            param: parameters,
                                                            commandType: CommandType.StoredProcedure);
            return Result.Success(rows);
        }
    }
}
