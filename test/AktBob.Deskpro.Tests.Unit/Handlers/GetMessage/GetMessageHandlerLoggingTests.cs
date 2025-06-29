using Aktbob.Modules.Deskpro.Features.GetMessage;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetMessage;
public class GetMessageHandlerLoggingTests
{
    private readonly GetMessageHandlerLogging _sut;
    private readonly IGetMessageHandler _inner = Substitute.For<IGetMessageHandler>();
    private readonly FakeLogger<GetMessageHandler> _logger = new FakeLogger<GetMessageHandler>();

    public GetMessageHandlerLoggingTests()
    {
        _sut = new GetMessageHandlerLogging(_inner, _logger);
    }

    [Fact]
    public async Task Handle_ShouldLogInformationAndResultReturnInnerResult_WhenInvoked()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var innerResult = ErrorOrFactory.From(new MessageDto());
        var expectedResult = ErrorOrFactory.From(new MessageDto());
        _inner.Handle(ticketId, messageId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(ticketId, messageId, CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(expectedResult.Value);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        await _inner.Handle(ticketId, messageId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldLogDebugAndReturnInnerResult_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var error = Error.Failure().ToErrorOr<MessageDto>();
        _inner.Handle(ticketId, messageId, Arg.Any<CancellationToken>()).Returns(error);

        // Act
        var result = await _sut.Handle(ticketId, messageId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(error);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Warning);
        await _inner.Handle(ticketId, messageId, Arg.Any<CancellationToken>());
    }
}
