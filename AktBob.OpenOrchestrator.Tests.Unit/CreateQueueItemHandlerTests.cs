using AktBob.Shared;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AktBob.OpenOrchestrator.Tests.Unit;
public class CreateQueueItemHandlerTests
{
    private readonly CreateQueueItemHandler _sut;
    private readonly IOpenOrchestratorSqlConnection _sqlConnection = Substitute.For<IOpenOrchestratorSqlConnection>();
    private readonly ISqlExecutor<IOpenOrchestratorSqlConnection> _sqlExecutor = Substitute.For<ISqlExecutor<IOpenOrchestratorSqlConnection>>();
    private readonly IDbConnection _connection = Substitute.For<IDbConnection>();

    public CreateQueueItemHandlerTests()
    {
        _sqlConnection.CreateConnection().Returns(_connection);
        _sut = new CreateQueueItemHandler(_sqlExecutor);
    }

    [Fact]
    public async Task Handle_ShouldExecuteSqlAndReturnSuccessResult_WhenRowIsInserted()
    {
        // Arrange
        var queueName = "queue name";
        var payload = "payload";
        var reference = "reference";

        _sqlExecutor
            .ExecuteAsync(
                Arg.Any<string>(),
                Arg.Any<object?>(),
                Arg.Any<IDbTransaction?>(),
                Arg.Any<int?>(),
                Arg.Any<CommandType?>())
            .Returns(1);

        // Act
        var result = await _sut.Handle(queueName, payload, reference, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _sqlExecutor.Received(1).ExecuteAsync(
            Arg.Is("INSERT INTO Queues (id, queue_name, status, data, reference, created_date, created_by) VALUES (@Id, @QueueName, @Status, @Data, @Reference, @CreatedAt, @CreatedBy)"),
            Arg.Is<object>(arg =>
                arg.GetType().GetProperty("QueueName")!.GetValue(arg)!.Equals(queueName)
                && arg.GetType().GetProperty("Data")!.GetValue(arg)!.Equals(payload)),
            commandType: Arg.Any<CommandType>());
    }

    public async Task Handle_ShouldReturnError_WhenRowIsNotInserted()
    {
        // Arrange


        // Act


        // Assert
    }


}
