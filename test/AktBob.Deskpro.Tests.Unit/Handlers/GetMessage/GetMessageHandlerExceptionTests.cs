using Aktbob.Modules.Deskpro.Features.GetMessage;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetMessage;
public class GetMessageHandlerExceptionTests
{
    private readonly GetMessageHandlerException _sut;
    private readonly IGetMessageHandler _inner = Substitute.For<IGetMessageHandler>();
    private readonly FakeLogger<GetMessageHandler> _logger = new FakeLogger<GetMessageHandler>();

    public GetMessageHandlerExceptionTests()
    {
        _sut = new GetMessageHandlerException(_inner, _logger);
    }

    [Fact]
    public async Task Handle_ReturnInnerResult_WhenInnerResponseIsSuccessful()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var dto = new MessageDto();
        var innerResult = ErrorOrFactory.From(dto);
        var expectedResult = ErrorOrFactory.From(dto);

        _inner.Handle(ticketId, messageId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(ticketId, messageId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        await _inner.Received(1).Handle(ticketId, messageId, Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_LogAndReturnError_WhenInnerModuleThrowsException()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        _inner.Handle(ticketId, messageId, Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var result = await _sut.Handle(ticketId, messageId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        await _inner.Received(1).Handle(ticketId, messageId, Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }
}
