using AktBob.OpenOrchestrator.Contracts;
using Ardalis.GuardClauses;
using Ardalis.Result;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.OpenOrchestrator;

internal class CreateQueueItemCommandHandler(IConfiguration configuration, ILogger<CreateQueueItemCommandHandler> logger) : IRequestHandler<CreateQueueItemCommand, Result<Guid>>
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<CreateQueueItemCommandHandler> _logger = logger;

    public async Task<Result<Guid>> Handle(CreateQueueItemCommand command, CancellationToken cancellationToken)
    {
        var connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("OpenOrchestratorDb"));
        Guard.Against.NullOrEmpty(command.QueueName);

        using (var connection = new SqlConnection(connectionString))
        {
            var sql = "INSERT INTO Queues (id, queue_name, status, data, reference, created_date, created_by) VALUES (@Id, @QueueName, @Status, @Data, @Reference, @CreatedAt, @CreatedBy)";
            var id = Guid.NewGuid();

            try
            {
                await connection.QueryAsync(
                    sql,
                    new
                    {
                        Id = id,
                        QueueName = command.QueueName.Trim(),
                        Status = "NEW",
                        Data = command.Payload.Trim(),
                        Reference = command.Reference.Trim(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = $"{Environment.MachineName} AktBob.Worker".Trim()
                    },
                    commandType: System.Data.CommandType.Text);

                _logger.LogInformation("OpenOrchestrator queue item {id} created. Data: {data}", id, command.Payload);

                return Result.Success(id);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating OpenOrchestrator queue item: {message}. Data: {data}", ex.Message, command.Payload);
                return Result.Error();
            }
        }
    }
}