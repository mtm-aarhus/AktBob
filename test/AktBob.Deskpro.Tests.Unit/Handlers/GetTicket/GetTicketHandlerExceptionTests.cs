using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers.GetTicket;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetTicket;
public class GetTicketHandlerExceptionTests
{
    private readonly GetTicketHandlerException _sut;
    private readonly IGetTicketHandler _inner = Substitute.For<IGetTicketHandler>();
    private readonly FakeLogger<GetTicketHandler> _logger = new FakeLogger<GetTicketHandler>();

    public GetTicketHandlerExceptionTests()
    {
        _sut = new GetTicketHandlerException(_inner, _logger);
    }

    [Fact]
    public async Task GetTicket_ReturnInnerResult_WhenInnerResponseIsSuccessful()
    {
        // Arrange
        var ticketId = 1;
        var ticketDto = new TicketDto { Id = ticketId };
        var innerResult = ErrorOrFactory.From(ticketDto);
        var expectedResult = ErrorOrFactory.From(ticketDto);
        _inner.Handle(ticketId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(1, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        await _inner.Received(1).Handle(ticketId, Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetTicket_LogAndReturnError_WhenInnerModuleThrowsException()
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
