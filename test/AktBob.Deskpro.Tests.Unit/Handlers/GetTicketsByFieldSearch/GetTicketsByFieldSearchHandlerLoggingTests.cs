using Aktbob.Modules.Deskpro.Features.GetTicketsByFieldSearch;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetTicketsByFieldSearch;
public class GetTicketsByFieldSearchHandlerLoggingTests
{
    private readonly GetTicketsByFieldSearchHandlerLogging _sut;
    private readonly IGetTicketsByFieldSearchHandler _inner = Substitute.For<IGetTicketsByFieldSearchHandler>();
    private readonly FakeLogger<GetTicketsByFieldSearchHandler> _logger = new FakeLogger<GetTicketsByFieldSearchHandler>();

    public GetTicketsByFieldSearchHandlerLoggingTests()
    {
        _sut = new GetTicketsByFieldSearchHandlerLogging(_inner, _logger);
    }


    [Fact]
    public async Task Handle_ShouldLogInformationAndReturnInnerResult_WhenInvoked()
    {
        // Arrange
        int[] fields = [1, 2, 3];
        var searchValue = "search value";

        var collection = new List<TicketDto>();
        var innerResult = ErrorOrFactory.From<IReadOnlyCollection<TicketDto>>(collection);
        var expectedResult = ErrorOrFactory.From<IReadOnlyCollection<TicketDto>>(collection);

        _inner.Handle(fields, searchValue, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(fields, searchValue, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        await _inner.Received(1).Handle(fields, searchValue, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldLogDebugAndReturnInnerResult_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        int[] fields = [1, 2, 3];
        var searchValue = "search value";

        var collection = new List<TicketDto>();
        var error = Error.Failure().ToErrorOr<IReadOnlyCollection<TicketDto>>();
        _inner.Handle(fields, searchValue, Arg.Any<CancellationToken>()).Returns(error);

        // Act
        var result = await _sut.Handle(fields, searchValue, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(error);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Warning);
        await _inner.Received(1).Handle(fields, searchValue, Arg.Any<CancellationToken>());
    }
}
