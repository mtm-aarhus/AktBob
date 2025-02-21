using AktBob.Database.Contracts.Dtos;
using AktBob.Database.Contracts.Messages;
using Ardalis.GuardClauses;
using Ardalis.Result;
using Dapper;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AktBob.Database.UseCases.Messages;
public class UpdateMessageSetGoDocumentIdCommandHandler(IConfiguration configuration, IMediator mediator) : MediatorRequestHandler<UpdateMessageSetGoDocumentIdCommand, Result<MessageDto>>
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IMediator _mediator = mediator;

    protected override async Task<Result<MessageDto>> Handle(UpdateMessageSetGoDocumentIdCommand request, CancellationToken cancellationToken)
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
        var getMessageQueryResult = await _mediator.SendRequest(new GetMessageByDeskproMessageIdQuery(request.DeskproMessageId), cancellationToken);
        if (!getMessageQueryResult.IsSuccess)
        {
            return Result.NotFound();
        }

        return Result.Success(getMessageQueryResult.Value);
    }
}