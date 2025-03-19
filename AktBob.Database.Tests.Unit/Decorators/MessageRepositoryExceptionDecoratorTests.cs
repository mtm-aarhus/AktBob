using AktBob.Database.Contracts;
using AktBob.Database.Decorators;
using AktBob.Database.Entities;
using AktBob.Database.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AktBob.Database.Tests.Unit.Decorators;

public class MessageRepositoryExceptionDecoratorTests
{
    private readonly MessageRepositoryExceptionDecorator _sut;
    private readonly IMessageRepository _inner = Substitute.For<IMessageRepository>();
    private readonly FakeLogger<MessageRepository> _logger = new FakeLogger<MessageRepository>();

    public MessageRepositoryExceptionDecoratorTests()
    {
        _sut = new MessageRepositoryExceptionDecorator(_inner, _logger);
    }

    [Fact]
    public async Task Add_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var @case = new Message();
        _inner
            .Add(Arg.Any<Message>())
            .Returns(true);

        // Act
        var result = await _sut.Add(@case);

        // Assert
        result.Should().Be(true);
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Add_ShouldLogErrorAndRethrowException_WhenInnerModuleFails()
    {
        // Arrange
        var @case = new Message();
        _inner
            .Add(Arg.Any<Message>())
            .ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Add(@case);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task Delete_ShouldReturnResult_WhenInnerModuleSuceeds()
    {
        // Arrange
        _inner.Delete(Arg.Any<int>()).Returns(true);

        // Act
        var result = await _sut.Delete(1);

        // Assert
        result.Should().BeTrue();
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Delete_ShouldLogErrorAndRethrowException_WhenInnerModuleFails()
    {
        // Arrange
        _inner.Delete(Arg.Any<int>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Delete(1);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }


    [Fact]
    public async Task Get_ShouldReturnResult_WhenInnerModuleSuceeds()
    {
        // Arrange
        var expectedMessage = new Message();
        _inner.Get(Arg.Any<int>()).Returns(expectedMessage);

        // Act
        var result = await _sut.Get(1);

        // Assert
        result.Should().Be(expectedMessage);
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Get_ShouldLogErrorAndRethrowException_WhenInnerModuleFails()
    {
        // Arrange
        _inner.Get(Arg.Any<int>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Get(1);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task GetByDeskproMessageId_ShouldReturnResult_WhenInnerModuleSuceeds()
    {
        // Arrange
        var expectedMessage = new Message();
        _inner.GetByDeskproMessageId(Arg.Any<int>()).Returns(expectedMessage);

        // Act
        var result = await _sut.GetByDeskproMessageId(1);

        // Assert
        result.Should().Be(expectedMessage);
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetByDeskproMessageId_ShouldLogErrorAndRethrowException_WhenInnerModuleFails()
    {
        // Arrange
        _inner.GetByDeskproMessageId(Arg.Any<int>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.GetByDeskproMessageId(1);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task Update_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var message = new Message();

        _inner
            .Update(Arg.Any<Message>())
            .Returns(true);

        // Act
        var result = await _sut.Update(message);

        // Assert
        result.Should().BeTrue();
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Update_ShouldLogErrorAndRethrowException_WhenInnerModuleFails()
    {
        // Arrange
        var message = new Message();

        _inner
            .Update(Arg.Any<Message>())
            .ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Update(message);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }
}
