using Ardalis.GuardClauses;
using Ardalis.Result;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.OpenOrchestrator;

internal class CreateQueueItemHandler(IConfiguration configuration, ILogger<CreateQueueItemHandler> logger) : ICreateQueueItemHandler
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<CreateQueueItemHandler> _logger = logger;

    public async Task<Result<Guid>> Handle(string queueName, string payload, string reference, CancellationToken cancellationToken)
    {
        var connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("OpenOrchestratorDb"));
        Guard.Against.NullOrEmpty(queueName);

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
                        QueueName = queueName.Trim(),
                        Status = "NEW",
                        Data = payload.Trim(),
                        Reference = reference.Trim(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = $"{Environment.MachineName} AktBob.Worker".Trim()
                    },
                    commandType: System.Data.CommandType.Text);

                _logger.LogInformation("OpenOrchestrator queue item {id} created. Data: {data}", id, payload);

                return Result.Success(id);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating OpenOrchestrator queue item: {message}. Data: {data}", ex.Message, payload);
                return Result.Error();
            }
        }
    }
}