using AktBob.Shared;
using Ardalis.GuardClauses;
using Ardalis.Result;
using Dapper;

namespace AktBob.OpenOrchestrator;

internal class CreateQueueItemHandler(ISqlConnectionFactory sqlConnectionFactory) : ICreateQueueItemHandler
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<Result<Guid>> Handle(string queueName, string payload, string reference, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrEmpty(queueName);

        using var connection = _sqlConnectionFactory.CreateConnection();
        var sql = "INSERT INTO Queues (id, queue_name, status, data, reference, created_date, created_by) VALUES (@Id, @QueueName, @Status, @Data, @Reference, @CreatedAt, @CreatedBy)";
        var id = Guid.NewGuid();

        var rowsAffected = await connection.ExecuteAsync(
            sql,
            new
            {
                Id = id,
                QueueName = queueName.Trim().Length > 100 ? queueName.Trim().Substring(0, 100) : queueName.Trim(),
                Status = "NEW",
                Data = payload.Trim().Length > 2000 ? payload.Trim().Substring(0, 2000) : payload.Trim(),
                Reference = reference.Trim().Length > 100 ? reference.Trim().Substring(0, 100) : reference.Trim(),
                CreatedAt = DateTime.Now,
                CreatedBy = $"{Environment.MachineName} AktBob.Worker"
            },
            commandType: System.Data.CommandType.Text);

        if (rowsAffected == 0)
        {
            return Result.Error($"Error creating OpenOrchestrator queue item: No rows in database affected. {payload}");
        }

        return Result.Success(id);
    }
}