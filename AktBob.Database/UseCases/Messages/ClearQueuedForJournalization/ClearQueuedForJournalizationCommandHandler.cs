using AktBob.Database.UseCases.Messages.GetMessageById;
using Ardalis.GuardClauses;
using Ardalis.Result;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.Database.UseCases.Messages.ClearQueuedForJournalization;
internal class ClearQueuedForJournalizationCommandHandler : IRequestHandler<ClearQueuedForJournalizationCommand, Result>
{
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediator;
    private readonly ILogger<ClearQueuedForJournalizationCommandHandler> _logger;

    public ClearQueuedForJournalizationCommandHandler(IConfiguration configuration, IMediator mediator, ILogger<ClearQueuedForJournalizationCommandHandler> logger)
    {
        _configuration = configuration;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result> Handle(ClearQueuedForJournalizationCommand request, CancellationToken cancellationToken)
    {
        var connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("Database"));

        // Get the message as it is before update
        var getMessageQuery = new GetMessageByIdQuery(request.Id);
        var getMessageQueryResult = await _mediator.Send(getMessageQuery, cancellationToken);


        if (!getMessageQueryResult.IsSuccess)
        {
            // The message was not found in the datbase
            return Result.NotFound();
        }

        var message = getMessageQueryResult.Value;

        // Update
        try
        {

        using (var connection = new SqlConnection(connectionString))
        {
            var updateMessageParameters = new DynamicParameters();
            updateMessageParameters.Add(Constants.T_MESSAGES_ID, message.Id);
            await connection.QueryAsync(Constants.SP_MESSAGE_CLEAR_QUEUED_FOR_JOURNALIZATION, updateMessageParameters);

            return Result.Success();
        }
        }
        catch (Exception ex)
        {
            _logger.LogError("Database error when trying to clear QueuedForJournalization: {message}", ex.Message);
            return Result.Error();
        }
    }
}
