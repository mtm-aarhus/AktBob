using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Contracts.Messages;
using Ardalis.GuardClauses;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AktBob.Database.UseCases.Messages;
internal class UpdateMessageSetGoDocumentIdCommandHandler(IConfiguration configuration, IQueryDispatcher queryDispatcher) : ICommandHandler<UpdateMessageSetGoDocumentIdCommand, Result<MessageDto>>
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IQueryDispatcher _queryDispatcher = queryDispatcher;

    public async Task<Result<MessageDto>> Handle(UpdateMessageSetGoDocumentIdCommand request, CancellationToken cancellationToken)
    {
        var connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("Database"));

        // Update the message
        using (var connection = new SqlConnection(connectionString))
        {
            var updateMessageParameters = new DynamicParameters();
            updateMessageParameters.Add(Constants.T_MESSAGES_DESKPRO_ID, request.DeskproMessageId);
            updateMessageParameters.Add(Constants.T_MESSAGES_GO_DOCUMENT_ID, request.GoDocumentId);

            await connection.QueryAsync(Constants.SP_MESSAGE_UPDATE, updateMessageParameters);
        }

        // Return the updated database object
        var getMessageQueryResult = await _queryDispatcher.Dispatch(new GetMessageByDeskproMessageIdQuery(request.DeskproMessageId), cancellationToken);
        if (!getMessageQueryResult.IsSuccess)
        {
            return Result.NotFound();
        }

        return Result.Success(getMessageQueryResult.Value);
    }
}