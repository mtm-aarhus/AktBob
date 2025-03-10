using Ardalis.GuardClauses;
using Ardalis.Result;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AktBob.OpenOrchestrator;

internal class CreateQueueItemHandler(IConfiguration configuration) : ICreateQueueItemHandler
{
    private readonly IConfiguration _configuration = configuration;

    public async Task<Result<Guid>> Handle(string queueName, string payload, string reference, CancellationToken cancellationToken)
    {
        var connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("OpenOrchestratorDb"));
        Guard.Against.NullOrEmpty(queueName);

        using (var connection = new SqlConnection(connectionString))
        {
            var sql = "INSERT INTO Queues (id, queue_name, status, data, reference, created_date, created_by) VALUES (@Id, @QueueName, @Status, @Data, @Reference, @CreatedAt, @CreatedBy)";
            var id = Guid.NewGuid();

            var rowsAffected = await connection.ExecuteAsync(
                sql,
                new
                {
                    Id = id,
                    QueueName = queueName.Trim().Substring(0, 100),
                    Status = "NEW",
                    Data = payload.Trim().Substring(0, 2000),
                    Reference = reference.Trim().Substring(0, 100),
                    CreatedAt = DateTime.Now,
                    CreatedBy = $"{Environment.MachineName} AktBob.Worker".Trim().Substring(0, 100)
                },
                commandType: System.Data.CommandType.Text);

            if (rowsAffected == 0)
            {
                return Result.Error($"Error creating OpenOrchestrator queue item: No rows in database affected. {payload}");
            }

            return Result.Success(id);
        }
    }
}