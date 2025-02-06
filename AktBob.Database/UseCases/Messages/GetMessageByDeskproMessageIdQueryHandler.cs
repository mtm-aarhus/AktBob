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
public class GetMessageByDeskproMessageIdQueryHandler(IConfiguration configuration) : MediatorRequestHandler<GetMessageByDeskproMessageIdQuery, Result<MessageDto>>
{
    private readonly IConfiguration _configuration = configuration;

    protected override async Task<Result<MessageDto>> Handle(GetMessageByDeskproMessageIdQuery request, CancellationToken cancellationToken)
    {
        var connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("Database"));

        using (var connection = new SqlConnection(connectionString))
        {
            var parameters = new
            {
                request.DeskproMessageId
            };

            var messages = await connection.QueryAsync<Message>(
                Constants.SP_MESSAGE_GET_BY_DESKPRO_MESSAGE_ID,
                param: parameters,
                commandType: CommandType.StoredProcedure);

            var message = messages.FirstOrDefault()?.ToDto();
            if (messages is not null && messages.Count() > 0)
            {
                return Result.Success(messages.First().ToDto());
            }

            return Result.NotFound();
        }
    }
}
