using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Collections.ObjectModel;
using Aktbob.Modules.Deskpro.Features.GetTicketsByFieldSearch;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetTicketsByFieldSearch;
public class GetTicketsByFieldSearchHandlerExceptionTests
{
    private readonly GetTicketsByFieldSearchHandlerException _sut;
    private readonly IGetTicketsByFieldSearchHandler _inner = Substitute.For<IGetTicketsByFieldSearchHandler>();
    private readonly FakeLogger<GetTicketsByFieldSearchHandler> _logger = new FakeLogger<GetTicketsByFieldSearchHandler>();

    public GetTicketsByFieldSearchHandlerExceptionTests()
    {
        _sut = new GetTicketsByFieldSearchHandlerException(_inner, _logger);
    }

    [Fact]
    public async Task GetTicketsByFieldSearch_ReturnInnerResult_WhenInnerResponseIsSuccessful()
    {
        // Arrange
        int[] fields = { 1, 2, 3 };
        var searchValue = "search value";
        var collection = new Collection<TicketDto> { new TicketDto { Id = 1 } };
        var innerResult = ErrorOrFactory.From<IReadOnlyCollection<TicketDto>>(collection);
        var expectedResult = ErrorOrFactory.From<IReadOnlyCollection<TicketDto>>(collection);
        _inner.Handle(fields, searchValue, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(fields, searchValue, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        await _inner.Received(1).Handle(fields, searchValue, Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetTicketsByFieldSearch_LogAndReturnError_WhenInnerModuleThrowsException()
    {
        // Arrange
        int[] fields = { 1, 2, 3 };
        var searchValue = "search value";
        _inner.Handle(Arg.Any<int[]>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var result = await _sut.Handle(fields, searchValue, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        await _inner.Received(1).Handle(fields, searchValue, Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }
}
