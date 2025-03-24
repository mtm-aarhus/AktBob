using AktBob.Shared.DataAccess;
using Dapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AktBob.Database.Tests.Unit.Decorators;

public class SqlDataAccessExceptionDecoratorTests
{
    private readonly SqlDataAccessExceptionDecorator<IDatabaseSqlConnectionFactory> _sut;
    private readonly ISqlDataAccess<IDatabaseSqlConnectionFactory> _inner = Substitute.For<ISqlDataAccess<IDatabaseSqlConnectionFactory>>();
    private readonly FakeLogger<SqlDataAccess<IDatabaseSqlConnectionFactory>> _logger = new FakeLogger<SqlDataAccess<IDatabaseSqlConnectionFactory>>();

    public SqlDataAccessExceptionDecoratorTests()
    {
        _sut = new SqlDataAccessExceptionDecorator<IDatabaseSqlConnectionFactory>(_inner, _logger);
    }

    [Fact]
    public async Task Execute_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var parameters = new DynamicParameters();
        var sql = "some sql";

        _inner.Execute(Arg.Any<string>(), Arg.Any<DynamicParameters>()).Returns(1);

        // Act
        var result = await _sut.Execute(sql, parameters);

        // Assert
        result.Should().BePositive();
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Execute_ShouldLogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        var parameters = new DynamicParameters();
        var sql = "some sql";

        _inner.Execute(Arg.Any<string>(), Arg.Any<DynamicParameters>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Execute(sql, parameters);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task ExecuteProcedure_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var parameters = new DynamicParameters();
        var sql = "some sql";

        _inner.ExecuteProcedure(Arg.Any<string>(), Arg.Any<DynamicParameters>()).Returns(1);

        // Act
        var result = await _sut.ExecuteProcedure(sql, parameters);

        // Assert
        result.Should().BePositive();
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteProcedure_ShouldLogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        var parameters = new DynamicParameters();
        var sql = "some sql";

        _inner.ExecuteProcedure(Arg.Any<string>(), Arg.Any<DynamicParameters>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.ExecuteProcedure(sql, parameters);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task Query_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var parameters = new {};
        var sql = "some sql";
        var expected = new List<object>();

        _inner.Query<object>(Arg.Any<string>(), Arg.Any<object?>()).Returns(expected);

        // Act
        var result = await _sut.Query<object>(sql, parameters);

        // Assert
        result.Should().BeSameAs(expected);
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Query_ShouldLogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        var parameters = new {};
        var sql = "some sql";

        _inner.Query<object>(Arg.Any<string>(), Arg.Any<object?>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Query<object>(sql, parameters);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task Query_WithSplit_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var parameters = new { };
        var sql = "some sql";
        var splitOn = "some value";

        var expected = new List<object>();

        _inner
            .Query(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<string>(),
                Arg.Any<Func<object, object, object>>())
            .Returns(expected);

        // Act
        var result = await _sut.Query<object, object>(sql, parameters, splitOn, (obj1, obj2) => { return true; });

        // Assert
        result.Should().BeSameAs(expected);
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Query_WithSplit_ShouldLogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        var parameters = new { };
        var sql = "some sql";
        var splitOn = "some value";

        _inner
            .Query(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<string>(),
                Arg.Any<Func<object, object, object>>())
            .ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Query<object, object>(sql, parameters, splitOn, (obj1, obj2) => { return true; });

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task QuerySingle_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var parameters = new { };
        var sql = "some sql";
        var expected = new {};

        _inner.QuerySingle<object>(Arg.Any<string>(), Arg.Any<object?>()).Returns(expected);

        // Act
        var result = await _sut.QuerySingle<object>(sql, parameters);

        // Assert
        result.Should().Be(expected);
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task QuerySingle_ShouldLogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        var parameters = new { };
        var sql = "some sql";

        _inner.QuerySingle<object>(Arg.Any<string>(), Arg.Any<object?>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.QuerySingle<object>(sql, parameters);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }
}
