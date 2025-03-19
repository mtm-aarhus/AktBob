using AktBob.Database.Contracts;
using AktBob.Database.Decorators;
using AktBob.Database.Entities;
using AktBob.Database.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace AktBob.Database.Tests.Unit.Decorators;

public class CaseRepositoryLoggingDecoratorTests
{
    private readonly CaseRepositoryLoggingDecorator _sut;
    private readonly ICaseRepository _inner = Substitute.For<ICaseRepository>();
    private readonly FakeLogger<CaseRepository> _logger = new FakeLogger<CaseRepository>();

    public CaseRepositoryLoggingDecoratorTests()
    {
        _sut = new CaseRepositoryLoggingDecorator(_inner, _logger);
    }

    [Fact]
    public async Task Add_ShouldLogInformationAndReturnResult_WhenInvoked()
    {
        // Arrange
        var @case = new Case();
        _inner.Add(Arg.Any<Case>()).Returns(true);

        // Act
        var result = await _sut.Add(@case);

        // Assert
        result.Should().BeTrue();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public async Task Add_ShouldLogDebugAndReturnResult_WhenInnerModuleIsNotSuccesful()
    {
        // Arrange
        var @case = new Case();
        _inner.Add(Arg.Any<Case>()).Returns(false);

        // Act
        var result = await _sut.Add(@case);

        // Assert
        result.Should().BeFalse();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
    }


    [Fact]
    public async Task Get_ShouldLogInformationAndReturnResult_WhenInvoked()
    {
        // Arrange
        var expectedCase = new Case();
        _inner.Get(Arg.Any<int>()).Returns(expectedCase);

        // Act
        var result = await _sut.Get(1);

        // Assert
        result.Should().Be(expectedCase); ;
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public async Task Get_ShouldLogDebugAndReturnResult_WhenInnerModuleReturnsNull()
    {
        // Arrange
        _inner.Get(Arg.Any<int>()).ReturnsNull();

        // Act
        var result = await _sut.Get(1);

        // Assert
        result.Should().BeNull();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public async Task GetAll_ShouldLogInformationAndReturnResult_WhenInvoked()
    {
        // Arrange
        var expected = new List<Case>
        {
            new Case()
        };

        _inner.GetAll(Arg.Any<long?>(), Arg.Any<Guid?>()).Returns(expected);

        // Act
        var result = await _sut.GetAll(null, null);

        // Assert
        result.Should().BeEquivalentTo(expected);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }


    [Theory]
    [InlineData(null, null)]
    [InlineData(12312312312, null)]
    [InlineData(null, "6BD12A48-9B1F-4869-A3E5-87F0748C5180")]
    [InlineData(12312312312, "6BD12A48-9B1F-4869-A3E5-87F0748C5180")]
    public async Task GetAll_ShouldLogDebugAndReturnResult_WhenInnerReturnsEmptyCollection(long? podioItemId, string? filArkivCaseId)
    {
        // Arrange
        var expected = new List<Case>();
        _inner.GetAll(Arg.Any<long?>(), Arg.Any<Guid?>()).Returns(expected);

        // Act
        Guid? parsedFilArkivCaseId = !string.IsNullOrEmpty(filArkivCaseId) ? Guid.Parse(filArkivCaseId) : null;
        var result = await _sut.GetAll(podioItemId, parsedFilArkivCaseId);

        // Assert
        result.Should().BeEquivalentTo(expected);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public async Task Update_ShouldLogInformationAndReturnResult_WhenInvoked()
    {
        // Arrange
        var @case = new Case();
        _inner.Update(Arg.Any<Case>()).Returns(true);

        // Act
        var result = await _sut.Update(@case);

        // Assert
        result.Should().BeTrue();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public async Task Update_ShouldLogDebugAndReturnResult_WhenInnerModuleIsNotSuccesful()
    {
        // Arrange
        var @case = new Case();
        _inner.Update(Arg.Any<Case>()).Returns(false);

        // Act
        var result = await _sut.Update(@case);

        // Assert
        result.Should().BeFalse();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
    }
}
