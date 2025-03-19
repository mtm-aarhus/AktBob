using AktBob.Database.DataAccess;
using AktBob.Database.Decorators;
using Dapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace AktBob.Database.Tests.Unit.Decorators;

public class SqlDataAccessLoggingDecoratorTests
{
    private readonly SqlDataAccessLoggingDecorator _sut;
    private readonly ISqlDataAccess _inner = Substitute.For<ISqlDataAccess>();
    private readonly FakeLogger<SqlDataAccess> _logger = new FakeLogger<SqlDataAccess>();

    public SqlDataAccessLoggingDecoratorTests()
    {
        _sut = new SqlDataAccessLoggingDecorator(_inner, _logger);
    }

    [Fact]
    public async Task Execute_ShouldLogInformationAndReturnResult_WhenInnerModuleAffectsMoreThanZeroRows()
    {
        // Arrange
        var parameters = new DynamicParameters();
        var sql = "some sql";
        var expectedResult = 1;

        _inner.Execute(Arg.Any<string>(), Arg.Any<DynamicParameters>()).Returns(expectedResult);

        // Act
        var result = await _sut.Execute(sql, parameters);

        // Assert
        result.Should().Be(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public async Task Execute_ShouldLogDebugAndReturnResult_WhenInnerModuleAffectsZeroRows()
    {
        // Arrange
        var parameters = new DynamicParameters();
        var sql = "some sql";
        var expectedResult = 0;

        _inner.Execute(Arg.Any<string>(), Arg.Any<DynamicParameters>()).Returns(expectedResult);

        // Act
        var result = await _sut.Execute(sql, parameters);

        // Assert
        result.Should().Be(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public async Task ExecuteProcedure_ShouldReturnResult_WhenInnerModuleAffectsMoreThanZeroRows()
    {
        // Arrange
        var parameters = new DynamicParameters();
        var sql = "some sql";
        var expectedResult = 1;

        _inner.ExecuteProcedure(Arg.Any<string>(), Arg.Any<DynamicParameters>()).Returns(expectedResult);

        // Act
        var result = await _sut.ExecuteProcedure(sql, parameters);

        // Assert
        result.Should().Be(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public async Task ExecuteProcedure_ShouldLogDebugAndReturnResult_WhenInnerModuleAffectsZeroRows()
    {
        // Arrange
        var parameters = new DynamicParameters();
        var sql = "some sql";
        var expecedResult = 0;

        _inner.ExecuteProcedure(Arg.Any<string>(), Arg.Any<DynamicParameters>()).Returns(expecedResult);

        // Act
        var result = await _sut.ExecuteProcedure(sql, parameters);

        // Assert
        result.Should().Be(expecedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public async Task Query_ShouldReturnResult_WhenInnerModuleReturnsCollectionWithItems()
    {
        // Arrange
        var parameters = new { };
        var sql = "some sql";
        var expected = new List<object>
        {
            new {}
        };

        _inner.Query<object>(Arg.Any<string>(), Arg.Any<object?>()).Returns(expected);

        // Act
        var result = await _sut.Query<object>(sql, parameters);

        // Assert
        result.Should().BeSameAs(expected);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public async Task Query_ShouldLogErrorAndRethrowException_WhenInnerModuleReturnsEmptyCollection()
    {
        // Arrange
        var parameters = new { };
        var sql = "some sql";
        var expected = new List<object>();

        _inner.Query<object>(Arg.Any<string>(), Arg.Any<object?>()).Returns(expected);

        // Act
        var result = await  _sut.Query<object>(sql, parameters);

        // Assert
        result.Should().BeSameAs(expected);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public async Task Query_WithSplit_ShouldReturnResult_WhenInnerModuleReturnsCollectionWithItems()
    {
        // Arrange
        var parameters = new { };
        var sql = "some sql";
        var splitOn = "some value";
        var expected = new List<object>
        {
            new { }
        };

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
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public async Task Query_WithSplit_ShouldLogErrorAndRethrowException_WhenInnerModuleReturnEmptyCollection()
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
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public async Task QuerySingle_ShouldReturnResult_WhenInnerModuleReturnsAnItem()
    {
        // Arrange
        var parameters = new { };
        var sql = "some sql";
        var expected = new { };

        _inner.QuerySingle<object>(Arg.Any<string>(), Arg.Any<object?>()).Returns(expected);

        // Act
        var result = await _sut.QuerySingle<object>(sql, parameters);

        // Assert
        result.Should().Be(expected);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public async Task QuerySingle_ShouldLogErrorAndRethrowException_WhenInnerModuleResultNull()
    {
        // Arrange
        var parameters = new { };
        var sql = "some sql";

        _inner.QuerySingle<object>(Arg.Any<string>(), Arg.Any<object?>()).ReturnsNull();

        // Act
        var result = await _sut.QuerySingle<object>(sql, parameters);

        // Assert
        result.Should().BeNull();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
    }
}
