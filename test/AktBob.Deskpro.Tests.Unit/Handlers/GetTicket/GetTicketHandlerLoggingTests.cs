using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers.GetTicket;
using AktBob.Shared.Types.Deskpro;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetTicket;
public class GetTicketHandlerLoggingTests
{
    private readonly GetTicketHandlerLogging _sut;
    private readonly IGetTicketHandler _inner = Substitute.For<IGetTicketHandler>();
    private readonly FakeLogger<GetTicketHandler> _logger = new FakeLogger<GetTicketHandler>();

    public GetTicketHandlerLoggingTests()
    {
        _sut = new GetTicketHandlerLogging(_inner, _logger);
    }

    [Fact]
    public async Task GetTicket_ShouldLogInformationAndReturnInnerResult_WhenInvoked()
    {
        // Arrange
        var ticketId = TicketId.Create(1);
        var ticketDto = new TicketDto
        {
            Id = ticketId
        };

        var innerResult = ErrorOrFactory.From(ticketDto);
        var expectedResult = ErrorOrFactory.From(ticketDto);

        _inner.Handle(ticketId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(ticketId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        await _inner.Received(1).Handle(ticketId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTicket_ShouldLogDebugAndReturnInnerResult_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var ticketId = TicketId.Create(1);
        var ticketDto = new TicketDto
        {
            Id = ticketId
        };

        var error = Error.Failure().ToErrorOr<TicketDto>();
        _inner.Handle(ticketId, Arg.Any<CancellationToken>()).Returns(error);

        // Act
        var result = await _sut.Handle(ticketId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(error);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Warning);
        await _inner.Received(1).Handle(ticketId, Arg.Any<CancellationToken>());
    }
}
