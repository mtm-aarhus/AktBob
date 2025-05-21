using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers.GetMessages;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetMessages;
public class GetMessagesHandlerLoggingTests
{
    private readonly GetMessagesHandlerLogging _sut;
    private readonly IGetMessagesHandler _inner = Substitute.For<IGetMessagesHandler>();
    private readonly FakeLogger<GetMessagesHandler> _logger = new FakeLogger<GetMessagesHandler>();

    public GetMessagesHandlerLoggingTests()
    {
        _sut = new GetMessagesHandlerLogging(_inner, _logger);
    }

    [Fact]
    public async Task Handle_ShouldLogInformationAndReturnInnerResult_WhenInvoked()
    {
        // Arrange
        var ticketId = 1;
        var error = ErrorOrFactory.From<IReadOnlyCollection<MessageDto>>(new List<MessageDto>());
        _inner.Handle(ticketId, CancellationToken.None).Returns(error);

        // Act
        var result = await _sut.Handle(ticketId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(error);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        await _inner.Received(1).Handle(ticketId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldLogDebugAndReturnInnerResult_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var ticketId = 1;
        var error = Error.Failure().ToErrorOr<IReadOnlyCollection<MessageDto>>();
        _inner.Handle(ticketId, CancellationToken.None).Returns(error);

        // Act
        var result = await _sut.Handle(ticketId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(error);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Warning);
        await _inner.Received(1).Handle(ticketId, Arg.Any<CancellationToken>());
    }

}
