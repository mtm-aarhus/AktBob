using AktBob.Shared.DataAccess;
using FluentAssertions;
using NSubstitute;
using System.Data;

namespace AktBob.OpenOrchestrator.Tests.Unit;
public class CreateQueueItemHandlerTests
{
    private readonly CreateQueueItemHandler _sut;
    private readonly IOpenOrchestratorSqlConnection _sqlConnection = Substitute.For<IOpenOrchestratorSqlConnection>();
    private readonly ISqlDataAccess<IOpenOrchestratorSqlConnection> _sqlDataAccess = Substitute.For<ISqlDataAccess<IOpenOrchestratorSqlConnection>>();
    private readonly IDbConnection _connection = Substitute.For<IDbConnection>();

    public CreateQueueItemHandlerTests()
    {
        _sqlConnection.CreateConnection().Returns(_connection);
        _sut = new CreateQueueItemHandler(_sqlDataAccess);
    }

    [Fact]
    public async Task Handle_ShouldExecuteSqlAndReturnSuccessResult_WhenRowIsInserted()
    {
        // Arrange
        var queueName = "queue name";
        var payload = "payload";
        var reference = "reference";

        _sqlDataAccess
            .Execute(
                Arg.Any<string>(),
                Arg.Any<object?>())
            .Returns(1);

        // Act
        var result = await _sut.Handle(queueName, payload, reference, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _sqlDataAccess.Received(1).Execute(
            Arg.Is("INSERT INTO Queues (id, queue_name, status, data, reference, created_date, created_by) VALUES (@Id, @QueueName, @Status, @Data, @Reference, @CreatedAt, @CreatedBy)"),
            Arg.Is<object>(arg =>
                arg.GetType().GetProperty("QueueName")!.GetValue(arg)!.Equals(queueName)
                && arg.GetType().GetProperty("Data")!.GetValue(arg)!.Equals(payload)));
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenRowIsNotInserted()
    {
        // Arrange
        var queueName = "queue name";
        var payload = "payload";
        var reference = "reference";

        _sqlDataAccess
            .Execute(
                Arg.Any<string>(),
                Arg.Any<object?>())
            .Returns(0);

        // Act
        var result = await _sut.Handle(queueName, payload, reference, CancellationToken.None);


        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}