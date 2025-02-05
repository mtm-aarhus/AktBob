using AktBob.Database.Entities;
using AktBob.Database.UseCases.Messages.ClearQueuedForJournalization;
using AktBob.Database.UseCases.Messages.GetMessageById;
using Ardalis.GuardClauses;
using Ardalis.Result;
using Dapper;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AktBob.Database.UseCases.Messages.PatchMessage;
internal class PatchMessageCommandHandler(IConfiguration configuration, IMediator mediator) : MediatorRequestHandler<PatchMessageCommand, Result<Message>>
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IMediator _mediator = mediator;

    protected override async Task<Result<Message>> Handle(PatchMessageCommand request, CancellationToken cancellationToken)
    {
        var connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("Database"));

        // Get the message as it is before update
        var getMessageQuery = new GetMessageByIdQuery(request.Id);
        var getMessageQueryResult = await _mediator.SendRequest(getMessageQuery, cancellationToken);


        if (!getMessageQueryResult.IsSuccess)
        {
            // The message was not found in the datbase
            return getMessageQueryResult;
        }

        var message = getMessageQueryResult.Value;


        // Update the message
        using (var connection = new SqlConnection(connectionString))
        {
            if (request.GoDocumentId is not null)
            {
                message.GODocumentId = request.GoDocumentId;

                // Clear queued for journalization
                var clearQueuedForJournalizationCommand = new ClearQueuedForJournalizationCommand(message.Id);
                await _mediator.Send(clearQueuedForJournalizationCommand, cancellationToken);
            }

            var updateMessageParameters = new DynamicParameters();
            updateMessageParameters.Add(Constants.T_MESSAGES_ID, message.Id);
            updateMessageParameters.Add(Constants.T_MESSAGES_TICKET_ID, message.TicketId);
            updateMessageParameters.Add(Constants.T_MESSAGES_DESKPRO_ID, message.DeskproMessageId);
            updateMessageParameters.Add(Constants.T_MESSAGES_GO_DOCUMENT_ID, message.GODocumentId);

            await connection.QueryAsync(Constants.SP_MESSAGE_UPDATE, updateMessageParameters);
        }

        // Get the message again in order to return the actual updated database object
        getMessageQueryResult = await _mediator.SendRequest(getMessageQuery, cancellationToken);

        if (!getMessageQueryResult.IsSuccess)
        {
            return getMessageQueryResult;
        }

        return Result.Success(getMessageQueryResult.Value);
    }
}