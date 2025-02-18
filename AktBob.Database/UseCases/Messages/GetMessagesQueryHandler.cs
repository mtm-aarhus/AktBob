//using AktBob.Database.Contracts.Dtos;
//using AktBob.Database.Contracts.Messages;
//using AktBob.Database.Entities;
//using AktBob.Database.Extensions;
//using Ardalis.GuardClauses;
//using Ardalis.Result;
//using Dapper;
//using MassTransit.Mediator;
//using Microsoft.Data.SqlClient;
//using Microsoft.Extensions.Configuration;
//using System.Data;

//namespace AktBob.Database.UseCases.Messages;
//public class GetMessagesQueryHandler(IConfiguration configuration) : MediatorRequestHandler<GetMessagesQuery, Result<IEnumerable<MessageDto>>>
//{
//    private readonly IConfiguration _configuration = configuration;

//    protected override async Task<Result<IEnumerable<MessageDto>>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
//    {
//        var connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("Database"));

//        using (var connection = new SqlConnection(connectionString))
//        {
//            var rows = await connection.QueryAsync<Message>(Constants.SP_MESSAGE_GET_ALL, commandType: CommandType.StoredProcedure);
//            return Result.Success(rows.Select(x => x.ToDto()));
//        }
//    }
//}
