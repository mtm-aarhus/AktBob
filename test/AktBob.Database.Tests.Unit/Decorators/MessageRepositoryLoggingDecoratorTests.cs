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

public class MessageRepositoryLoggingDecoratorTests
{
    private readonly MessageRepositoryLoggingDecorator _sut;
    private readonly IMessageRepository _inner = Substitute.For<IMessageRepository>();
    private readonly FakeLogger<MessageRepository> _logger = new FakeLogger<MessageRepository>();
    
    public MessageRepositoryLoggingDecoratorTests()
    {
        _sut = new MessageRepositoryLoggingDecorator(_inner, _logger);
    }

    [Fact]
    public async Task Add_ShouldLogInformationAndReturnResult_WhenInvoked()
    {
        // Arrange
        var message = new Message();
        _inner.Add(Arg.Any<Message>()).Returns(true);

        // Act
        var result = await _sut.Add(message);

        // Assert
        result.Should().BeTrue();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public async Task Add_ShouldLogDebugAndReturnResult_WhenInnerModuleIsNotSuccesful()
    {
        // Arrange
        var message = new Message();
        _inner.Add(Arg.Any<Message>()).Returns(false);

        // Act
        var result = await _sut.Add(message);

        // Assert
        result.Should().BeFalse();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public async Task Delete_ShouldLogInformationAndReturnResult_WhenInvoked()
    {
        // Arrange
        _inner.Delete(Arg.Any<int>()).Returns(true);

        // Act
        var result = await _sut.Delete(1);

        // Assert
        result.Should().BeTrue();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public async Task Delete_ShouldLogDebugAndReturnResult_WhenInnerModuleIsNotSuccesful()
    {
        // Arrange
        _inner.Delete(Arg.Any<int>()).Returns(false);

        // Act
        var result = await _sut.Delete(1);

        // Assert
        result.Should().BeFalse();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public async Task GetByDeskproMessageId_ShouldLogInformationAndReturnResult_WhenInvoked()
    {
        // Arrange
        var expected = new Message();
        _inner.GetByDeskproMessageId(Arg.Any<int>()).Returns(expected);

        // Act
        var result = await _sut.GetByDeskproMessageId(1);

        // Assert
        result.Should().Be(expected);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public async Task GetByDeskproMessageId_ShouldLogDebugAndReturnResult_WhenInnerModuleIsNotSuccesful()
    {
        // Arrange
        _inner.GetByDeskproMessageId(Arg.Any<int>()).ReturnsNull();

        // Act
        var result = await _sut.GetByDeskproMessageId(1);

        // Assert
        result.Should().BeNull();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public async Task Get_ShouldLogInformationAndReturnResult_WhenInvoked()
    {
        // Arrange
        var expected = new Message();
        _inner.Get(Arg.Any<int>()).Returns(expected);

        // Act
        var result = await _sut.Get(1);

        // Assert
        result.Should().Be(expected);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public async Task Get_ShouldLogDebugAndReturnResult_WhenInnerModuleIsNotSuccesful()
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
    public async Task Update_ShouldLogInformationAndReturnResult_WhenInvoked()
    {
        // Arrange
        var message = new Message();
        _inner.Update(Arg.Any<Message>()).Returns(true);

        // Act
        var result = await _sut.Update(message);

        // Assert
        result.Should().BeTrue();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public async Task Update_ShouldLogDebugAndReturnResult_WhenInnerModuleIsNotSuccesful()
    {
        // Arrange
        var message = new Message();
        _inner.Update(Arg.Any<Message>()).Returns(false);

        // Act
        var result = await _sut.Update(message);

        // Assert
        result.Should().BeFalse();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
    }
}
