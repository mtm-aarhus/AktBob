using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers.GetMessages;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Collections.ObjectModel;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetMessages;
public class GetMessagesHandlerExceptionTests
{
    private readonly GetMessagesHandlerException _sut;
    private readonly IGetMessagesHandler _inner = Substitute.For<IGetMessagesHandler>();
    private readonly FakeLogger<GetMessagesHandler> _logger = new FakeLogger<GetMessagesHandler>();

    public GetMessagesHandlerExceptionTests()
    {
        _sut = new GetMessagesHandlerException(_inner, _logger);
    }

    [Fact]
    public async Task Handle_ReturnInnerResult_WhenInnerResponseIsSuccessful()
    {
        // Arrange
        var ticketId = 1;
        var collection = new Collection<MessageDto>();
        var innerResult = ErrorOrFactory.From<IReadOnlyCollection<MessageDto>>(collection);
        var expectedResult = ErrorOrFactory.From<IReadOnlyCollection<MessageDto>>(collection);
        _inner.Handle(ticketId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(ticketId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        await _inner.Received(1).Handle(ticketId, Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_LogAndReturnError_WhenInnerModuleThrowsException()
    {
        // Arrange
        var ticketId = 1;
        _inner.Handle(ticketId, Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var result = await _sut.Handle(ticketId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        await _inner.Received(1).Handle(ticketId, Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }
}
