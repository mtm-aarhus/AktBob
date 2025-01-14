using AktBob.OpenOrchestrator.Contracts;
using Ardalis.GuardClauses;
using Ardalis.Result;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AktBob.OpenOrchestrator;

internal class CreateQueueItemCommandHandler(IConfiguration configuration, ILogger<CreateQueueItemCommandHandler> logger) : IRequestHandler<CreateQueueItemCommand, Result<Guid>>
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<CreateQueueItemCommandHandler> _logger = logger;

    async Task<Result<Guid>> IRequestHandler<CreateQueueItemCommand, Result<Guid>>.Handle(CreateQueueItemCommand command, CancellationToken cancellationToken)
    {
        var connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("OpenOrcehstratorDb"));
        Guard.Against.Null(command.Data);
        Guard.Against.NullOrEmpty(command.QueueName);

        using (var connection = new SqlConnection(connectionString))
        {
            var data = JsonSerializer.Serialize(command.Data, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            var sql = "INSERT INTO Queues (id, queue_name, status, data, reference, created_date, created_by) VALUES (@Id, @QueueName, @Status, @Data, @Reference, @CreatedAt, @CreatedBy)";
            var id = Guid.NewGuid();

            try
            {
                await connection.QueryAsync(
                    sql,
                    new
                    {
                        Id = id,
                        QueueName = command.QueueName,
                        Status = "NEW",
                        Data = data,
                        Reference = command.Reference,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "AktBob.InternalWorker"
                    },
                    commandType: System.Data.CommandType.Text);

                return Result.Success(id);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating OpenOrchestrator queue item: {message}", ex.Message);
                return Result.Error();
            }
        }
    }
}
